using StagWare.Settings;

namespace StagWare
{
    public class ServiceSettings : SettingsBase
    {
        public ServiceSettings()
        {
            SettingsVersion = 0;
        }

        public string SelectedConfigId { get; set; }
        public bool Autostart { get; set; }
        public bool ReadOnly { get; set; }
        public float[] TargetFanSpeeds { get; set; }
    }
}
