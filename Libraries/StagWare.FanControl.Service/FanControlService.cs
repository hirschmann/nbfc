using StagWare.FanControl.Configurations;
using System;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.Linq;
using StagWare.FanControl.Service.Settings;
using System.Collections.Generic;

namespace StagWare.FanControl.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class FanControlService : IFanControlService, IDisposable
    {
        #region Constants

        private const string ConfigsDirectoryName = "Configs";
        private const int AutoControlFanSpeedPercentage = 101;
        private static readonly string SettingsDir = Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.CommonApplicationData),
            "NbfcService");

        #endregion

        #region Private Fields

        private FanControl fanControl;
        private bool initialized;
        private string selectedConfig;
        private int[] fanSpeedSteps;
        private string executingAssemblyDirName;

        #endregion

        #region Constructors

        public FanControlService()
        {
            executingAssemblyDirName = Assembly.GetExecutingAssembly().Location;
            executingAssemblyDirName = Path.GetDirectoryName(executingAssemblyDirName);

            using (var settings = ServiceSettings.Load(SettingsDir))
            {
                if (settings.AutoStart && TryInitializeFanControl(settings))
                {
                    this.initialized = true;
                    this.fanControl.Start();
                }
            }
        }

        #endregion

        #region IFanControlService implementation

        public void SetTargetFanSpeed(float value, int fanIndex)
        {
            if (fanControl != null)
            {
                fanControl.SetTargetFanSpeed(value, fanIndex);
            }
        }

        public FanControlInfo GetFanControlInfo()
        {
            var info = new FanControlInfo()
            {
                IsInitialized = initialized,
                SelectedConfig = selectedConfig,
            };

            if (fanControl != null)
            {
                info.CpuTemperature = (int)Math.Round(fanControl.CpuTemperature);

                IList<FanInformation> fanInfo = this.fanControl.FanInformation;
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

        public bool Restart()
        {
            using (var settings = ServiceSettings.Load(SettingsDir))
            {
                return Restart(settings);
            }
        }

        public void Stop()
        {
            if (this.initialized)
            {
                initialized = false;
                DisposeFanControl();

                using (var settings = ServiceSettings.Load(SettingsDir))
                {
                    settings.AutoStart = false;
                    settings.Save();
                }
            }
        }

        public bool Start()
        {
            if (!this.initialized)
            {
                using (var settings = ServiceSettings.Load(SettingsDir))
                {
                    if (TryInitializeFanControl(settings))
                    {
                        this.initialized = true;
                        this.fanControl.Start();
                    }

                    settings.AutoStart = true;
                    settings.Save();
                }
            }

            return this.initialized;
        }

        public void SetConfig(string configUniqueId)
        {
            using (var settings = ServiceSettings.Load(SettingsDir))
            {
                settings.SelectedConfigId = configUniqueId;
                settings.Save();

                Restart(settings);
            }
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            DisposeFanControl();
            GC.SuppressFinalize(this);
        }

        private void DisposeFanControl()
        {
            if (fanControl != null)
            {
                try
                {
                    using (var settings = ServiceSettings.Load(SettingsDir))
                    {
                        settings.AutoStart = this.initialized;
                        settings.TargetFanSpeeds = fanControl.FanInformation
                            .Select(x => x.AutoFanControlEnabled ? AutoControlFanSpeedPercentage : x.TargetFanSpeed).ToArray();

                        settings.Save();
                    }
                }
                catch
                {
                }

                fanControl.Dispose();
                fanControl = null;
            }
        }

        #endregion

        #region Public Methods

        public void ReInitializeFanControl()
        {
            this.fanControl.Start();
        }

        #endregion

        #region Private Methods

        private bool TryInitializeFanControl(ServiceSettings settings)
        {
            bool success = false;

            try
            {
                FanControlConfigV2 cfg;

                if (TryLoadConfig(settings, out cfg))
                {
                    InitializeFanSpeedSteps(cfg);
                    InitializeFanControl(settings, cfg);
                    success = true;
                }
            }
            catch
            {
            }
            finally
            {
                if (!success && this.fanControl != null)
                {
                    this.fanControl.Dispose();
                    this.fanControl = null;
                }
            }

            return success;
        }

        private void InitializeFanControl(ServiceSettings settings, FanControlConfigV2 cfg)
        {
            this.fanControl = new FanControl(cfg);

            if (settings.TargetFanSpeeds == null)
            {
                settings.TargetFanSpeeds = new float[fanControl.FanInformation.Count];

                for (int i = 0; i < settings.TargetFanSpeeds.Length; i++)
                {
                    settings.TargetFanSpeeds[i] = AutoControlFanSpeedPercentage;
                }
            }

            for (int i = 0; i < settings.TargetFanSpeeds.Length; i++)
            {
                fanControl.SetTargetFanSpeed(settings.TargetFanSpeeds[i], i);
            }
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

        private bool TryLoadConfig(ServiceSettings settings, out FanControlConfigV2 config)
        {
            bool result = false;
            string path = Path.Combine(executingAssemblyDirName, ConfigsDirectoryName);
            var configManager = new FanControlConfigManager(path);

            if (!string.IsNullOrWhiteSpace(settings.SelectedConfigId)
                && configManager.SelectConfig(settings.SelectedConfigId))
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

        private bool Restart(ServiceSettings settings)
        {
            this.initialized = false;
            DisposeFanControl();

            if (TryInitializeFanControl(settings))
            {
                this.initialized = true;
                this.fanControl.Start();
            }

            return this.initialized;
        }

        #endregion
    }
}
