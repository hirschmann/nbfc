using StagWare.FanControl.Configurations;
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
        private float fanSpeedPercentage;
        private OverrideTargetOperation targetOperation;
        private FanConfigViewModel parent;

        #endregion

        public FanSpeedOverrideViewModel()
        {
            this.TargetOperation = OverrideTargetOperation.ReadWrite;
        }

        #region Properties

        public float FanSpeedPercentage
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

        public OverrideTargetOperation TargetOperation
        {
            get { return targetOperation; }
            set
            {
                if (targetOperation != value)
                {
                    targetOperation = value;
                    OnPropertyChanged("TargetOperation");
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

        public float SliderValue
        {
            get { return (Parent.FanSpeedSteps * FanSpeedPercentage) / 100.0f; }
            set { FanSpeedPercentage = (value / Parent.FanSpeedSteps) * 100.0f; }
        }

        #endregion

        #region ICloneable implementation

        public object Clone()
        {
            return new FanSpeedOverrideViewModel()
            {
                FanSpeedPercentage = this.FanSpeedPercentage,
                FanSpeedValue = this.FanSpeedValue,
                TargetOperation = this.TargetOperation,
                Parent = this.Parent
            };
        }

        #endregion
    }
}
