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

        private const int AutoFanSpeedPercentage = 101;
        private const int RPMConstant = 245760;
        private const int CriticalTemperatureOffset = 15;

        #endregion

        #region Private Fields

        private readonly int criticalTemperature;
        private TemperatureThresholdManager threshMan;
        private FanConfiguration fanConfig;
        private Dictionary<double, int> overriddenPercentages;
        private Dictionary<int, double> overriddenValues;

        private double fanSpeedPercentage;
        private int fanSpeedValue;

        #endregion

        #region Properties

        public double FanSpeedPercentage
        {
            get
            {
                return this.CriticalModeEnabled ?
                    100 : this.fanSpeedPercentage;
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

        #endregion

        #region Constructors

        public FanSpeedManager(FanConfiguration config, int criticalTemperature)
        {
            this.fanConfig = config;
            this.criticalTemperature = criticalTemperature;            
            this.overriddenPercentages = new Dictionary<double, int>();
            this.overriddenValues = new Dictionary<int, double>();

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
                    this.overriddenPercentages.Add(o.FanSpeedPercentage, o.FanSpeedValue);
                }

                if (!this.overriddenValues.ContainsKey(o.FanSpeedValue))
                {
                    this.overriddenValues.Add(o.FanSpeedValue, o.FanSpeedPercentage);
                }
            }
        }

        #endregion

        #region Public Methods

        public void UpdateFanSpeed(double speed, int cpuTemperature)
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

        public void UpdateFanSpeed(int cpuTemperature, bool autoSelectSpeed = false)
        {
            double speed = autoSelectSpeed ?
                AutoFanSpeedPercentage : this.fanSpeedPercentage;

            UpdateFanSpeed(speed, cpuTemperature);
        }

        #endregion

        #region Public Methods

        public int PercentageToFanSpeed(double percentage)
        {
            if ((percentage > 100) || (percentage < 0))
            {
                throw new ArgumentOutOfRangeException(
                    "percentage", 
                    "Percentage must be greater or equal 0 and less or equal 100");
            }

            if (this.overriddenPercentages.ContainsKey(percentage))
            {
                return this.overriddenPercentages[percentage];
            }
            else
            {
                return (int)Math.Round(
                    ((percentage / 100.0)
                    * (this.fanConfig.MaxSpeedValue - this.fanConfig.MinSpeedValue)) 
                    + this.fanConfig.MinSpeedValue);
            }
        }

        public double FanSpeedToPercentage(int fanSpeed)
        {
            if (this.overriddenValues.ContainsKey(fanSpeed))
            {
                return this.overriddenValues[fanSpeed];
            }
            else
            {
                if (this.fanConfig.MinSpeedValue == this.fanConfig.MaxSpeedValue)
                {
                    return 0;
                }
                else
                {
                    return (double)(fanSpeed - this.fanConfig.MinSpeedValue)
                        / (double)(this.fanConfig.MaxSpeedValue - this.fanConfig.MinSpeedValue) * 100;
                }
            }
        }

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

        #endregion
        
        #region Private Methods

        private void HandleCriticalMode(int cpuTemperature)
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
