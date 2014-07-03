using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConfigEditor.ViewModels
{
    public class FanSpeedOverrideViewModel : ViewModelBase, ICloneable
    {
        #region Private Fields

        private int fanSpeedValue;
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

        public int FanSpeedValue
        {
            get
            {
                return fanSpeedValue;
            }

            set
            {
                if (fanSpeedValue != value)
                {
                    fanSpeedValue = value;
                    OnPropertyChanged("FanSpeedValue");
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
            return new FanSpeedOverrideViewModel()
            {
                FanSpeedPercentage = this.FanSpeedPercentage,
                FanSpeedValue = this.FanSpeedValue,
                Parent = this.Parent
            };
        }

        #endregion
    }
}
