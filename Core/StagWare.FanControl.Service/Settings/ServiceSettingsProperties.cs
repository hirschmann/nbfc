namespace StagWare.Settings
{
    public sealed partial class ServiceSettings
    {
        public string SelectedConfigId { get; set; }
        public bool Autostart { get; set; }
        public bool ReadOnly { get; set; }
        public float[] TargetFanSpeeds { get; set; }
    }
}
