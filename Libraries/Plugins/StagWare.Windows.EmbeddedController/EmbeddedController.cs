using OpenHardwareMonitor.Hardware;
using StagWare.FanControl.Plugins;
using StagWare.Hardware.LPC;
using System;
using System.ComponentModel.Composition;

namespace StagWare.Windows.EmbeddedController
{
    [Export(typeof(IEmbeddedController))]
    [FanControlPluginMetadata("StagWare.Windows.EmbeddedController", PlatformID.Win32NT, MinOSVersion = "5.0")]
    public class EmbeddedController : EmbeddedControllerBase, IEmbeddedController
    {
        #region Constants

        private const int MaxRetries = 10;

        #endregion

        #region Private Fields

        Computer computer;

        #endregion

        #region IEmbeddedController implementation

        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            if (!this.IsInitialized)
            {
                this.computer = new Computer();
                this.computer.Open();
                this.IsInitialized = true;
            }
        }

        public void WriteByte(byte register, byte value)
        {
            int writes = 0;

            while (writes < MaxRetries)
            {
                if (TryWriteByte(register, value))
                {
                    return;
                }

                writes++;
            }
        }

        public void WriteWord(byte register, ushort value)
        {
            int writes = 0;

            while (writes < MaxRetries)
            {
                if (TryWriteWord(register, value))
                {
                    return;
                }

                writes++;
            }
        }

        public byte ReadByte(byte register)
        {
            byte result = 0;
            int reads = 0;

            while (reads < MaxRetries)
            {
                if (TryReadByte(register, out result))
                {
                    return result;
                }

                reads++;
            }

            return result;
        }

        public ushort ReadWord(byte register)
        {
            int result = 0;
            int reads = 0;

            while (reads < MaxRetries)
            {
                if (TryReadWord(register, out result))
                {
                    return (ushort)result;
                }

                reads++;
            }

            return (ushort)result;
        }

        public bool AquireLock(int timeout)
        {
            return this.computer.WaitIsaBusMutex(timeout);
        }

        public void ReleaseLock()
        {
            this.computer.ReleaseIsaBusMutex();
        }

        public void Dispose()
        {
            if (this.computer != null)
            {
                this.computer.Close();
                this.computer = null;
            }

            GC.SuppressFinalize(this);
        }

        #endregion

        #region EmbeddedControllerBase implementation

        protected override void WritePort(int port, byte value)
        {
            this.computer.WriteIoPort(port, value);
        }

        protected override byte ReadPort(int port)
        {
            return this.computer.ReadIoPort(port);
        }

        #endregion
    }
}
