using StagWare.FanControl.Configurations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace ConfigEditor.ViewModels
{
    public class RequestConfigNameViewModel : ViewModelBase
    {
        #region Private Fields

        private FanControlConfigManager configManager;
        private string configName;

        #endregion

        #region Constructors

        public RequestConfigNameViewModel(FanControlConfigManager configManager, string configNameDefault)
        {
            this.configManager = configManager;
            this.ConfigName = configNameDefault;

            UpdateIsConfigNameUniqueProperty();
            UpdateIsConfigNameValidProperty();
        }

        #endregion

        #region Properties

        public string ConfigName
        {
            get
            {
                return configName;
            }

            set
            {
                if (configName != value)
                {
                    configName = value;
                    OnPropertyChanged("ConfigName");

                    UpdateIsConfigNameUniqueProperty();
                    UpdateIsConfigNameValidProperty();
                }
            }
        }

        public bool IsConfigNameUnique { get; private set; }
        public bool IsConfigNameValid { get; private set; }

        #endregion

        #region Private Methods

        private void UpdateIsConfigNameUniqueProperty()
        {
            bool isUnique = configManager != null
                        && !configManager.Contains(this.ConfigName)
                        && !configManager.ConfigFileExists(this.ConfigName);

            if (isUnique != IsConfigNameUnique)
            {
                IsConfigNameUnique = isUnique;
                OnPropertyChanged("IsConfigNameUnique");
            }
        }

        private void UpdateIsConfigNameValidProperty()
        {
            bool isValid = !string.IsNullOrWhiteSpace(this.ConfigName)
                            && this.ConfigName.IndexOfAny(FanControlConfigManager.InvalidFileNameChars.ToArray()) == -1;

            if (isValid != IsConfigNameValid)
            {
                IsConfigNameValid = isValid;
                OnPropertyChanged("IsConfigNameValid");
            }
        }

        #endregion
    }
}
