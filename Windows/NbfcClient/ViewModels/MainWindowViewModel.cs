using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace NbfcClient.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Private Fields

        private bool isServiceAvailable;
        private int temperature;
        private string selectedConfig;
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

        public string SelectedConfig
        {
            get
            {
                return selectedConfig;
            }

            set
            {
                if (selectedConfig != value)
                {
                    selectedConfig = value;
                    OnPropertyChanged("SelectedConfig");
                }
            }
        }

        public bool IsServiceAvailable
        {
            get
            {
                return isServiceAvailable;
            }

            set
            {
                if (isServiceAvailable != value)
                {
                    isServiceAvailable = value;
                    OnPropertyChanged("IsServiceAvailable");
                }
            }
        }

        public int Temperature
        {
            get
            {
                return temperature;
            }

            set
            {
                if (temperature != value)
                {
                    temperature = value;
                    OnPropertyChanged("Temperature");
                }
            }
        }       

        public string TemperatureSourceName
        {
            get { return temperatureSourceName; }
            set
            {
                if (temperatureSourceName != value)
                {
                    temperatureSourceName = value;
                    OnPropertyChanged("TemperatureSourceName");
                }
            }
        }        

        public ObservableCollection<FanControllerViewModel> FanControllers
        {
            get
            {
                return fanControllers;
            }

            set
            {
                if (fanControllers != value)
                {
                    fanControllers = value;
                    OnPropertyChanged("FanControllers");
                }
            }
        }

        #endregion
    }
}
