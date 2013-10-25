using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using OpenHardwareMonitor.Hardware;
using StagWare.FanControl.Configurations;

namespace StagWare.FanControl
{
    public class FanControl : IDisposable
    {
        #region Constants

        private const int MaxRetries = 10;
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

        private readonly IHardware cpu;
        private readonly ITemperatureFilter tempFilter;
        private readonly ISensor[] cpuTempSensors;

        private readonly FanInformation[] fanInfo;
        private readonly FanSpeedManager[] fanSpeeds;

        private volatile int cpuTemperature;

        #endregion

        #region Constructor

        public FanControl(FanControlConfigV2 config)
            : this(config, GetDefaultTemperatureFilter(DeliminatePollInterval(config.EcPollInterval)))
        {
        }

        public FanControl(FanControlConfigV2 config, ITemperatureFilter filter)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }

            this.tempFilter = filter;
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

            var computer = new Computer();
            computer.CPUEnabled = true;
            computer.Open();
            this.cpu = computer.Hardware.FirstOrDefault(x => x.HardwareType == HardwareType.CPU);

            if (this.cpu != null)
            {
                this.cpuTempSensors = InitializeTempSensors(cpu);
            }

            if (this.cpuTempSensors == null || this.cpuTempSensors.Length <= 0)
            {
                throw new PlatformNotSupportedException("No CPU temperature sensor(s) found.");
            }
        }

        #region Contruction Helper Methods

        private static ITemperatureFilter GetDefaultTemperatureFilter(int pollInterval)
        {
            return new AverageTemperatureFilter(
                (int)Math.Ceiling((double)AverageTemperatureTimespan / pollInterval));
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

        public int CpuTemperature { get { return this.cpuTemperature; } }
        public bool Enabled { get { return this.timer != null; } }

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
            if (EmbeddedController.WaitIsaBusMutex(DefaultPollInterval))
            {
                try
                {
                    ApplyRegisterWriteConfigurations(true);
                }
                finally
                {
                    EmbeddedController.ReleaseIsaBusMutex();
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
                    this.cpuTemperature = GetCpuTemperature();
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
            if (EmbeddedController.WaitIsaBusMutex(pollInterval / 2))
            {
                try
                {
                    ApplyRegisterWriteConfigurations();
                    UpdateAndApplyFanSpeeds();
                }
                finally
                {
                    EmbeddedController.ReleaseIsaBusMutex();
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

        private static void WriteValue(int register, int value, bool writeWord)
        {
            int tries = 0;
            int successfulTries = 0;

            while ((successfulTries < 3) && (tries < MaxRetries))
            {
                tries++;
                bool success = false;

                if (writeWord)
                {
                    success = EmbeddedController.TryWriteWord((byte)register, value);
                }
                else
                {
                    success = EmbeddedController.TryWriteByte((byte)register, (byte)value);
                }

                if (success)
                {
                    successfulTries++;
                }
            }
        }

        private static int ReadValue(int register, bool readWord)
        {
            int tries = 0;
            int wordValue = 0;
            byte byteValue = 0;
            bool success = false;

            while (!success && (tries < MaxRetries))
            {
                tries++;

                if (readWord)
                {
                    success = EmbeddedController.TryReadWord((byte)register, out wordValue);
                }
                else
                {
                    success = EmbeddedController.TryReadByte((byte)register, out byteValue);
                }
            }

            return readWord ? wordValue : byteValue;
        }

        private void ResetEc()
        {
            Stop();

            if (this.config.RegisterWriteConfigurations.Any(x => x.ResetRequired)
                || this.config.FanConfigurations.Any(x => x.ResetRequired))
            {
                bool mutexAquired = EmbeddedController.WaitIsaBusMutex(DefaultPollInterval / 2);

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
                        EmbeddedController.ReleaseIsaBusMutex();
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

        private static int GetFanSpeedValue(FanConfiguration cfg, bool readWriteWords)
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

        private int GetCpuTemperature()
        {
            float temp = 0;
            int count = 0;
            this.cpu.Update();

            foreach (ISensor sensor in this.cpuTempSensors)
            {
                if (sensor.Value != null)
                {
                    temp += (float)sensor.Value;
                    count++;
                }
            }

            int temperature = (int)Math.Round(temp / (float)count);

            return tempFilter.FilterTemperature(temperature);
        }

        private static ISensor[] InitializeTempSensors(IHardware cpu)
        {
            if (cpu == null)
            {
                throw new PlatformNotSupportedException("Failed to access CPU temperature sensors(s).");
            }

            cpu.Update();

            IEnumerable<ISensor> sensors = cpu.Sensors
                .Where(x => x.SensorType == SensorType.Temperature);

            ISensor packageSensor = sensors.FirstOrDefault(x =>
            {
                string upper = x.Name.ToUpperInvariant();
                return upper.Contains("PACKAGE") || upper.Contains("TOTAL");
            });

            return packageSensor != null ?
                new ISensor[] { packageSensor } : sensors.ToArray();
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
