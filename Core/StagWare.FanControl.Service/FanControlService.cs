using StagWare.FanControl.Configurations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using StagWare.Settings;

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
                Start();
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

                    ServiceSettings.Default.TargetFanSpeeds = GetTargetFanSpeeds(this.fanControl.FanInformation);
                    ServiceSettings.Save();
                }
            }
        }

        public FanControlInfo GetFanControlInfo()
        {
            bool initialized = !this.disposed && this.fanControl != null;
            bool enabled = initialized && this.fanControl.Enabled;

            var info = new FanControlInfo()
            {
                Enabled = enabled,
                SelectedConfig = selectedConfig,
                Temperature = 0
            };

            if (initialized)
            {
                info.TemperatureSourceDisplayName = this.fanControl.TemperatureSourceDisplayName;
            }

            if (enabled)
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

        public bool Start()
        {
            if (!this.disposed)
            {
                if (this.fanControl == null)
                {
                    FanControlConfigV2 cfg;

                    if (TryLoadConfig(out cfg))
                    {
                        InitializeFanSpeedSteps(cfg);

                        if (TryInitializeFanControl(cfg, out this.fanControl))
                        {
                            this.fansCount = this.fanControl.FanInformation.Count;
                            this.fanControl.Start();
                            ServiceSettings.Default.Autostart = this.fanControl.Enabled;
                            ServiceSettings.Save();
                        }
                    }
                }
                else if (!this.fanControl.Enabled)
                {
                    this.fanControl.Start();
                    ServiceSettings.Default.Autostart = this.fanControl.Enabled;
                    ServiceSettings.Save();
                }
            }

            return ServiceSettings.Default.Autostart;
        }

        public void Stop()
        {
            if (!this.disposed && fanControl != null)
            {
                try
                {
                    ServiceSettings.Default.Autostart = false;
                    ServiceSettings.Default.TargetFanSpeeds = GetTargetFanSpeeds(this.fanControl.FanInformation);
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

                    Start();
                }
            }
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

        public void ReInitializeFanControl()
        {
            if (!this.disposed && this.fanControl != null)
            {
                this.fanControl.ReInitialize();
            }
        }

        #endregion

        #region Private Methods

        private static float[] GetTargetFanSpeeds(IList<FanInformation> fanInfos)
        {
            float[] speeds = new float[fanInfos.Count];

            for (int i = 0; i < speeds.Length; i++)
            {
                speeds[i] = fanInfos[i].AutoFanControlEnabled
                    ? FanControl.AutoFanSpeedPercentage
                    : fanInfos[i].TargetFanSpeed;
            }

            return speeds;
        }

        private static bool TryInitializeFanControl(FanControlConfigV2 cfg, out FanControl fanControl)
        {
            bool success = false;
            fanControl = null;

            try
            {
                float[] speeds = ServiceSettings.Default.TargetFanSpeeds;
                fanControl = new FanControl(cfg);

                if (speeds == null || speeds.Length != cfg.FanConfigurations.Count)
                {
                    speeds = new float[cfg.FanConfigurations.Count];

                    for (int i = 0; i < speeds.Length; i++)
                    {
                        speeds[i] = FanControl.AutoFanSpeedPercentage;
                    }
                }

                if (speeds != null && speeds.Length == fanControl.FanInformation.Count)
                {
                    for (int i = 0; i < speeds.Length; i++)
                    {
                        fanControl.SetTargetFanSpeed(speeds[i], i);
                    }
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

                // Add 1 extra step for "auto control"
                this.fanSpeedSteps[i] = 1 + (Math.Max(fanConfig.MinSpeedValue, fanConfig.MaxSpeedValue)
                    - Math.Min(fanConfig.MinSpeedValue, fanConfig.MaxSpeedValue));
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
