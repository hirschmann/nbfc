using StagWare.Settings;
using System.Windows.Media;

namespace NbfcClient
{
    public class AppSettings : SettingsBase
    {
        public AppSettings()
        {
            SettingsVersion = 0;
        }

        [DefaultValue(typeof(XmlWrapper<Color>), "White")]
        public XmlWrapper<Color> TrayIconForegroundColor { get; set; }

        [DefaultValue(false)]
        public bool CloseToTray { get; set; }

        [DefaultValue(350.0)]
        public double WindowHeight { get; set; }

        [DefaultValue(430.0)]
        public double WindowWidth { get; set; }
    }
}
