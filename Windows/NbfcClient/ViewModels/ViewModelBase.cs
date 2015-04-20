using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace NbfcClient.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        private string version;

        public string Version
        {
            get
            {
                if (this.version ==  null)
                {
                    this.version = GetInformationalVersionString();
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
