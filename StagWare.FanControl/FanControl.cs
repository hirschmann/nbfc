using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using OpenHardwareMonitor.Hardware;
using OpenHardwareMonitor.Hardware.LPC;
using StagWare.FanControl.Configurations;

namespace StagWare.FanControl
{
    public class FanControl : IDisposable
    {
        #region Constants

        private const int MinPollInterval = 100;
        private const int DefaultPollInterval = 3000;
        private const int AverageTemperatureTimespan = 6000;

        #endregion

        #region Private Fields

        private readonly AutoResetEvent autoEvent = new AutoResetEvent(false);
        private Timer timer;
        private AsyncOperation asyncOp;

        private readonly int pollInterval;
        private readonly FanControlConfigV2 config;

        private readonly ITemperatureFilter tempFilter;
        private readonly ITemperatureProvider tempProvider;
        private readonly IEmbeddedController ec;

        private readonly FanInformation[] fanInfo;
        private readonly FanSpeedManager[] fanSpeeds;

        private volatile float cpuTemperature;

        #endregion

        #region Constructor

        public FanControl(FanControlConfigV2 config)
            : this(
            config,
            GetDefaultTemperatureFilter(DeliminatePollInterval(config.EcPollInterval)),
            GetDefaultTemperatureProvider(),
            GetDefaultEc())
        {
        }

        public FanControl(
            FanControlConfigV2 config,
            ITemperatureFilter tempFilter,
            ITemperatureProvider tempProvider,
            IEmbeddedController ec)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (tempFilter == null)
            {
                throw new ArgumentNullException("filter");
            }

            if (tempProvider == null)
            {
                throw new ArgumentNullException("tempProvider");
            }

            if (ec == null)
            {
                throw new ArgumentNullException("ec");
            }

            this.tempFilter = tempFilter;
            this.tempProvider = tempProvider;
            this.ec = ec;
            this.config = (FanControlConfigV2)config.Clone();
            this.pollInterval = config.EcPollInterval;
            this.asyncOp = AsyncOperationManager.CreateOperation(null);
            this.fanInfo = new FanInformation[config.FanConfigurations.Count];
            this.fanSpeeds = new FanSpeedManager[config.FanConfigurations.Count];

            for (int i = 0; i < this.fanInfo.Length; i++)
            {
                var cfg = config.FanConfigurations[i];

                this.fanSpeeds[i] = new FanSpeedManager(cfg, config.CriticalTemperature);
                this.fanInfo[i] = new FanInformation()
                {
                    FanDisplayName = cfg.FanDisplayName
                };
            }
        }

        #region Contruction Helper Methods

        private static ITemperatureFilter GetDefaultTemperatureFilter(int pollInterval)
            {
            return new AverageTemperatureFilter(
                (int)Math.Ceiling((double)AverageTemperatureTimespan / pollInterval));
            }

        private static ITemperatureProvider GetDefaultTemperatureProvider()
            {
            return new CpuTemperatureProvider();
            }

        private static IEmbeddedController GetDefaultEc()
        {
            return new EmbeddedController();
        }

        private static int DeliminatePollInterval(int pollInterval)
        {
            #region limit poll intervall in release

#if !DEBUG

            if (pollInterval < MinPollInterval)
            {
                return DefaultPollInterval;
            }

#endif

            #endregion

            return pollInterval;
        }

        #endregion

        #endregion

        #region Events

        public event EventHandler ECUpdated;

        #endregion

        #region Properties

        public float CpuTemperature
        {
            get { return this.cpuTemperature; }
        }

        public bool Enabled
        {
            get { return this.timer != null; }
        }

        public IList<FanInformation> FanInformation
        {
            get
            {
                lock (this.fanInfo)
                {
                    return this.fanInfo.Select(x => x.Clone() as FanInformation).ToList();
                }
            }
        }

        #endregion       

        #region Public Methods

        public void Start(int delay = 0)
        {
            if (this.ec.AquireLock(DefaultPollInterval))
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

            if (this.timer == null)
            {
                this.autoEvent.Set();
                this.timer = new Timer(new TimerCallback(TimerCallback), null, delay, this.pollInterval);
            }
        }

        public void SetTargetFanSpeed(double speed, int fanIndex)
        {
            lock (this.fanSpeeds)
            {
                this.fanSpeeds[fanIndex].UpdateFanSpeed(speed, this.cpuTemperature);

                lock (this.fanInfo)
                {
                    UpdateFanInformation();
                }
            }

            UpdateEcAsync();
        }

        #endregion

        #region Protected Methods

        protected void OnECUpdated()
        {
            if (ECUpdated != null)
            {
                ECUpdated(this, new EventArgs());
            }
        }

        #endregion

        #region Private Methods

        #region Update EC

        private void Stop()
        {
            if (this.autoEvent != null && !this.autoEvent.SafeWaitHandle.IsClosed)
            {
                this.autoEvent.Reset();
            }

            if (this.timer != null)
            {
                this.timer.Dispose();
                this.timer = null;
            }
        }

