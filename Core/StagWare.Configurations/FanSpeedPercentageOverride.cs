using System;

namespace StagWare.FanControl.Configurations
{
    [Flags]
    public enum OverrideTargetOperation
    {
        Read = 1,
        Write = 2,
        ReadWrite = Read | Write
    }

    public class FanSpeedPercentageOverride : ICloneable
    {
        public float FanSpeedPercentage { get; set; }
        public int FanSpeedValue { get; set; }
        public OverrideTargetOperation TargetOperation { get; set; }

        public FanSpeedPercentageOverride()
        {
            this.TargetOperation = OverrideTargetOperation.ReadWrite;
        }

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
