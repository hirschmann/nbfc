using StagWare.Configurations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using StagWare.ExtensionMethods;

namespace StagWare.FanControl.Configurations
{
    public class FanControlConfigManager : ConfigManager<FanControlConfigV2>
    {
        #region Constants

        private const string RemoveBracketsPattern = @"[\(\[\{].*?[\)\]\}]";

        #endregion

        #region Private Fields

        private readonly ReadOnlyDictionary<string, string> VendorAliases =
            new ReadOnlyDictionary<string, string>(new Dictionary<string, string>()
        {
            { "Hewlett-Packard", "HP" }
        });

        #endregion

        #region Properties

        public FanControlConfigV2 SelectedConfig { get; private set; }
        public string SelectedConfigName { get; private set; }
        public string DeviceModelName { get; private set; }

        #endregion

        #region Constructor

        public FanControlConfigManager(string configsDirPath)
            : base(configsDirPath)
        {
            DeviceModelName = GetDeviceModelName();
        }

        public FanControlConfigManager(string configsDirPath, string configFileExtension)
            : base(configsDirPath, configFileExtension)
        {
            DeviceModelName = GetDeviceModelName();
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

        public List<string> RecommendConfigs()
        {
            return RecommendConfigs(DeviceModelName);
        }

        public List<string> RecommendConfigs(string deviceModel)
        {
            deviceModel = Regex.Replace(deviceModel, RemoveBracketsPattern, "");

            if (string.IsNullOrWhiteSpace(deviceModel))
            {
                return new List<string>();
            }

            var dict = new Dictionary<string, Tuple<string, double>>();

            foreach (string cfgName in ConfigNames)
            {
                string cleanCfgName = Regex.Replace(cfgName, RemoveBracketsPattern, "");
                double similarity = GetSimilarityIndex(deviceModel, cleanCfgName);

                if (similarity >= 1)
                {
                    FanControlConfigV2 cfg = GetConfig(cleanCfgName);

                    if ((cfg.FanConfigurations == null) || (cfg.FanConfigurations.Count == 0))
                    {
                        continue;
                    }

                    string key = "";
                    cfg.FanConfigurations.ForEach(x => key += $"{x.ReadRegister}{x.WriteRegister}");

                    if (dict.ContainsKey(key))
                    {
                        Tuple<string, double> tuple = dict[key];

                        if (tuple.Item2 < similarity)
                        {
                            dict[key] = new Tuple<string, double>(cleanCfgName, similarity);
                        }
                    }
                    else
                    {
                        dict.Add(key, new Tuple<string, double>(cleanCfgName, similarity));
                    }
                }
            }

            return dict.Values
                .OrderByDescending(x => x.Item2)
                .Select(x => x.Item1)
                .ToList();
        }

        #endregion

        #region Private Methods

        private string GetDeviceModelName()
        {
            var biosInfo = BiosInfo.BiosInfo.Create();
            string sysName = biosInfo?.SystemName.Trim();

            if (sysName == null)
            {
                return null;
            }

            sysName = Regex.Replace(sysName, RemoveBracketsPattern, "").Trim();
            string vendor = biosInfo?.SystemVendor?.Trim();

            if (vendor != null)
            {
                if (VendorAliases.ContainsKey(vendor))
                {
                    vendor = VendorAliases[vendor];
                }

                if (!sysName.StartsWith(vendor, StringComparison.OrdinalIgnoreCase))
                {
                    sysName = $"{vendor} {sysName}";
                }
            }

            return sysName;
        }

        private static double GetSimilarityIndex(string modelName1, string modelName2)
        {
            double result = 0;
            modelName1 = modelName1.ToLower();
            modelName2 = modelName2.ToLower();

            foreach (string s1 in modelName1.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                double maxSimilarity = 0;

                foreach (string s2 in modelName2.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    int lcsLength = s1.GetLongestCommonSubstring(s2).Length;

                    if (lcsLength < 2)
                    {
                        continue;
                    }

                    double similarity = (double)lcsLength / Math.Max(s1.Length, s2.Length);
                    maxSimilarity = Math.Max(maxSimilarity, similarity);
                }

                result += maxSimilarity;
            }

            return result;
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
