using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using NbfcClient.Messages;
using NbfcClient.Properties;
using StagWare.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

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

            trayIconColor = Settings.Default.TrayIconForegroundColor;
            closeToTray = Settings.Default.CloseToTray;
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
                    Settings.Default.CloseToTray = value;
                    Settings.Default.Save();
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
                    Settings.Default.TrayIconForegroundColor = value;
                    Settings.Default.Save();

                    // Make the main view model update the tray icon
                    Messenger.Default.Send(new ReloadFanControlInfoMessage(false));
                }
            }
        }

        public Color[] AvailableColors { get { return availableColors; } }

        #endregion
    }
}
