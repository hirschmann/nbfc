using StagWare.FanControl.Plugins;
using StagWare.Hardware.LPC;
using StagWare.Hardware;
using System.ComponentModel.Composition;

namespace StagWare.Plugins
{
    [Export(typeof(IEmbeddedController))]
    [FanControlPluginMetadata(
        "StagWare.Plugins.ECWindows", 
        SupportedPlatforms.Windows,
        SupportedCpuArchitectures.x86 | SupportedCpuArchitectures.x64,
        MinOSVersion = "5.0")]
    public class ECWindows : EmbeddedControllerBase, IEmbeddedController
    {
        #region Private Fields

        HardwareMonitor hwMon;

        #endregion

        #region IEmbeddedController implementation

        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            if (!this.IsInitialized)
            {
                this.hwMon = HardwareMonitor.Instance;
                this.IsInitialized = true;
            }
        }

        public bool AquireLock(int timeout)
        {
            return this.hwMon.WaitIsaBusMutex(timeout);
        }

        public void ReleaseLock()
        {
            this.hwMon.ReleaseIsaBusMutex();
        }

        public void Dispose()
        {
        }

        #endregion

        #region EmbeddedControllerBase implementation

        protected override void WritePort(int port, byte value)
        {
            this.hwMon.WriteIoPort(port, value);
        }

        protected override byte ReadPort(int port)
        {
            return this.hwMon.ReadIoPort(port);
        }

        #endregion
    }
}
