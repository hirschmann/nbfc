using StagWare.FanControl.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;

namespace StagWare.Plugins.Generic
{
    [Export(typeof(ITemperatureMonitor))]
    [FanControlPluginMetadata(
        "StagWare.Plugins.FSTemperatureMonitor",
        SupportedPlatforms.Windows | SupportedPlatforms.Unix,
        SupportedCpuArchitectures.x86 | SupportedCpuArchitectures.x64)]
    public class FSTemperatureMonitor : ITemperatureMonitor
    {
        #region Constants

        private const string SettingsFileName = "StagWare.Plugins.FSTemperatureMonitor.sources";
        private const string SettingsFolderName = "NbfcService";

        private const string LinuxHwmonDirectory = "/sys/class/hwmon/hwmon{0}/";
        private const string LinuxTempSensorFileName = "temp{1}_input";
        private const string LinuxTempSensorNameFileName = "name";
        private static readonly string[] LinuxTempSensorNames = { "coretemp", "k10temp" };

        #endregion

        #region Private Fields

        private TemperatureSource[] sources;

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

                if (!File.Exists(settingsFile))
                {
                    CreateSettingsFile(settingsFile);
                }

                var list = new List<TemperatureSource>();

                foreach (string s in File.ReadAllLines(settingsFile))
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
                            double.TryParse(arr[1], out multi);
                        }

                        list.Add(new TemperatureSource(arr[0], multi));
                    }
                }

                this.sources = list.ToArray();
                this.IsInitialized = true;
            }
        }

        private void CreateSettingsFile(string settingsFile)
        {
            File.Create(settingsFile);

            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                int i = 0;
                string dir = string.Format(LinuxHwmonDirectory, i);

                while (Directory.Exists(dir))
                {
                    int j = 0;
                    string file = Path.Combine(dir, LinuxTempSensorNameFileName);

                    if (LinuxTempSensorNames.Contains(File.ReadAllText(file).Trim()))
                    {
                        var lines = new List<string>();
                        file = Path.Combine(dir, string.Format(LinuxTempSensorFileName, j));

                        while (File.Exists(file))
                        {
                            lines.Add(string.Format("{0};{1}", file, 0.001));

                            j++;
                            file = Path.Combine(dir, string.Format(LinuxTempSensorFileName, j));
                        }

                        if (lines.Count > 0)
                        {
                            File.AppendAllLines(settingsFile, lines);
                            return;
                        }
                    }

                    i++;
                    string.Format(LinuxHwmonDirectory, i);
                }
            }
        }

        public double GetTemperature()
        {
            double temp = 0;

            foreach (TemperatureSource src in this.sources)
            {
                temp += GetTemperature(src.FilePath, src.Multiplier);
            }

            return temp / this.sources.Length;
        }

        public void Dispose()
        {
        }

        #endregion

        #region Private Methods

        private static double GetTemperature(string sourceFilePath, double multiplier)
        {
            double? temp = null;
            IOException lastException = null;

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    temp = double.Parse(File.ReadAllText(sourceFilePath)) * multiplier;
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

        #endregion
    }
}
