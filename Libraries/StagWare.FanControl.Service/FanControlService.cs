using StagWare.FanControl.Configurations;
using StagWare.FanControl.Service.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.ServiceModel;

namespace StagWare.FanControl.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class FanControlService : IFanControlService, IDisposable
    {
        #region Constants

        private const string ConfigsDirectoryName = "Configs";

        #endregion

        #region Private Fields

        private static readonly string configsDirectory = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                ConfigsDirectoryName);

        private FanControl fanControl;
        private string selectedConfig;
        private int[] fanSpeedSteps;

        #endregion

        #region Constructors

        public FanControlService()
        {
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }

            if (Settings.Default.AutoStart)
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
                //TODO: check if index is valid

                fanControl.SetTargetFanSpeed(value, fanIndex);
            }
        }

        public FanControlInfo GetFanControlInfo()
        {
            bool enabled = !this.disposed
                && this.fanControl != null
                && this.fanControl.Enabled;

            var info = new FanControlInfo()
            {
                Enabled = enabled,
                SelectedConfig = selectedConfig,
            };

            if (enabled)
            {
                info.CpuTemperature = (int)Math.Round(fanControl.CpuTemperature);

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
                            this.fanControl.Start();
                            Settings.Default.AutoStart = this.fanControl.Enabled;
                            Settings.Default.Save();
                        }
                    }
                }
                else if (!this.fanControl.Enabled)
                {
                    this.fanControl.Start();
                    Settings.Default.AutoStart = this.fanControl.Enabled;
                    Settings.Default.Save();
                }
            }

            return Settings.Default.AutoStart;
        }

        public void Stop()
        {
            if (!this.disposed && fanControl != null)
            {
                try
                {
                    Settings.Default.AutoStart = false;
                    Settings.Default.TargetFanSpeeds = GetTargetFanSpeeds(this.fanControl.FanInformation);
                    Settings.Default.Save();
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
                //TODO: check if config exists

                Settings.Default.SelectedConfigId = configUniqueId;
                Settings.Default.Save();

                if (this.fanControl != null)
                {
                    this.fanControl.Dispose();
                    this.fanControl = null;
                }

                Start();
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
            this.fanControl.ReInitialize();
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
                fanControl = new FanControl(cfg);
                float[] speeds = Settings.Default.TargetFanSpeeds;

                if (speeds != null && speeds.Length == fanControl.FanInformation.Count)
                {
                    for (int i = 0; i < speeds.Length; i++)
                    {
                        fanControl.SetTargetFanSpeed(speeds[i], i);
                    }
                }
                success = true;
            }
            catch
            {
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
            var configManager = new FanControlConfigManager(FanControlService.configsDirectory);
            string id = Settings.Default.SelectedConfigId;

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
