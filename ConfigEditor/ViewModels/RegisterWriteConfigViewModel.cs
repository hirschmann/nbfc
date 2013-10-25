using StagWare.FanControl.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConfigEditor.ViewModels
{
    public class RegisterWriteConfigViewModel : ViewModelBase, ICloneable
    {
        #region Private Fields

        private string description;
        private RegisterWriteMode writeMode;
        private RegisterWriteOccasion writeOccasion;
        private int register;
        private int val;
        private bool resetRequired;
        private int resetValue;

        #endregion

        #region Properties

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

        public int Value
        {
            get
            {
                return val;
            }

            set
            {
                if (val != value)
                {
                    val = value;
                    OnPropertyChanged("Value");
                }
            }
        }

        public int Register
        {
            get
            {
                return register;
            }

            set
            {
                if (register != value)
                {
                    register = value;
                    OnPropertyChanged("Register");
                }
            }
        }

        public RegisterWriteOccasion WriteOccasion
        {
            get
            {
                return writeOccasion;
            }

            set
            {
                if (writeOccasion != value)
                {
                    writeOccasion = value;
                    OnPropertyChanged("WriteOccasion");
                }
            }
        }

        public RegisterWriteMode WriteMode
        {
            get
            {
                return writeMode;
            }

            set
            {
                if (writeMode != value)
                {
                    writeMode = value;
                    OnPropertyChanged("WriteMode");
                }
            }
        }

        public string Description
        {
            get
            {
                return description;
            }

            set
            {
                if (description != value)
                {
                    description = value;
                    OnPropertyChanged("Description");
                }
            }
        }

        #endregion

        #region ICloneable implementation

        public object Clone()
        {
            return new RegisterWriteConfigViewModel()
            {
                Description = this.Description,
                Register = this.Register,
                Value = this.Value,
                WriteMode = this.WriteMode,
                WriteOccasion = this.WriteOccasion,
                ResetRequired = this.ResetRequired,
                ResetValue = this.ResetValue
            };
        }

        #endregion
    }
}
