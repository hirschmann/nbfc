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
        public float CurrentFanSpeed { get; set; }

        [DataMember]
        public float TargetFanSpeed { get; set; }

        [DataMember]
        public int FanSpeedSteps { get; set; }
    }
}
