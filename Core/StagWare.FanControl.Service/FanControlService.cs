using StagWare.FanControl.Configurations;
using StagWare.Settings;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.Linq;
using System.Threading;

namespace StagWare.FanControl.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class FanControlService : IFanControlService, IDisposable
    {
        #region Constants

        private const string ConfigsDirectoryName = "Configs";
        private const string SettingsFileName = "NbfcServiceSettings.xml";
        private const string SettingsFolderName = "NbfcService";

        #endregion

        #region Private Fields

        private static readonly string ConfigsDirectory = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                ConfigsDirectoryName);

        private FanControl fanControl;
        private string selectedConfig;
        private int[] fanSpeedSteps;
        int fansCount;

        #endregion

        #region Constructors

        public FanControlService()
        {
            string dir = "";

            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                dir = "/etc/";
            }
            else
            {
                dir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            }

            string settingsFile = Path.Combine(
                dir,
                SettingsFolderName,
                SettingsFileName);

            ServiceSettings.SettingsFileName = settingsFile;

            if (ServiceSettings.Default.Autostart)
            {
                Start(ServiceSettings.Default.ReadOnly);
            }
        }

        #endregion

        #region IFanControlService implementation

        public void SetTargetFanSpeed(float value, int fanIndex)
        {
            if (!this.disposed && fanControl != null)
            {
                if (fanIndex < 0 || fanIndex >= this.fansCount)
                {
                    throw new ArgumentOutOfRangeException("fanIndex");
                }
                else
                {
                    this.fanControl.SetTargetFanSpeed(value, fanIndex);

                    ServiceSettings.Default.TargetFanSpeeds[fanIndex] = value;
                    ServiceSettings.Save();
                }
            }
        }

        public FanControlInfo GetFanControlInfo()
        {
            bool initialized = !this.disposed && this.fanControl != null;
            var info = new FanControlInfo()
            {
                SelectedConfig = this.selectedConfig,
                Temperature = 0
            };

            if (initialized)
            {
                info.Enabled = this.fanControl.Enabled;
                info.ReadOnly = this.fanControl.ReadOnly;
            }

            if (initialized)
            {
                info.TemperatureSourceDisplayName = this.fanControl.TemperatureSourceDisplayName;
            }

            if (this.fanControl.Enabled)
            {
                info.Temperature = (int)Math.Round(fanControl.Temperature);

                ReadOnlyCollection<FanInformation> fanInfo = this.fanControl.FanInformation;
                info.FanStatus = new FanStatus[fanInfo.Count];

                for (int i = 0; i < fanInfo.Count; i++)
                {
                    info.FanStatus[i] = new FanStatus()
                    {
                        AutoControlEnabled = fanInfo[i].AutoFanControlEnabled,
                        CriticalModeEnabled = fanInfo[i].CriticalModeEnabled,
                        CurrentFanSpeed = fanInfo[i].CurrentFanSpeed,
                        TargetFanSpeed = fanInfo[i].TargetFanSpeed,
                        FanDisplayName = fanInfo[i].FanDisplayName,
                        FanSpeedSteps = this.fanSpeedSteps[i]
                    };
                }
            }

            return info;
        }

        public void Start(bool readOnly = true)
        {
            if (this.disposed)
            {
                return;
            }

            if (this.fanControl == null)
            {
                FanControlConfigV2 cfg;

                if (TryLoadConfig(out cfg))
                {
                    InitializeFanSpeedSteps(cfg);

                    if (TryInitializeFanControl(cfg, out this.fanControl))
                    {
                        this.fansCount = this.fanControl.FanInformation.Count;
                        ServiceSettings.Default.Autostart = this.fanControl.Enabled;
                        ServiceSettings.Save();
                    }
                }
            }

            if (this.fanControl != null)
            {
                this.fanControl.Start(readOnly);

                ServiceSettings.Default.Autostart = this.fanControl.Enabled;
                ServiceSettings.Default.ReadOnly = this.fanControl.ReadOnly;
                ServiceSettings.Save();
            }
        }

        public void Stop()
        {
            if (!this.disposed && fanControl != null)
            {
                try
                {
                    ServiceSettings.Default.Autostart = false;
                    ServiceSettings.Save();
                }
                catch
                {
                }

                fanControl.Stop();
            }
        }

        public void SetConfig(string configUniqueId)
        {
            if (!this.disposed)
            {
                var cfgMan = new FanControlConfigManager(ConfigsDirectory);

                if (!cfgMan.Contains(configUniqueId))
                {
                    throw new ArgumentException("The specified config does not exist.");
                }
                else
                {
                    ServiceSettings.Default.SelectedConfigId = configUniqueId;
                    ServiceSettings.Save();

                    if (this.fanControl != null)
                    {
                        this.fanControl.Dispose();
                        this.fanControl = null;
                    }
                }
            }
        }

        public string[] GetConfigNames()
        {
            var cfgMan = new FanControlConfigManager(ConfigsDirectory);
            return cfgMan.ConfigNames.ToArray();
        }

        #endregion

        #region IDisposable implementation

        private bool disposed = false;

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;

                if (this.fanControl != null)
                {
                    this.fanControl.Dispose();
                    this.fanControl = null;
                }

                GC.SuppressFinalize(this);
            }
        }

        #endregion

        #region Public Methods

        public void Pause()
        {
            if ((this.fanControl != null) && (this.fanControl.Enabled))
            {
                this.fanControl.Start(true);
            }
        }

        public void Continue()
        {
            if ((this.fanControl != null) && (this.fanControl.Enabled))
            {
                try
                {
                    this.fanControl.Start(ServiceSettings.Default.ReadOnly);
                }
                catch (TimeoutException)
                {
                    Thread.Sleep(3000);
                    this.fanControl.Start(ServiceSettings.Default.ReadOnly);
                }

                if (!ServiceSettings.Default.ReadOnly)
                {
                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        Thread.Sleep(5000);

                        // Retry if current fan speed differs from target fan speed by 10% or more
                        if (this.fanControl.FanInformation.Any(
                            x => Math.Abs(x.CurrentFanSpeed - x.TargetFanSpeed) >= 10))
                        {
                            this.fanControl.Start(true);
                            this.fanControl.Start(false);
                        }
                    });
                }
            }
        }

        #endregion

        #region Private Methods

        private static bool TryInitializeFanControl(FanControlConfigV2 cfg, out FanControl fanControl)
        {
            bool success = false;
            fanControl = null;

            try
            {
                float[] speeds = ServiceSettings.Default.TargetFanSpeeds;

                if (speeds == null || speeds.Length != cfg.FanConfigurations.Count)
                {
                    speeds = new float[cfg.FanConfigurations.Count];

                    for (int i = 0; i < speeds.Length; i++)
                    {
                        speeds[i] = FanControl.AutoFanSpeedPercentage;
                    }

                    ServiceSettings.Default.TargetFanSpeeds = speeds;
                    ServiceSettings.Save();
                }

                fanControl = new FanControl(cfg);

                for (int i = 0; i < speeds.Length; i++)
                {
                    fanControl.SetTargetFanSpeed(speeds[i], i);
                }

                success = true;
            }
            finally
            {
                if (!success && fanControl != null)
                {
                    fanControl.Dispose();
                    fanControl = null;
                }
            }

            return success;
        }

        private void InitializeFanSpeedSteps(FanControlConfigV2 cfg)
        {
            this.fanSpeedSteps = new int[cfg.FanConfigurations.Count];

            for (int i = 0; i < this.fanSpeedSteps.Length; i++)
            {
                var fanConfig = cfg.FanConfigurations[i];

                this.fanSpeedSteps[i] = Math.Max(fanConfig.MinSpeedValue, fanConfig.MaxSpeedValue)
                    - Math.Min(fanConfig.MinSpeedValue, fanConfig.MaxSpeedValue);
            }
        }

        private bool TryLoadConfig(out FanControlConfigV2 config)
        {
            bool result = false;
            var configManager = new FanControlConfigManager(FanControlService.ConfigsDirectory);
            string id = ServiceSettings.Default.SelectedConfigId;

            if (!string.IsNullOrWhiteSpace(id) && configManager.SelectConfig(id))
            {
                this.selectedConfig = configManager.SelectedConfigName;
                config = configManager.SelectedConfig;
                result = true;
            }
            else
            {
                config = null;
            }

            return result;
        }

        #endregion
    }
}
