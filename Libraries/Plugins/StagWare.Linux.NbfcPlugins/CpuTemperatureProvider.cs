using StagWare.FanControl.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StagWare.Linux.NbfcPlugins
{
    [Export(typeof(ITemperatureProvider))]
    [FanControlPluginMetadata("StagWare.Linux.CpuTempProvider", PlatformID.Unix, MinOSVersion = "13.0")]
    public class CpuTemperatureProvider : ITemperatureProvider
    {
        const string cpuTempFilePath = "/sys/class/hwmon/hwmon0/cpu1_input";

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
                Debug.WriteLine("GetTemperature failed");
            }
            else
            {
                Debug.WriteLine(string.Format("GetTemperature succeeded: {0.00}", temp));
            }

            return temp;
        }

        public void Dispose()
        {
            Debug.WriteLine("Disposed CpuTemperatureProvider");
        }
    }
}
