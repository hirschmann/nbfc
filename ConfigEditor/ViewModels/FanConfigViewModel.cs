using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using StagWare.FanControl.Configurations;

namespace ConfigEditor.ViewModels
{
    public class FanConfigViewModel : ViewModelBase, ICloneable
    {
        #region Private Fields

        private string fanDisplayName;
        private int readRegister;
        private int writeRegister;
        private int minSpeedValue;
        private int maxSpeedValue;
        private bool resetRequired;
        private int resetValue;
        private ObservableCollection<TemperatureThresholdViewModel> temperatureThreshold;
        private ObservableCollection<FanSpeedOverrideViewModel> fanSpeedOverrides;
        private MainViewModel parent;

        #endregion

        #region Properties

        public MainViewModel Parent
        {
            get
            {
                return parent;
            }

            set
            {
                if (parent != value)
                {
                    parent = value;
                    OnPropertyChanged("Parent");
                }
            }
        }

        public ObservableCollection<FanSpeedOverrideViewModel> FanSpeedOverrides
        {
            get
            {
                return fanSpeedOverrides;
            }

            set
            {
                if (fanSpeedOverrides != value)
                {
                    fanSpeedOverrides = value;
                    OnPropertyChanged("Overrides");
                }
            }
        }

        public ObservableCollection<TemperatureThresholdViewModel> TemperatureThresholds
        {
            get
            {
                return temperatureThreshold;
            }

            set
            {
                if (temperatureThreshold != value)
                {
                    temperatureThreshold = value;
                    OnPropertyChanged("TemperatureThresholds");
                }
            }
        }

        public int ResetValue
        {
            get
            {
                return resetValue;
            }

            set
            {
                if (resetValue != value)
                {
                    resetValue = value;
                    OnPropertyChanged("ResetValue");
                }
            }
        }

        public bool ResetRequired
        {
            get
            {
                return resetRequired;
            }

            set
            {
                if (resetRequired != value)
                {
                    resetRequired = value;
                    OnPropertyChanged("ResetRequired");
                }
            }
        }

        public int MaxSpeedValue
        {
            get
            {
                return maxSpeedValue;
            }

            set
            {
                if (maxSpeedValue != value)
                {
                    maxSpeedValue = value;
                    OnPropertyChanged("MaxSpeedValue");
                    OnPropertyChanged("FanSpeedSteps");
                }
            }
        }

        public int MinSpeedValue
        {
            get
            {
                return minSpeedValue;
            }

            set
            {
                if (minSpeedValue != value)
                {
                    minSpeedValue = value;
                    OnPropertyChanged("MinSpeedValue");
                    OnPropertyChanged("FanSpeedSteps");
                }
            }
        }

        public int WriteRegister
        {
            get
            {
                return writeRegister;
            }

            set
            {
                if (writeRegister != value)
                {
                    writeRegister = value;
                    OnPropertyChanged("WriteRegister");
                }
            }
        }

        public int ReadRegister
        {
            get
            {
                return readRegister;
            }

            set
            {
                if (readRegister != value)
                {
                    readRegister = value;
                    OnPropertyChanged("ReadRegister");
                }
            }
        }

        public string FanDisplayName
        {
            get
            {
                return fanDisplayName;
            }

            set
            {
                if (fanDisplayName != value)
                {
                    fanDisplayName = value;
                    OnPropertyChanged("FanDisplayName");
                }
            }
        }

        public int FanSpeedSteps
        {
            get
            {
                return Math.Max(MaxSpeedValue, MinSpeedValue)
                    - Math.Min(MaxSpeedValue, MinSpeedValue);
            }
        }

        #endregion

        #region Constructors

        public FanConfigViewModel()
        {
            this.FanSpeedOverrides = new ObservableCollection<FanSpeedOverrideViewModel>();
            this.TemperatureThresholds = new ObservableCollection<TemperatureThresholdViewModel>();
        }

        #endregion

        #region ICloneable implementation

        public object Clone()
        {
            return new FanConfigViewModel()
            {
                Parent = this.Parent,
                FanDisplayName = this.FanDisplayName,
                ReadRegister = this.ReadRegister,
                WriteRegister = this.WriteRegister,
                MinSpeedValue = this.MinSpeedValue,
                MaxSpeedValue = this.MaxSpeedValue,
                ResetRequired = this.ResetRequired,
                ResetValue = this.ResetValue,

                TemperatureThresholds = new ObservableCollection<TemperatureThresholdViewModel>(
                    this.TemperatureThresholds.Select(x => x.Clone() as TemperatureThresholdViewModel)),

                FanSpeedOverrides = new ObservableCollection<FanSpeedOverrideViewModel>(
                    this.FanSpeedOverrides.Select(x => x.Clone() as FanSpeedOverrideViewModel))
            };
        }

        #endregion
    }
}
