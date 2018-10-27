using StagWare.FanControl.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace StagWare.Plugins.Generic
{
    [Export(typeof(ITemperatureMonitor))]
    [FanControlPluginMetadata(
        "StagWare.Plugins.FSTemperatureMonitor",
        SupportedPlatforms.Windows | SupportedPlatforms.Unix,
        SupportedCpuArchitectures.x86 | SupportedCpuArchitectures.x64,
        FanControlPluginMetadataAttribute.DefaultPriority + 10)]
    public class FSTemperatureMonitor : ITemperatureMonitor
    {
        #region Constants

        private const string SettingsFileName = "StagWare.Plugins.FSTemperatureMonitor.sources";
        private const string SettingsFolderName = "NbfcService";

        private static readonly string[] LinuxHwmonDirs = { "/sys/class/hwmon/hwmon{0}/", "/sys/class/hwmon/hwmon{0}/device/" };
        private const string LinuxTempSensorFile = "temp{0}_input";
        private const string LinuxTempSensorNameFile = "name";
        private static readonly string[] LinuxTempSensorNames = { "coretemp", "k10temp" };

        #endregion

        #region Private Fields

        private List<TemperatureSource> sources;

        #endregion

        #region ITemperatureMonitor implementation

        public bool IsInitialized
        {
            get;
            private set;
        }

        public string TemperatureSourceDisplayName { get; private set; }

        public void Initialize()
        {
            if (!this.IsInitialized)
            {
                string settingsFile = GetSettingsFileName();

                if (File.Exists(settingsFile))
                {
                    this.sources = ReadSettingsFile(settingsFile);

                    if (this.sources.Count > 0)
                    {
                        this.IsInitialized = true;
                        return;
                    }            
                }

                this.sources = FindTemperatureSources();
                this.IsInitialized = (this.sources != null) && (this.sources.Count > 0);
            }
        }        

        public double GetTemperature()
        {
            double temp = 0;

            foreach (TemperatureSource src in this.sources)
            {
                temp += GetTemperature(src.FilePath, src.Multiplier);
            }

            return temp / this.sources.Count;
        }

        public void Dispose()
        {
        }

        #endregion

        #region Private Methods

        private static List<TemperatureSource> ReadSettingsFile(string settingsFilePath)
        {
            var list = new List<TemperatureSource>();

            foreach (string s in File.ReadAllLines(settingsFilePath))
            {
                string[] arr = s.Split(';');

                if (arr.Length > 0 && !string.IsNullOrWhiteSpace(arr[0]) && File.Exists(arr[0]))
                {
                    try
                    {
                        GetTemperature(arr[0], 1);
                    }
                    catch
                    {
                        continue;
                    }

                    double multi = 1;

                    if (arr.Length > 1)
                    {
                        double.TryParse(arr[1], NumberStyles.Number, CultureInfo.InvariantCulture, out multi);
                    }

                    list.Add(new TemperatureSource(arr[0], multi));
                }
            }

            return list;
        }

        private static double GetTemperature(string sourceFilePath, double multiplier)
        {
            double? temp = null;
            Exception lastException = null;

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    string src = File.ReadAllText(sourceFilePath);
                    temp = double.Parse(src, NumberStyles.Number, CultureInfo.InvariantCulture) * multiplier;
                    break;
                }
                catch (FormatException e)
                {
                    lastException = e;
                    break;
                }
                catch (IOException e)
                {
                    lastException = e;
                }

                Thread.Sleep(50);
            }

            if (!temp.HasValue)
            {
                throw lastException;
            }
            else
            {
                return temp.Value;
            }
        }

        private static string GetSettingsFileName()
        {
            string dir = "";

            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                dir = "/etc/";
            }
            else
            {
                dir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            }

            string settingsFile = Path.Combine(
                dir,
                SettingsFolderName,
                SettingsFileName);

            return settingsFile;
        }

        private static List<TemperatureSource> FindTemperatureSources()
        {
            var tempSources = new List<TemperatureSource>();

            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                for (int i = 0; i < 10; i++)
                {
                    foreach (string s in LinuxHwmonDirs)
                    {
                        string dir = string.Format(s, i);
                        string sensorNameFile = Path.Combine(dir, LinuxTempSensorNameFile);

                        if (!Directory.Exists(dir) || !File.Exists(sensorNameFile))
                        {
                            continue;
                        }

                        string sensorName = File.ReadAllText(sensorNameFile).Trim();

                        if (LinuxTempSensorNames.Contains(sensorName))
                        {
                            for (int j = 0; j < 10; j++)
                            {
                                string sensorFile = Path.Combine(dir, string.Format(LinuxTempSensorFile, j));

                                if (File.Exists(sensorFile))
                                {
                                    try
                                    {
                                        GetTemperature(sensorFile, 0.001);
                                        var src = new TemperatureSource(sensorFile, 0.001);
                                        tempSources.Add(src);
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                            }

                            if (tempSources.Count > 0)
                            {
                                return tempSources;
                            }
                        }
                    }
                }
            }

            return tempSources;
        }

        #endregion
    }
}
