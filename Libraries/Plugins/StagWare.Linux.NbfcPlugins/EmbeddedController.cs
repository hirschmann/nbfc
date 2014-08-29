using StagWare.FanControl.Plugins;
using StagWare.Hardware.LPC;
using System;
using System.ComponentModel.Composition;
using System.IO;

namespace StagWare.Linux.NbfcPlugins
{
    [Export(typeof(IEmbeddedController))]
    [FanControlPluginMetadata("StagWare.Linux.EmbeddedController", PlatformID.Unix, MinOSVersion = "3.10")]
    public class EmbeddedController : EmbeddedControllerBase, IEmbeddedController
    {
        #region Constants

        const string PortFilePath = "/dev/port";
        private const int MaxRetries = 10;

        #endregion

        #region Private Fields

        private FileStream stream;

        #endregion

        #region IEmbeddedController implementation

        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            this.IsInitialized = true;
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
            bool success = false;

            try
            {
                this.stream = File.Open(PortFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch
            {
            }

            return success;
        }

        public void ReleaseLock()
        {
            if (this.stream != null)
            {
                this.stream.Dispose();
                this.stream = null;
            }
        }

        public void Dispose()
        {
            ReleaseLock();
        }

        #endregion

        #region EmbeddedControllerBase implementation

        protected override void WritePort(int port, byte value)
        {
            this.stream.Seek(port, SeekOrigin.Begin);
            this.stream.WriteByte(value);
        }

        protected override byte ReadPort(int port)
        {
            this.stream.Seek(port, SeekOrigin.Begin);
            return (byte)this.stream.ReadByte();
        }

        #endregion
    }
}
