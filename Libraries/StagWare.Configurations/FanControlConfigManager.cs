using StagWare.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace StagWare.FanControl.Configurations
{
    public class FanControlConfigManager : ConfigManager<FanControlConfigV2>
    {
        #region Properties

        public FanControlConfigV2 SelectedConfig { get; private set; }
        public string SelectedConfigName { get; private set; }

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
