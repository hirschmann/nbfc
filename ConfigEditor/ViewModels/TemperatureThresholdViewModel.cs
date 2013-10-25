using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConfigEditor.ViewModels
{
    public class TemperatureThresholdViewModel : ViewModelBase, ICloneable
    {
        #region Private Fields

        private int upThreshold;
        private int downThreshold;
        private double fanSpeedPercentage;
        private FanConfigViewModel parent;

        #endregion

        #region Properties

        public double FanSpeedPercentage
        {
            get
            {
                return fanSpeedPercentage;
            }

            set
            {
                if (fanSpeedPercentage != value)
                {
                    fanSpeedPercentage = value;
                    OnPropertyChanged("FanSpeedPercentage");
                    OnPropertyChanged("SliderValue");
                }
            }
        }

        public int DownThreshold
        {
            get
            {
                return downThreshold;
            }

            set
            {
                if (downThreshold != value)
                {
                    downThreshold = value;
                    OnPropertyChanged("DownThreshold");
                }
            }
        }

        public int UpThreshold
        {
            get
            {
                return upThreshold;
            }

            set
            {
                if (upThreshold != value)
                {
                    upThreshold = value;
                    OnPropertyChanged("UpThreshold");
                }
            }
        }

        public FanConfigViewModel Parent
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

        public double SliderValue
        {
            get { return (Parent.FanSpeedSteps * FanSpeedPercentage) / 100.0; }
            set { FanSpeedPercentage = (value / Parent.FanSpeedSteps) * 100.0; }
        }

        #endregion

        #region ICloneable implementation

        public object Clone()
        {
            return new TemperatureThresholdViewModel()
            {
                DownThreshold = this.DownThreshold,
                UpThreshold = this.UpThreshold,
                FanSpeedPercentage = this.FanSpeedPercentage,
                Parent = this.Parent
            };
        }

        #endregion
    }
}
