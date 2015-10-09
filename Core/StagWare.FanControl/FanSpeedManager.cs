using StagWare.FanControl.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StagWare.FanControl
{
    internal class FanSpeedManager
    {
        #region Constants

        private const int RPMConstant = 245760;
        private const int CriticalTemperatureOffset = 15;

        #endregion

        #region Private Fields

        private readonly int criticalTemperature;
        private TemperatureThresholdManager threshMan;
        private FanConfiguration fanConfig;
        private Dictionary<float, FanSpeedPercentageOverride> overriddenPercentages;
        private Dictionary<int, FanSpeedPercentageOverride> overriddenValues;

        private float fanSpeedPercentage;
        private int fanSpeedValue;

        #endregion

        #region Properties

        public float FanSpeedPercentage
        {
            get
            {
                return this.CriticalModeEnabled
                    ? 100.0f
                    : this.fanSpeedPercentage;
            }
        }

        public int FanSpeedValue
        {
            get
            {
                return this.CriticalModeEnabled ?
                    this.fanConfig.MaxSpeedValue : this.fanSpeedValue;
            }
        }

        public bool AutoControlEnabled { get; private set; }
        public bool CriticalModeEnabled { get; private set; }
        public int MinSpeedValueWrite { get; private set; }
        public int MaxSpeedValueWrite { get; private set; }
        public int MinSpeedValueRead { get; private set; }
        public int MaxSpeedValueRead { get; private set; }
        public int MinSpeedValueReadAbs { get; private set; }
        public int MaxSpeedValueReadAbs { get; private set; }

        #endregion

        #region Constructors

        public FanSpeedManager(FanConfiguration config, int criticalTemperature)
        {
            this.fanConfig = config;
            this.criticalTemperature = criticalTemperature;
            this.overriddenPercentages = new Dictionary<float, FanSpeedPercentageOverride>();
            this.overriddenValues = new Dictionary<int, FanSpeedPercentageOverride>();

            this.MinSpeedValueWrite = config.MinSpeedValue;
            this.MaxSpeedValueWrite = config.MaxSpeedValue;

            if (config.IndependentReadMinMaxValues)
            {
                this.MinSpeedValueRead = config.MinSpeedValueRead;
                this.MaxSpeedValueRead = config.MaxSpeedValueRead;
            }
            else
            {
                this.MinSpeedValueRead = this.MinSpeedValueWrite;
                this.MaxSpeedValueRead = this.MaxSpeedValueWrite;
            }

            this.MinSpeedValueReadAbs = Math.Min(this.MinSpeedValueRead, this.MaxSpeedValueRead);
            this.MinSpeedValueReadAbs = Math.Max(this.MinSpeedValueRead, this.MaxSpeedValueRead);

            if (config.TemperatureThresholds != null
                && config.TemperatureThresholds.Count > 0)
            {
                this.threshMan = new TemperatureThresholdManager(config.TemperatureThresholds);
            }
            else
            {
                this.threshMan = new TemperatureThresholdManager(FanConfiguration.DefaultTemperatureThresholds);
            }

            foreach (FanSpeedPercentageOverride o in config.FanSpeedPercentageOverrides)
            {
                if (!this.overriddenPercentages.ContainsKey(o.FanSpeedPercentage))
                {
                    this.overriddenPercentages.Add(o.FanSpeedPercentage, o);
                }

                if (!this.overriddenValues.ContainsKey(o.FanSpeedValue))
                {
                    this.overriddenValues.Add(o.FanSpeedValue, o);
                }
            }
        }

        #endregion

        #region Public Methods

        public static int FanSpeedToRpm(int fanSpeed)
        {
            if (fanSpeed == 0)
            {
                return 0;
            }
            else
            {
                return RPMConstant / fanSpeed;
            }
        }

        public void UpdateFanSpeed(float speed, float cpuTemperature)
        {
            HandleCriticalMode(cpuTemperature);

            this.AutoControlEnabled = (speed < 0) || (speed > 100);

            if (AutoControlEnabled)
            {
                var threshold = this.threshMan.AutoSelectThreshold(cpuTemperature);

                if (threshold != null)
                {
                    this.fanSpeedPercentage = threshold.FanSpeed;
                }
            }
            else
            {
                this.fanSpeedPercentage = speed;
            }

            this.fanSpeedValue = PercentageToFanSpeed(this.fanSpeedPercentage);
        }

        public int PercentageToFanSpeed(float percentage)
        {
            if ((percentage > 100) || (percentage < 0))
            {
                throw new ArgumentOutOfRangeException(
                    "percentage",
                    "Percentage must be greater or equal 0 and less or equal 100");
            }

            if (this.overriddenPercentages.ContainsKey(percentage)
                && this.overriddenPercentages[percentage].TargetOperation.HasFlag(OverrideTargetOperation.Write))
            {
                return this.overriddenPercentages[percentage].FanSpeedValue;
            }
            else
            {
                return (int)Math.Round(
                    ((percentage / 100.0) * (MaxSpeedValueWrite - MinSpeedValueWrite))
                    + MinSpeedValueWrite);
            }
        }

        public float FanSpeedToPercentage(int fanSpeed)
        {
            if (this.overriddenValues.ContainsKey(fanSpeed)
                && this.overriddenPercentages[fanSpeed].TargetOperation.HasFlag(OverrideTargetOperation.Read))
            {
                return this.overriddenValues[fanSpeed].FanSpeedPercentage;
            }
            else
            {
                if (MinSpeedValueRead == MaxSpeedValueRead)
                {
                    return 0;
                }
                else
                {
                    return (float)(fanSpeed - MinSpeedValueRead)
                        / (float)(MaxSpeedValueRead - MinSpeedValueRead) * 100;
                }
            }
        }

        #endregion

        #region Private Methods

        private void HandleCriticalMode(double cpuTemperature)
        {
            if (this.CriticalModeEnabled
                && (cpuTemperature < this.criticalTemperature - CriticalTemperatureOffset))
            {
                this.CriticalModeEnabled = false;
            }
            else if (cpuTemperature > this.criticalTemperature)
            {
                this.CriticalModeEnabled = true;
            }
        }

        #endregion
    }
}
