using StagWare.FanControl.Plugins;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;

namespace StagWare.Linux.NbfcPlugins
{
    [Export(typeof(ITemperatureProvider))]
    [FanControlPluginMetadata(
        "StagWare.Linux.CpuTempProvider", 
        SupportedPlatforms.Unix,
        SupportedCpuArchitectures.x86 | SupportedCpuArchitectures.x64,
        MinOSVersion = "3.10")]
    public class CpuTemperatureProvider : ITemperatureProvider
    {
        #region Constants

        const string cpuTempFilePath = "/sys/class/hwmon/hwmon0/temp1_input";

        #endregion

        #region ITemperatureProvider implementation

        public bool IsInitialized
        {
            get;
            private set;
        }

        public void Initialize()
        {
            this.IsInitialized = true;
        }

        public double GetTemperature()
        {
            double temp = 0;

            if (double.TryParse(File.ReadAllText(cpuTempFilePath), out temp))
            {
                temp /= 1000;
            }

            return temp;
        }

        public void Dispose()
        {
        }

        #endregion
    }
}
