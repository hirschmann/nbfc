using StagWare.FanControl.Plugins;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
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

        const string SourcesFileName = "StagWare.Plugins.FSTemperatureMonitor.sources";

        #endregion

        #region Private Fields

        private string[] sourceFilePaths;

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
                string sourcesFile = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                sourcesFile = Path.Combine(sourcesFile, SourcesFileName);
                var paths = new List<string>();

                foreach (string s in File.ReadAllLines(sourcesFile))
                {
                    string src = s.Trim();

                    if (File.Exists(src))
                    {
                        GetTemperature(src);
                        paths.Add(src);
                    }
                }

                this.sourceFilePaths = paths.ToArray();
                this.IsInitialized = true;
            }
        }

        public double GetTemperature()
        {
            double temp = 0;

            foreach (string path in this.sourceFilePaths)
            {
                temp += GetTemperature(path);
            }

            return temp / this.sourceFilePaths.Length;
        }

        public void Dispose()
        {
        }

        #endregion

        #region Private Methods

        private static double GetTemperature(string sourceFilePath)
        {
            double? temp = null;
            IOException lastException = null;

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    temp = double.Parse(File.ReadAllText(sourceFilePath)) / 1000.0;
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

        #endregion
    }
}
