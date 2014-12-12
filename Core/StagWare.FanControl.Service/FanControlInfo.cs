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
        public int Temperature { get; set; }

        [DataMember]
        public string TemperatureSourceDisplayName { get; set; }

        [DataMember]
        public string SelectedConfig { get; set; }
    }
}
