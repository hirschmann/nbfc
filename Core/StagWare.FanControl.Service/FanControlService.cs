using StagWare.FanControl.Configurations;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Threading;
using NLog;
using SettingsService = StagWare.Settings.SettingsService<StagWare.ServiceSettings>;

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

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
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
            SettingsService.BaseDirectory = Environment.OSVersion.Platform == PlatformID.Unix
                ? "/etc/"
                : Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

            SettingsService.LoadSettingsFailed += (sender, args) =>
            {
                logger.Error(args.Exception, "Failed to load settings. Restoring defaults.");
                SettingsService.RestoreDefaults();
            };

            if (SettingsService.Settings.Autostart)
            {
                Start(SettingsService.Settings.ReadOnly);
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

                    SettingsService.Settings.TargetFanSpeeds[fanIndex] = value;
                    SaveSettings();
                }
            }
        }

        public FanControlInfo GetFanControlInfo()
        {
            var info = new FanControlInfo()
            {
                Enabled = false,
                ReadOnly = SettingsService.Settings.ReadOnly,
                SelectedConfig = this.selectedConfig,
                Temperature = 0
            };

            if (!this.disposed && this.fanControl != null)
            {
                info.Enabled = this.fanControl.Enabled;
                info.ReadOnly = this.fanControl.ReadOnly;
                info.TemperatureSourceDisplayName = this.fanControl.TemperatureSourceDisplayName;

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
                string selectedCfg = SettingsService.Settings.SelectedConfigId;

                if (string.IsNullOrWhiteSpace(selectedCfg))
                {
                    logger.Warn("An attempt to start the fan controller failed because no config is selected");
                    return;
                }

                FanControlConfigV2 cfg;

                if (TryLoadConfig(selectedCfg, out cfg))
                {
                    InitializeFanSpeedSteps(cfg);

                    if (TryInitializeFanControl(cfg, out this.fanControl))
                    {
                        this.fansCount = this.fanControl.FanInformation.Count;
                        SettingsService.Settings.Autostart = this.fanControl.Enabled;
                        SaveSettings();
                    }
                }
                else
                {
                    logger.Error($"Failed to load config: {selectedCfg}");
                }
            }

            if (this.fanControl != null)
            {
                this.fanControl.Start(readOnly);

                SettingsService.Settings.Autostart = this.fanControl.Enabled;
                SettingsService.Settings.ReadOnly = this.fanControl.ReadOnly;
                SaveSettings();
            }
        }

        public void Stop()
        {
            if (!this.disposed && fanControl != null)
            {
                SettingsService.Settings.Autostart = false;
                SaveSettings();
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
                    SettingsService.Settings.SelectedConfigId = configUniqueId;
                    SaveSettings();

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

        public string[] GetRecommendedConfigs()
        {
            var cfgMan = new FanControlConfigManager(ConfigsDirectory);
            return cfgMan.RecommendConfigs().ToArray();
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
                    this.fanControl.Start(SettingsService.Settings.ReadOnly);
                }
                catch (TimeoutException e)
                {
                    logger.Warn(e, "Trying to start the service timed out. Retrying");
                    Thread.Sleep(3000);
                    this.fanControl.Start(SettingsService.Settings.ReadOnly);
                }
            }
        }

        #endregion

        #region Private Methods

        private static void SaveSettings()
        {
            try
            {
                SettingsService.Save();
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to save settings");
            }
        }

        private static bool TryInitializeFanControl(FanControlConfigV2 cfg, out FanControl fanControl)
        {
            bool success = false;
            fanControl = null;

            try
            {
                float[] speeds = SettingsService.Settings.TargetFanSpeeds;

                if (speeds == null || speeds.Length != cfg.FanConfigurations.Count)
                {
                    speeds = new float[cfg.FanConfigurations.Count];

                    for (int i = 0; i < speeds.Length; i++)
                    {
                        speeds[i] = FanControl.AutoFanSpeedPercentage;
                    }

                    SettingsService.Settings.TargetFanSpeeds = speeds;
                    SaveSettings();
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

        private bool TryLoadConfig(string id, out FanControlConfigV2 config)
        {
            bool result = false;
            var configManager = new FanControlConfigManager(FanControlService.ConfigsDirectory);

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
