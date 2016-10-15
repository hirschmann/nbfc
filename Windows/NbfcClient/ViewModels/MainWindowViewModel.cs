using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Linq;

namespace NbfcClient.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Private Fields

        private string version;
        private string selectedConfig;        
        private bool isServiceAvailable;
        private int temperature;
        private string temperatureSourceName;
        private ObservableCollection<FanControllerViewModel> fanControllers;

        #endregion

        #region Constructors

        public MainWindowViewModel()
        {
            this.FanControllers = new ObservableCollection<FanControllerViewModel>();
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
            set { this.Set(ref this.selectedConfig, value); }
        }

        public bool IsServiceAvailable
        {
            get { return this.isServiceAvailable; }
            set { this.Set(ref this.isServiceAvailable, value); }
        }        

        public int Temperature
        {
            get { return this.temperature; }
            set { this.Set(ref this.temperature, value); }
        }        

        public string TemperatureSourceName
        {
            get { return this.temperatureSourceName; }
            set { this.Set(ref this.temperatureSourceName, value); }
        }        

        public ObservableCollection<FanControllerViewModel> FanControllers
        {
            get { return this.fanControllers; }
            set { this.Set(ref this.fanControllers, value); }
        }

        #endregion

        #region Private Methods

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
    }
}