        private void TimerCallback(object state)
        {
            if (this.autoEvent.WaitOne(pollInterval / 2))
            {
                try
                {
                    // Read CPU temperature before the call to UpdateEc(),
                    // because both methods try to aquire ISA bus mutex
                    this.cpuTemperature = (float)GetTemperature();
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
            if (this.ec.AquireLock(pollInterval / 2))
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

                asyncOp.Post((object args) => OnECUpdated(), null);
            }
        }

        private void UpdateAndApplyFanSpeeds()
        {
            lock (this.fanSpeeds)
            {
                int idx = 0;

                foreach (FanSpeedManager fsm in this.fanSpeeds)
                {
                    fsm.UpdateFanSpeed(this.cpuTemperature, fsm.AutoControlEnabled);

                    WriteValue(
                        this.config.FanConfigurations[idx].WriteRegister,
                        fsm.FanSpeedValue, 
                        this.config.ReadWriteWords);

                    idx++;
                }

                lock (this.fanInfo)
                {
                    UpdateFanInformation(true);
                }
            }
        }

        private void UpdateEcAsync()
        {
            var action = new Action<object>(TimerCallback);
            action.BeginInvoke(null, null, null);
        }

        #region Helper Methods

        private void UpdateFanInformation(bool isaBusMutexAquired = false)
        {
            for (int i = 0; i < this.fanSpeeds.Length; i++)
            {
                var info = this.fanInfo[i];
                var speed = this.fanSpeeds[i];
                var fanConfig = this.config.FanConfigurations[i];

                info.CriticalModeEnabled = speed.CriticalModeEnabled;
                info.AutoFanControlEnabled = speed.AutoControlEnabled;
                info.TargetFanSpeed = speed.FanSpeedPercentage;

                if (isaBusMutexAquired)
                {
                    info.CurrentFanSpeed = speed.FanSpeedToPercentage(
                        GetFanSpeedValue(fanConfig, this.config.ReadWriteWords));
                }
            }
        }

        private void ApplyRegisterWriteConfigurations(bool initializing = false)
        {
            if (this.config.RegisterWriteConfigurations != null)
            {
                foreach (RegisterWriteConfiguration rwc in this.config.RegisterWriteConfigurations)
                {
                    if (initializing || rwc.WriteOccasion == RegisterWriteOccasion.OnWriteFanSpeed)
                    {
                        ApplyRegisterWriteConfig(rwc);
                    }
                }
            }
        }

        private void ApplyRegisterWriteConfig(RegisterWriteConfiguration rwc)
        {
            int value = rwc.Value;

            switch (rwc.WriteMode)
            {
                case RegisterWriteMode.And:
                    value &= ReadValue(rwc.Register, config.ReadWriteWords);
                    goto case RegisterWriteMode.Set;

                case RegisterWriteMode.Or:
                    value |= ReadValue(rwc.Register, config.ReadWriteWords);
                    goto case RegisterWriteMode.Set;

                case RegisterWriteMode.Set:
                    WriteValue(rwc.Register, value, config.ReadWriteWords);
                    break;
            }
        }

        #endregion

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

        private void ResetEc()
        {
            Stop();

            if (this.config.RegisterWriteConfigurations.Any(x => x.ResetRequired)
                || this.config.FanConfigurations.Any(x => x.ResetRequired))
            {
                bool mutexAquired = this.ec.AquireLock(DefaultPollInterval / 2);

                try
                {
                    if (config != null)
                    {
                        for (int i = 0; i < 5; i++)
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
            foreach (FanConfiguration fanCfg in this.config.FanConfigurations)
            {
                if (fanCfg.ResetRequired)
                {
                    WriteValue(fanCfg.WriteRegister, fanCfg.FanSpeedResetValue, this.config.ReadWriteWords);
                }
            }
        }

        private void ResetRegisterWriteConfigs()
        {
            foreach (RegisterWriteConfiguration regWrtCfg in this.config.RegisterWriteConfigurations)
            {
                if (regWrtCfg.ResetRequired)
                {
                    WriteValue(regWrtCfg.Register, regWrtCfg.ResetValue, false);
                }
            }
        }

        #endregion

        #region Get Hardware Infos

        private int GetFanSpeedValue(FanConfiguration cfg, bool readWriteWords)
        {
            int fanSpeed = 0;
            int min = Math.Min(cfg.MinSpeedValue, cfg.MaxSpeedValue);
            int max = Math.Max(cfg.MinSpeedValue, cfg.MaxSpeedValue);

            // If the value is out of range 3 or more times,
            // minFanSpeed and/or maxFanSpeed are probably wrong.
            for (int i = 0; i <= 2; i++)
            {
                fanSpeed = ReadValue(cfg.ReadRegister, readWriteWords);

                if ((fanSpeed >= min) && (fanSpeed <= max))
                {
                    break;
                }
            }

            return fanSpeed;
        }

        private double GetTemperature()
        {
            return this.tempFilter.FilterTemperature(this.tempProvider.GetTemperature());
                }

        #endregion

        #endregion

        #region IDisposable implementation

        ~FanControl()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposeManagedResources)
        {
            if (disposeManagedResources)
            {
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

                if (autoEvent != null)
                {
                    autoEvent.Dispose();
                }
            }

            ResetEc();
        }

        #endregion
    }
}
