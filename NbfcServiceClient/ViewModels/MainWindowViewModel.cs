using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace NbfcServiceClient.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Private Fields

        private bool isServiceAvailable;
        private int cpuTemperature;
        private string selectedConfig;
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
                return
                    selectedConfig;
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
                return
                    isServiceAvailable;
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

        public int CpuTemperature
        {
            get
            {
                return cpuTemperature;
            }

            set
            {
                if (cpuTemperature != value)
                {
                    cpuTemperature = value;
                    OnPropertyChanged("CpuTemperature");
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

        #region Private Methods

        #endregion
    }
}
