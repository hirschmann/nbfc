using StagWare.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace StagWare.FanControl.Configurations
{
    public class FanControlConfigManager : ConfigManager<FanControlConfigV2>
    {
        #region Private Fields

        private static string notebookModel;

        #endregion

        #region Properties

        public FanControlConfigV2 SelectedConfig { get; private set; }
        public string SelectedConfigName { get; private set; }

        public static string NotebookModel
        {
            get
            {
                if (string.IsNullOrWhiteSpace(notebookModel))
                {
                    notebookModel = GetModelName();
                }

                return notebookModel;
            }
        }

        #endregion

        #region Constructor

        public FanControlConfigManager(string configsDirPath)
            : base(configsDirPath)
        {  
        }

        public FanControlConfigManager(string configsDirPath, string configFileExtension)
            : base(configsDirPath, configFileExtension)
        { 
        }

        #endregion

        #region Public Methods

        public bool AutoSelectConfig()
        {
            var pairs = KeyValuePairs.Where(x => x.Value.NotebookModel.Trim().Equals(
                    FanControlConfigManager.NotebookModel,
                    StringComparison.OrdinalIgnoreCase));

            if (pairs.Count() > 0)
            {
                var pair = pairs.First();

                this.SelectedConfigName = pair.Key;
                this.SelectedConfig = pair.Value;

                return true;
            }

            return false;
        }

        public bool SelectConfig(string configName)
        {
            if (Contains(configName))
            {
                SelectedConfig = GetConfig(configName);
                SelectedConfigName = configName;
            }
            else
            {
                SelectedConfig = null;
                SelectedConfigName = null;
            }

            return SelectedConfig != null;
        }

        #endregion

        #region Private Methods

        private static string GetModelName()
        {
            string model = string.Empty;

            using (var searcher = new ManagementObjectSearcher(@"SELECT * FROM CIM_ComputerSystem"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    try
                    {
                        model = obj["Model"].ToString();
                    }
                    catch
                    { 
                    }

                    if (!string.IsNullOrWhiteSpace(model))
                    {
                        break;
                    }
                }
            }

            return model.Trim();
        }

        #endregion

        #region Overrides

        public override void AddConfig(FanControlConfigV2 config, string configName)
        {
            base.AddConfig(config, configName);
            SelectConfig(configName);
        }

        public override void RemoveConfig(string configName)
        {            
            base.RemoveConfig(configName);

            if (configName == SelectedConfigName)
            {
                SelectedConfigName = null;
                SelectedConfig = null;
            }
        }

        public override void UpdateConfig(string configName, FanControlConfigV2 config)
        {
            base.UpdateConfig(configName, config);
            SelectConfig(configName);
        }

        #endregion
    }
}
