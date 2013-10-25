using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StagWare.FanControl
{
    public class FanInformation : ICloneable
    {
        public double TargetFanSpeed { get; set; }
        public double CurrentFanSpeed { get; set; }
        public bool AutoFanControlEnabled { get; set; }
        public bool CriticalModeEnabled { get; set; }
        public string FanDisplayName { get; set; }

        public object Clone()
        {
            return new FanInformation()
            {
                TargetFanSpeed = this.TargetFanSpeed,
                CurrentFanSpeed = this.CurrentFanSpeed,
                AutoFanControlEnabled = this.AutoFanControlEnabled,
                CriticalModeEnabled = this.CriticalModeEnabled,
                FanDisplayName = this.FanDisplayName
            };
        }
    }
}
