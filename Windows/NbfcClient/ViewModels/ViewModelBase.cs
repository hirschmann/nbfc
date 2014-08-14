using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace NbfcClient.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        private string version;

        public string Version
        {
            get
            {
                if (string.IsNullOrEmpty(this.version))
                {
                    this.version = Application.ProductVersion;
                    int idx = this.version.LastIndexOf('.');

                    if (idx >= 0)
                    {
                        this.version = this.version.Remove(idx);
                    }
                }

                return version;
            }
        }

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
