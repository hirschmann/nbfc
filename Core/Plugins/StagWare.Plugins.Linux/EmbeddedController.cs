using Mono.Unix.Native;
using StagWare.FanControl.Plugins;
using StagWare.Hardware.LPC;
using System;
using System.ComponentModel.Composition;
using System.Threading;

namespace StagWare.Plugins.Linux
{
    [Export(typeof(IEmbeddedController))]
    [FanControlPluginMetadata(
        "StagWare.Linux.EmbeddedController",
        SupportedPlatforms.Unix,
        SupportedCpuArchitectures.x86 | SupportedCpuArchitectures.x64,
        MinOSVersion = "3.10")]
    public class EmbeddedController : EmbeddedControllerBase, IEmbeddedController
    {
        #region Constants

        const string PortFilePath = "/dev/port";

        #endregion

        #region Private Fields

        static readonly object syncRoot = new object();
        private int fileDescriptor;

        #endregion

        #region IEmbeddedController implementation

        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            if (!this.IsInitialized)
            {
                this.fileDescriptor = -1;
                this.IsInitialized = true;
            }
        }

        public bool AquireLock(int timeout)
        {
            bool success = false;

            if (Monitor.TryEnter(syncRoot, timeout))
            {
                if (this.fileDescriptor == -1)
                {
                    try
                    {
                        this.fileDescriptor = Syscall.open(PortFilePath, OpenFlags.O_RDWR | OpenFlags.O_EXCL);
                    }
                    catch { }

                    success = this.fileDescriptor != -1;

                    if (!success)
                    {
                        Monitor.Exit(syncRoot);
                    }
                }
            }

            return success;
        }

        public void ReleaseLock()
        {
            if (Monitor.IsEntered(syncRoot))
            {
                if (this.fileDescriptor != -1)
                {
                    Syscall.close(this.fileDescriptor);
                    this.fileDescriptor = -1;
                }

                Monitor.Exit(syncRoot);
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
            Syscall.lseek(this.fileDescriptor, port, SeekFlags.SEEK_SET);
            byte[] buffer = new byte[] { value };

            unsafe
            {
                fixed (byte* p = buffer)
                {
                    Syscall.write(this.fileDescriptor, p, (ulong)buffer.Length);
                }
            }
        }

        protected override byte ReadPort(int port)
        {
            Syscall.lseek(this.fileDescriptor, port, SeekFlags.SEEK_SET);
            byte[] buffer = new byte[1];

            unsafe
            {
                fixed (byte* p = buffer)
                {
                    Syscall.read(this.fileDescriptor, p, (ulong)buffer.Length);
                }
            }

            return buffer[0];
        }

        #endregion
    }
}
