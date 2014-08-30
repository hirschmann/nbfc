using StagWare.FanControl.Plugins;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace StagWare.Windows.CpuTempProvider
{
    [Export(typeof(ITemperatureProvider))]
    [FanControlPluginMetadata(
        "StagWare.Windows.CpuTempProvider", 
        SupportedPlatforms.Windows | SupportedPlatforms.Unix,
        SupportedCpuArchitectures.x86 | SupportedCpuArchitectures.x64)]
    public class CpuTemperatureProvider : ITemperatureProvider
    {
        #region Private Fields

        private HardwareMonitor hwMon;

        #endregion

        #region ITemperatureProvider implementation

        public bool IsInitialized
        {
            get;
            private set;
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
