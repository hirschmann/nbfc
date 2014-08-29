using System;
using System.Collections.Generic;
using System.Linq;

namespace StagWare.FanControl.Configurations
{
    public class FanConfiguration : ICloneable
    {
        #region Constants

        private static readonly TemperatureThreshold[] DefaultThresholds =      
        {                
            new TemperatureThreshold(0, 0, 0),
            new TemperatureThreshold(60, 48, 10),
            new TemperatureThreshold(63, 55, 20),
            new TemperatureThreshold(66, 59, 50),
            new TemperatureThreshold(68, 63, 70),
            new TemperatureThreshold(71, 67, 100)
        };

        #endregion

        #region Properties

        public int ReadRegister { get; set; }
        public int WriteRegister { get; set; }
        public int MinSpeedValue { get; set; }
        public int MaxSpeedValue { get; set; }
        public bool ResetRequired { get; set; }
        public int FanSpeedResetValue { get; set; }
        public string FanDisplayName { get; set; }
        public List<TemperatureThreshold> TemperatureThresholds { get; set; }
        public List<FanSpeedPercentageOverride> FanSpeedPercentageOverrides { get; set; }

        #region Static

        public static List<TemperatureThreshold> DefaultTemperatureThresholds
        {
            get
            {
                return DefaultThresholds.Select(x => x.Clone() as TemperatureThreshold).ToList();
            }
        }

        #endregion

        #endregion

        #region Constructors

        public FanConfiguration()
        {
            this.TemperatureThresholds = new List<TemperatureThreshold>();
            this.FanSpeedPercentageOverrides = new List<FanSpeedPercentageOverride>();
        }

        #endregion

        #region ICloneable implementation

        public object Clone()
        {
            return new FanConfiguration()
            {
                ReadRegister = this.ReadRegister,
                WriteRegister = this.WriteRegister,
                MinSpeedValue = this.MinSpeedValue,
                MaxSpeedValue = this.MaxSpeedValue,
                ResetRequired = this.ResetRequired,
                FanSpeedResetValue = this.FanSpeedResetValue,
                FanDisplayName = this.FanDisplayName,
                TemperatureThresholds = this.TemperatureThresholds
                .Select(x => x.Clone() as TemperatureThreshold).ToList(),
                FanSpeedPercentageOverrides = this.FanSpeedPercentageOverrides
                .Select(x => x.Clone() as FanSpeedPercentageOverride).ToList()
            };
        }
        #endregion
    }
}
