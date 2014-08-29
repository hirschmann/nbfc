using System;

namespace StagWare.FanControl.Configurations
{
    public class FanSpeedPercentageOverride : ICloneable
    {
        public float FanSpeedPercentage { get; set; }
        public int FanSpeedValue { get; set; }

        #region ICloneable implementation
        
        public object Clone()
        {
            return new FanSpeedPercentageOverride()
            {
                FanSpeedPercentage = this.FanSpeedPercentage,
                FanSpeedValue = this.FanSpeedValue
            };
        } 

        #endregion
    }
}
