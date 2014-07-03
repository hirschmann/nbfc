using System.Runtime.Serialization;

namespace StagWare.FanControl.Service
{
    [DataContract]
    public class FanStatus
    {
        [DataMember]
        public string FanDisplayName { get; set; }

        [DataMember]
        public bool AutoControlEnabled { get; set; }

        [DataMember]
        public bool CriticalModeEnabled { get; set; }

        [DataMember]
        public double CurrentFanSpeed { get; set; }

        [DataMember]
        public double TargetFanSpeed { get; set; }

        [DataMember]
        public int FanSpeedSteps { get; set; }
    }
}
