using OpenHardwareMonitor.Hardware;
using StagWare.FanControl.Plugins;
using System;
using System.ComponentModel.Composition;

namespace StagWare.Windows.EmbeddedController
{
    [Export(typeof(IEmbeddedController))]
    [FanControlPluginMetadata("StagWare.Windows.EmbeddedController", PlatformID.Win32NT, MinOSVersion = "5.0")]
    public class EmbeddedController : IEmbeddedController
    {
        #region Constants

        private const int MaxRetries = 10;

        #endregion

        #region Private Fields

        StagWare.Hardware.EmbeddedController ec;
        Computer computer;

        #endregion

        #region IEmbeddedController implementation

        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            if (!this.IsInitialized)
            {
                this.computer = new Computer();
                this.ec = new StagWare.Hardware.EmbeddedController(new Port(computer));
                this.computer.Open();
                this.IsInitialized = true;
            }
        }

        public void WriteByte(byte register, byte value)
        {
            int writes = 0;
            int successful = 0;

            while ((successful < 3) && (writes < MaxRetries))
            {
                writes++;

                if (this.ec.TryWriteByte(register, value))
                {
                    successful++;
                }
            }
        }

        public void WriteWord(byte register, ushort value)
        {
            int writes = 0;
            int successful = 0;

            while ((successful < 3) && (writes < MaxRetries))
            {
                writes++;

                if (this.ec.TryWriteWord(register, value))
                {
                    successful++;
                }
            }
        }

        public byte ReadByte(byte register)
        {
            byte result = 0;
            int reads = 0;
            bool success = false;

            while (!success && (reads < MaxRetries))
            {
                reads++;
                success = this.ec.TryReadByte(register, out result);
            }

            return result;
        }

        public ushort ReadWord(byte register)
        {
            int result = 0;
            int reads = 0;
            bool success = false;

            while (!success && (reads < MaxRetries))
            {
                reads++;
                success = this.ec.TryReadWord(register, out result);
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
    }
}
