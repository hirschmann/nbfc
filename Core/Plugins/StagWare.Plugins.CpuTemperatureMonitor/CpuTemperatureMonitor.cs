using StagWare.FanControl.Plugins;
using StagWare.Hardware;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace StagWare.Plugins.Generic
{
    [Export(typeof(ITemperatureMonitor))]
    [FanControlPluginMetadata(
        "StagWare.Plugins.CpuTemperatureMonitor",
        SupportedPlatforms.Windows | SupportedPlatforms.Unix,
        SupportedCpuArchitectures.x86 | SupportedCpuArchitectures.x64)]
    public class CpuTemperatureMonitor : ITemperatureMonitor
    {
        #region Constants

        const string DisplayName = "CPU";

        #endregion

        #region Private Fields

        private HardwareMonitor hwMon;

        #endregion

        #region ITemperatureMonitor implementation

        public bool IsInitialized
        {
            get;
            private set;
        }

        public string TemperatureSourceDisplayName
        {
            get { return DisplayName; }
        }

        public void Initialize()
        {
            if (!this.IsInitialized)
            {
                this.IsInitialized = true;
                this.hwMon = HardwareMonitor.Instance;
            }
        }

        public double GetTemperature()
        {
            KeyValuePair<string, double>[] temps = this.hwMon.CpuTemperatures;

            double temperature = 0;

            foreach (KeyValuePair<string, double> pair in temps)
            {
                temperature += pair.Value;
            }

            return temperature / temps.Length;
        }

        public void Dispose()
        {
        }

        #endregion
    }
}
