using StagWare.FanControl.Configurations;
using StagWare.FanControl.Plugins;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace StagWare.FanControl
{
    public class FanControl : IDisposable
    {
        #region Constants

#if DEBUG

        private const int MinPollInterval = 0;

#else

        private const int MinPollInterval = 100;

#endif

        public const int EcTimeout = 200;
        public const int MaxWaitHandleTimeout = 500;
        public const int DefaultPollInterval = 3000;
        public const string PluginsFolderDefaultName = "Plugins";
        public const int AutoFanSpeedPercentage = 101;

        #endregion

        #region Private Fields

        private AutoResetEvent autoEvent;
        private Timer timer;
        private AsyncOperation asyncOp;

        private readonly int pollInterval;
        private readonly int waitHandleTimeout;
        private readonly FanControlConfigV2 config;

        private readonly ITemperatureFilter tempFilter;
        private readonly ITemperatureMonitor tempMon;
        private readonly IEmbeddedController ec;
        private readonly FanSpeedManager[] fanSpeedManagers;

        private FanInformation[] fanInfo;
        private FanInformation[] fanInfoInternal;

        private volatile float temperature;
        private volatile float[] requestedSpeeds;

        #endregion

        #region Constructor

        public FanControl(FanControlConfigV2 config)
            : this(config, PluginsDirectory)
        {
        }

        public FanControl(FanControlConfigV2 config, string pluginsPath) :
            this(
            config,
            new ArithmeticMeanTemperatureFilter(Math.Max(config.EcPollInterval, MinPollInterval)),
            pluginsPath)
        { }

        public FanControl(FanControlConfigV2 config, ITemperatureFilter tempFilter, string pluginsPath)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (tempFilter == null)
            {
                throw new ArgumentNullException("filter");
            }

            if (pluginsPath == null)
            {
                throw new ArgumentNullException("pluginsPath");
            }

            if (!Directory.Exists(pluginsPath))
            {
                throw new DirectoryNotFoundException(pluginsPath + " could not be found.");
            }

            var ecLoader = new FanControlPluginLoader<IEmbeddedController>(pluginsPath);
            this.ec = ecLoader.FanControlPlugin;
            this.EmbeddedControllerPluginId = ecLoader.FanControlPluginId;

            if (this.ec == null)
            {
                throw new PlatformNotSupportedException("Could not load a compatible EC plugin.");
            }

            var tempMonloader = new FanControlPluginLoader<ITemperatureMonitor>(pluginsPath);
            this.tempMon = tempMonloader.FanControlPlugin;
            this.TemperatureMonitorPluginId = tempMonloader.FanControlPluginId;

            if (this.tempMon == null)
            {
                throw new PlatformNotSupportedException("Could not load a  compatible temperature monitoring plugin");
            }

            this.autoEvent = new AutoResetEvent(false);
            this.tempFilter = tempFilter;
            this.config = (FanControlConfigV2)config.Clone();
            this.pollInterval = config.EcPollInterval;
            this.waitHandleTimeout = Math.Min(MaxWaitHandleTimeout, config.EcPollInterval);
            this.requestedSpeeds = new float[config.FanConfigurations.Count];
            this.fanInfo = new FanInformation[config.FanConfigurations.Count];
            this.fanInfoInternal = new FanInformation[config.FanConfigurations.Count];
            this.fanSpeedManagers = new FanSpeedManager[config.FanConfigurations.Count];
            this.asyncOp = AsyncOperationManager.CreateOperation(null);

            for (int i = 0; i < config.FanConfigurations.Count; i++)
            {
                var cfg = config.FanConfigurations[i];

                this.fanSpeedManagers[i] = new FanSpeedManager(cfg, config.CriticalTemperature);
                this.requestedSpeeds[i] = AutoFanSpeedPercentage;
                this.fanInfo[i] = new FanInformation(0, 0, true, false, cfg.FanDisplayName);
            }
        }

        #endregion

        #region Events

        public event EventHandler EcUpdated;

        #endregion

        #region Properties

        public static string PluginsDirectory
        {
            get
            {
                return Path.Combine(AssemblyDirectory, PluginsFolderDefaultName);
            }
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public string TemperatureMonitorPluginId { get; private set; }
        public string EmbeddedControllerPluginId { get; private set; }

        public float Temperature
        {
            get { return this.temperature; }
        }

        public bool Enabled
        {
            get { return this.timer != null; }
        }

        public string TemperatureSourceDisplayName
        {
            get
            {
                if (this.tempMon == null || !this.tempMon.IsInitialized)
                {
                    return null;
                }
                else
                {
                    return this.tempMon.TemperatureSourceDisplayName;
                }
            }
        }

        public ReadOnlyCollection<FanInformation> FanInformation
        {
            get
            {
                return Array.AsReadOnly(fanInfo);
            }
        }

        #endregion

        #region Public Methods

        public void Start(int delay = 0)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(null);
            }

            if (!this.Enabled)
            {
                if (!this.tempMon.IsInitialized)
                {
                    this.tempMon.Initialize();
                }

                if (!this.ec.IsInitialized)
                {
                    this.ec.Initialize();
                }

                this.autoEvent.Set();
                InitializeRegisterWriteConfigurations();

                if (this.timer == null)
                {
                    this.timer = new Timer(new TimerCallback(TimerCallback), null, delay, this.pollInterval);
                }
            }
        }

        public void SetTargetFanSpeed(float speed, int fanIndex)
        {
            if (fanIndex >= 0 && fanIndex < this.requestedSpeeds.Length)
            {
                Thread.VolatileWrite(ref this.requestedSpeeds[fanIndex], speed);

                if (this.Enabled)
                {
                    ThreadPool.QueueUserWorkItem(TimerCallback, null);
                }
            }
            else
            {
                throw new IndexOutOfRangeException("fanIndex");
            }
        }

        public void Stop()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(null);
            }
            else
            {
                StopFanControlCore();
            }
        }

        public void ReInitialize()
        {
            if (this.Enabled)
            {
                InitializeRegisterWriteConfigurations();
            }
        }

        #endregion

        #region Protected Methods

        protected void OnEcUpdated()
        {
            if (EcUpdated != null)
            {
                EcUpdated(this, new EventArgs());
            }
        }

        #endregion

        #region Private Methods

        #region Update EC

        private void TimerCallback(object state)
        {
            if (this.autoEvent.WaitOne(this.waitHandleTimeout))
            {
                try
                {
                    // Read CPU temperature before the call to UpdateEc(),
                    // because both methods try to aquire ISA bus mutex
                    this.temperature = (float)GetTemperature();
                    UpdateEc();
                }
                finally
                {
                    this.autoEvent.Set();
                }
            }
        }

        private void UpdateEc()
        {
            if (this.ec.AcquireLock(EcTimeout))
            {
                try
                {
                    ApplyRegisterWriteConfigurations();
                    UpdateAndApplyFanSpeeds();
                }
                finally
                {
                    this.ec.ReleaseLock();
                }

                asyncOp.Post((object args) => OnEcUpdated(), null);
            }
        }

        private void StopFanControlCore()
        {
            if (this.Enabled)
            {
                if (this.autoEvent != null && !this.autoEvent.SafeWaitHandle.IsClosed)
                {
                    this.autoEvent.Reset();
                }

                if (timer != null)
                {
                    using (var handle = new EventWaitHandle(false, EventResetMode.ManualReset))
                    {
                        timer.Dispose(handle);

                        if (handle.WaitOne())
                        {
                            timer = null;
                        }
                    }
                }

                ResetEc();
            }
        }

        private void UpdateAndApplyFanSpeeds()
        {
            for (int i = 0; i < this.fanSpeedManagers.Length; i++)
            {
                float speed = Thread.VolatileRead(ref this.requestedSpeeds[i]);
                this.fanSpeedManagers[i].UpdateFanSpeed(speed, this.temperature);

                WriteValue(
                    this.config.FanConfigurations[i].WriteRegister,
                    this.fanSpeedManagers[i].FanSpeedValue,
                    this.config.ReadWriteWords);
            }

            UpdateFanInformation();
        }

        private void UpdateFanInformation()
        {
            for (int i = 0; i < this.fanSpeedManagers.Length; i++)
            {
                FanSpeedManager speedMan = this.fanSpeedManagers[i];
                FanConfiguration fanCfg = this.config.FanConfigurations[i];
               
                int speedVal = GetFanSpeedValue(
                    fanCfg.ReadRegister, 
                    speedMan.MinSpeedValueReadAbs, 
                    speedMan.MaxSpeedValueReadAbs, 
                    this.config.ReadWriteWords);

                this.fanInfoInternal[i] = new FanInformation(
                    speedMan.FanSpeedPercentage,
                    speedMan.FanSpeedToPercentage(speedVal),
                    speedMan.AutoControlEnabled,
                    speedMan.CriticalModeEnabled,
                    fanCfg.FanDisplayName);
            }

            this.fanInfoInternal = Interlocked.Exchange(ref this.fanInfo, this.fanInfoInternal);
        }

        private void InitializeRegisterWriteConfigurations()
        {
            try
            {
                if (this.autoEvent.WaitOne(waitHandleTimeout) && this.ec.AcquireLock(EcTimeout))
                {
                    try
                    {
                        ApplyRegisterWriteConfigurations(true);
                    }
                    finally
                    {
                        this.ec.ReleaseLock();
                    }
                }
                else
                {
                    throw new TimeoutException("EC initialization timed out.");
                }
            }
            finally
            {
                this.autoEvent.Set();
            }
        }

        private void ApplyRegisterWriteConfigurations(bool initializing = false)
        {
            if (this.config.RegisterWriteConfigurations != null)
            {
                foreach (RegisterWriteConfiguration cfg in this.config.RegisterWriteConfigurations)
                {
                    if (initializing || cfg.WriteOccasion == RegisterWriteOccasion.OnWriteFanSpeed)
                    {
                        ApplyRegisterWriteConfig(cfg.Value, cfg.Register, cfg.WriteMode);
                    }
                }
            }
        }

        private void ApplyRegisterWriteConfig(int value, int register, RegisterWriteMode mode)
        {
            if (mode == RegisterWriteMode.And)
            {
                value &= ReadValue(register, false);
            }
            else if (mode == RegisterWriteMode.Or)
            {
                value |= ReadValue(register, false);
            }

            WriteValue(register, value, false);
        }

        #endregion

        #region R/W EC

        private void WriteValue(int register, int value, bool writeWord)
        {
            if (writeWord)
            {
                this.ec.WriteWord((byte)register, (ushort)value);
            }
            else
            {
                this.ec.WriteByte((byte)register, (byte)value);
            }
        }

        private int ReadValue(int register, bool readWord)
        {
            return readWord
                ? this.ec.ReadWord((byte)register)
                : this.ec.ReadByte((byte)register);
        }

        #endregion

        #region Reset EC

        private void ResetEc()
        {
            if (this.config.RegisterWriteConfigurations.Any(x => x.ResetRequired)
                || this.config.FanConfigurations.Any(x => x.ResetRequired))
            {
                bool mutexAquired = this.ec.AcquireLock(EcTimeout * 2);

                //try to reset the EC even if AquireLock failed
                try
                {
                    if (config != null)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            ResetRegisterWriteConfigs();
                            ResetFans();
                        }
                    }
                }
                finally
                {
                    if (mutexAquired)
                    {
                        this.ec.ReleaseLock();
                    }
                }
            }
        }

        private void ResetFans()
        {
            foreach (FanConfiguration cfg in this.config.FanConfigurations)
            {
                if (cfg.ResetRequired)
                {
                    WriteValue(cfg.WriteRegister, cfg.FanSpeedResetValue, this.config.ReadWriteWords);
                }
            }
        }

        private void ResetRegisterWriteConfigs()
        {
            foreach (RegisterWriteConfiguration cfg in this.config.RegisterWriteConfigurations)
            {
                if (cfg.ResetRequired)
                {
                    ApplyRegisterWriteConfig(cfg.ResetValue, cfg.Register, cfg.ResetWriteMode);
                }
            }
        }

        #endregion

        #region Get Hardware Infos

        private int GetFanSpeedValue(int readRegister, int minReadValue, int maxReadValue, bool readWriteWords)
        {
            int fanSpeed = 0;


            // If the value is out of range 3 or more times,
            // minFanSpeed and/or maxFanSpeed are probably wrong.
            for (int i = 0; i <= 2; i++)
            {
                fanSpeed = ReadValue(readRegister, readWriteWords);

                if ((fanSpeed >= minReadValue) && (fanSpeed <= maxReadValue))
                {
                    break;
                }
            }

            return fanSpeed;
        }

        private double GetTemperature()
        {
            return this.tempFilter.FilterTemperature(this.tempMon.GetTemperature());
        }

        #endregion

        #endregion

        #region IDisposable implementation

        private bool disposed = false;

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                StopFanControlCore();

                if (this.autoEvent != null)
                {
                    this.autoEvent.Dispose();
                }

                if (this.asyncOp != null)
                {
                    this.asyncOp.OperationCompleted();
                }

                if (this.ec != null)
                {
                    this.ec.Dispose();
                }

                if (this.tempMon != null)
                {
                    this.tempMon.Dispose();
                }

                GC.SuppressFinalize(this);
            }
        }

        #endregion
    }
}
