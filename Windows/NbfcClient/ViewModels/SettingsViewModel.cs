using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using NbfcClient.Messages;
using StagWare.Windows;
using System.Linq;
using System.Windows.Media;
using SettingsService = StagWare.Settings.SettingsService<NbfcClient.AppSettings>;

namespace NbfcClient.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        #region Constants

        private const string RegistryAutorunValueName = "NBFC-ClientApplication";
        private const string StartInTrayParameter = "-tray";
        private readonly Color[] availableColors = typeof(Colors).GetProperties()
            .Select(x => (Color)x.GetValue(null)).ToArray();

        #endregion

        #region Private Fields

        private AutorunEntry autorun;

        private bool closeToTray;
        private bool autostart;
        private Color trayIconColor;

        #endregion

        #region Constructors

        public SettingsViewModel()
        {
            autorun = new AutorunEntry(RegistryAutorunValueName);
            autorun.Parameters = StartInTrayParameter;
            trayIconColor = SettingsService.Settings.TrayIconForegroundColor;
            closeToTray = SettingsService.Settings.CloseToTray;
            autostart = autorun.Exists;
        }

        #endregion

        #region Properties        

        public bool CloseToTray
        {
            get { return this.closeToTray; }
            set
            {
                if (Set(ref this.closeToTray, value))
                {
                    SettingsService.Settings.CloseToTray = value;
                    SettingsService.Save();
                }
            }
        }

        public bool Autostart
        {
            get { return this.autostart; }
            set
            {
                if (Set(ref this.autostart, value))
                {
                    autorun.Exists = value;
                }
            }
        }

        public Color TrayIconColor
        {
            get { return this.trayIconColor; }
            set
            {
                if (Set(ref this.trayIconColor, value))
                {
                    SettingsService.Settings.TrayIconForegroundColor = value;
                    SettingsService.Save();

                    // Make the main view model update the tray icon
                    Messenger.Default.Send(new ReloadFanControlInfoMessage(false));
                }
            }
        }

        public Color[] AvailableColors { get { return availableColors; } }

        #endregion
    }
}
