using System.Runtime.Serialization;

namespace StagWare.FanControl.Service
{
    [DataContract]
    public class FanControlInfo
    {
        [DataMember]
        public bool Enabled { get; set; }

        [DataMember]
        public FanStatus[] FanStatus { get; set; }

        [DataMember]
        public int CpuTemperature { get; set; }

        [DataMember]
        public string SelectedConfig { get; set; }
    }
}
