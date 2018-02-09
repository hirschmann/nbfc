using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using NbfcClient.Messages;
using NbfcClient.NbfcService;
using NbfcClient.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;
using SettingsService = StagWare.Settings.SettingsService<NbfcClient.AppSettings>;

namespace NbfcClient.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Private Fields

        private TrayIconRenderer renderer;
        private BitmapSource trayIcon;
        private IFanControlClient client;

        private string version;
        private string selectedConfig;
        private bool isServiceReadOnly;
        private bool isServiceDisabled;
        private bool isServiceEnabled;
        private int temperature;
        private string temperatureSourceName;
        private ObservableCollection<FanControllerViewModel> fanControllers;

        private RelayCommand selectConfigCommand;
        RelayCommand settingsCommand;

        #endregion

        #region Constructors

        public MainViewModel(IFanControlClient client)
        {
            this.FanControllers = new ObservableCollection<FanControllerViewModel>();
            this.renderer = new TrayIconRenderer();
            this.renderer.Color = SettingsService.Settings.TrayIconForegroundColor;
            this.client = client;
            this.client.FanControlStatusChanged += Client_FanControlStatusChanged;
            Messenger.Default.Register<ReloadFanControlInfoMessage>(this, Refresh);
            Refresh(true);
        }

        #endregion

        #region Properties

        public string Version
        {
            get
            {
                if (this.version == null)
                {
                    this.version = GetInformationalVersionString();
                }

                return version;
            }
        }

        public string SelectedConfig
        {
            get { return this.selectedConfig; }
            private set { this.Set(ref this.selectedConfig, value); }
        }

        public bool IsServiceDisabled
        {
            get { return this.isServiceDisabled; }
            set
            {
                if (Set(ref this.isServiceDisabled, value) && value)
                {
                    client.Stop();
                    IsServiceReadOnly = false;
                    IsServiceEnabled = false;
                    Refresh(true);
                }
            }
        }

        public bool IsServiceReadOnly
        {
            get { return this.isServiceReadOnly; }
            set
            {
                if (Set(ref this.isServiceReadOnly, value) && value)
                {
                    client.Start(true);
                    IsServiceDisabled = false;
                    IsServiceEnabled = false;
                    Refresh(true);
                }
            }
        }

        public bool IsServiceEnabled
        {
            get { return this.isServiceEnabled; }
            set
            {
                if (Set(ref this.isServiceEnabled, value) && value)
                {
                    client.Start(false);
                    IsServiceDisabled = false;
                    IsServiceReadOnly = false;
                    Refresh(true);
                }
            }
        }

        public int Temperature
        {
            get { return this.temperature; }
            private set { this.Set(ref this.temperature, value); }
        }

        public string TemperatureSourceName
        {
            get { return this.temperatureSourceName; }
            private set { this.Set(ref this.temperatureSourceName, value); }
        }

        public ObservableCollection<FanControllerViewModel> FanControllers
        {
            get { return this.fanControllers; }
            private set { this.Set(ref this.fanControllers, value); }
        }

        public BitmapSource TrayIcon
        {
            get { return this.trayIcon; }
            private set { this.Set(ref this.trayIcon, value); }
        }

        #endregion

        #region Commands       

        public RelayCommand SelectConfigCommand
        {
            get
            {
                if (this.selectConfigCommand == null)
                {
                    this.selectConfigCommand = new RelayCommand(SendOpenSelectConfigDialogMessage);
                }

                return this.selectConfigCommand;
            }
        }

        public RelayCommand SettingsCommand
        {
            get
            {
                if (this.settingsCommand == null)
                {
                    this.settingsCommand = new RelayCommand(SendOpenSettingsDialogMessage);
                }

                return this.settingsCommand;
            }
        }

        #endregion

        #region Private Methods

        private void Refresh(ReloadFanControlInfoMessage msg)
        {
            Refresh(msg.IgnoreCache);
        }

        private void Refresh(bool ignoreCache)
        {
            FanControlInfo info = ignoreCache
                ? client.GetFanControlInfo()
                : client.FanControlInfo;

            UpdateNotifyIcon(info.Temperature);
            UpdateProperties(info);
        }

        private void UpdateNotifyIcon(int temperature)
        {
            this.renderer.Color = SettingsService.Settings.TrayIconForegroundColor;
            TrayIcon = this.renderer.RenderIcon(temperature.ToString());
        }

        private void UpdateProperties(FanControlInfo info)
        {
            Set(ref isServiceDisabled, !info.Enabled, nameof(IsServiceDisabled));
            Set(ref isServiceReadOnly, (info.Enabled && info.ReadOnly), nameof(IsServiceReadOnly));
            Set(ref isServiceEnabled, (info.Enabled && !info.ReadOnly), nameof(IsServiceEnabled));
            Set(ref temperature, info.Temperature, nameof(Temperature));
            Set(ref selectedConfig, info.SelectedConfig, nameof(SelectedConfig));
            Set(ref temperatureSourceName, info.TemperatureSourceDisplayName, nameof(TemperatureSourceName));

            if (info.FanStatus == null)
            {
                this.fanControllers.Clear();
            }
            else if (this.fanControllers.Count != info.FanStatus.Length)
            {
                this.fanControllers.Clear();

                for (int i = 0; i < info.FanStatus.Length; i++)
                {
                    fanControllers.Add(new FanControllerViewModel(client, i));
                }
            }
        }

        private static void SendOpenSelectConfigDialogMessage()
        {
            Messenger.Default.Send(new OpenSelectConfigDialogMessage());
        }

        private static void SendOpenSettingsDialogMessage()
        {
            Messenger.Default.Send(new OpenSettingsDialogMessage());
        }

        private static string GetInformationalVersionString()
        {
            var attribute = (AssemblyInformationalVersionAttribute)Assembly.GetExecutingAssembly()
                .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
                .FirstOrDefault();

            if (attribute == null)
            {
                return string.Empty;
            }
            else
            {
                return attribute.InformationalVersion;
            }
        }

        #endregion        

        #region EventHandlers

        private void Client_FanControlStatusChanged(object sender, FanControlStatusChangedEventArgs e)
        {
            Refresh(false);
        }

        #endregion
    }
}
