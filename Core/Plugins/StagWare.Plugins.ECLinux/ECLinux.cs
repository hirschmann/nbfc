using Mono.Unix;
using Mono.Unix.Native;
using StagWare.FanControl.Plugins;
using StagWare.Hardware.LPC;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System;

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
        bool disposed = false;
        UnixStream stream;

        #endregion

        #region IEmbeddedController implementation

        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            if (!this.IsInitialized)
            {
                try
                {
                    this.IsInitialized = AcquireLock(500);
                }
                catch
                {
                }

                if (this.IsInitialized)
                {
                    ReleaseLock();
                }
            }
        }

        public bool AcquireLock(int timeout)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(ECLinux));
            }

            bool success = false;
            bool syncRootLockTaken = false;

            try
            {
                Monitor.TryEnter(syncRoot, timeout, ref syncRootLockTaken);

                if (!syncRootLockTaken)
                {
                    return false;
                }

                if(this.stream == null)
                {
                    int fd = Syscall.open(PortFilePath, OpenFlags.O_RDWR | OpenFlags.O_EXCL);

                    if (fd == -1)
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }

                    this.stream = new UnixStream(fd);
                }

                success = this.stream != null;
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            finally
            {
                if(syncRootLockTaken && !success)
                {
                    Monitor.Exit(syncRootLockTaken);
                }
            }

            return success;
        }

        public void ReleaseLock()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(ECLinux));
            }

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
            lock (syncRoot)
            {
                if (this.stream != null)
                {
                    this.stream.Dispose();
                    this.stream = null;
                }

                disposed = true;
            }
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
