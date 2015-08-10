using Mono.Unix;
using Mono.Unix.Native;
using StagWare.FanControl.Plugins;
using StagWare.Hardware.LPC;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace StagWare.Plugins
{
    [Export(typeof(IEmbeddedController))]
    [FanControlPluginMetadata(
        "StagWare.Plugins.ECLinux",
        SupportedPlatforms.Unix,
        SupportedCpuArchitectures.x86 | SupportedCpuArchitectures.x64)]
    public class ECLinux : EmbeddedControllerBase, IEmbeddedController
    {
        #region Constants

        const string PortFilePath = "/dev/port";

        #endregion

        #region Private Fields

        static readonly object syncRoot = new object();
        private UnixStream stream;

        #endregion

        #region IEmbeddedController implementation

        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            if (!this.IsInitialized)
            {
                this.IsInitialized = true;
            }
        }

        public bool AcquireLock(int timeout)
        {
            bool success = false;

            if (Monitor.TryEnter(syncRoot, timeout))
            {
                if (this.stream == null)
                {
                    int fd = Syscall.open(PortFilePath, OpenFlags.O_RDWR | OpenFlags.O_EXCL);

                    if (fd == -1)
                    {
                        var e = new Win32Exception(Marshal.GetLastWin32Error());
                        Debug.WriteLine(string.Format("Error opening {0}: {1}", PortFilePath, e.Message));

                        Monitor.Exit(syncRoot);
                    }
                    else
                    {
                        this.stream = new UnixStream(fd);
                        success = true;
                    }
                }
            }

            return success;
        }

        public void ReleaseLock()
        {
            try
            {
                if (this.stream != null)
                {
                    this.stream.Dispose();
                    this.stream = null;
                }
            }
            finally
            {
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
            byte[] buffer = new byte[] { value };
            this.stream.WriteAtOffset(buffer, 0, buffer.Length, port);
        }

        protected override byte ReadPort(int port)
        {
            byte[] buffer = new byte[1];
            this.stream.ReadAtOffset(buffer, 0, buffer.Length, port);

            return buffer[0];
        }

        #endregion
    }
}
