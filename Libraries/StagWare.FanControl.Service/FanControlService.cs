using StagWare.FanControl.Configurations;
using StagWare.FanControl.Service.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private FanControl fanControl;
        private bool initialized;
        private string selectedConfig;
        private int[] fanSpeedSteps;
        private string executingAssemblyDirName;

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

            executingAssemblyDirName = Assembly.GetExecutingAssembly().Location;
            executingAssemblyDirName = Path.GetDirectoryName(executingAssemblyDirName);

            if (Settings.Default.AutoStart && TryInitializeFanControl())
            {
                this.initialized = true;
                this.fanControl.Start();
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
            Stop();
            return Start();
        }

        public void Stop()
        {
            if (this.initialized)
            {
                initialized = false;
                DisposeFanControl();
            }
        }

        public bool Start()
        {
            if (!this.initialized && TryInitializeFanControl())
            {
                this.initialized = true;
                this.fanControl.Start();
                Settings.Default.AutoStart = true;
                Settings.Default.Save();
            }

            return this.initialized;
        }

        public void SetConfig(string configUniqueId)
        {
            Settings.Default.SelectedConfigId = configUniqueId;
            Settings.Default.Save();

            Restart();
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
                    Settings.Default.AutoStart = this.initialized;
                    Settings.Default.TargetFanSpeeds = GetTargetFanSpeeds(this.fanControl.FanInformation);
                    Settings.Default.Save();
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

        private static float[] GetTargetFanSpeeds(IEnumerable<FanInformation> fanInfos)
        {
            return fanInfos.Select(
                x => x.AutoFanControlEnabled
                    ? FanControl.AutoFanSpeedPercentage
                    : x.TargetFanSpeed).ToArray();
        }

        private bool TryInitializeFanControl()
        {
            bool success = false;

            try
            {
                FanControlConfigV2 cfg;

                if (TryLoadConfig(out cfg))
                {
                    InitializeFanSpeedSteps(cfg);
                    InitializeFanControl(cfg);
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

        private void InitializeFanControl(FanControlConfigV2 cfg)
        {
            this.fanControl = new FanControl(cfg);
            float[] speeds = Settings.Default.TargetFanSpeeds;

            if (speeds != null && speeds.Length == this.fanControl.FanInformation.Count)
            {
                for (int i = 0; i < speeds.Length; i++)
                {
                    fanControl.SetTargetFanSpeed(speeds[i], i);
                }
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

        private bool TryLoadConfig(out FanControlConfigV2 config)
        {
            bool result = false;
            string path = Path.Combine(executingAssemblyDirName, ConfigsDirectoryName);
            var configManager = new FanControlConfigManager(path);
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
