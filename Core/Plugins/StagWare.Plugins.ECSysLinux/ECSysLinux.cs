using Mono.Unix;
using Mono.Unix.Native;
using StagWare.FanControl.Plugins;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace StagWare.Plugins.ECSysLinux
{
    [Export(typeof(IEmbeddedController))]
    [FanControlPluginMetadata(
        "StagWare.Plugins.ECSysLinux",
        SupportedPlatforms.Unix,
        SupportedCpuArchitectures.x86 | SupportedCpuArchitectures.x64)]
    public class ECSysLinux : IEmbeddedController
    {
        #region Constants

        private const string EC0IOPath = "/sys/kernel/debug/ec/ec0/io";

        #endregion

        #region Private Fields

        static readonly object syncRoot = new object();
        private UnixStream stream;

        #endregion

        #region IEmbeddedController implementation

        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            try
            {
                Process modprobe = new Process();
                modprobe.StartInfo.FileName = "modprobe";
                modprobe.StartInfo.Arguments = "ec_sys write_support=1";
                modprobe.Start();
                modprobe.WaitForExit();

                IsInitialized = modprobe.ExitCode == 0 && File.Exists(EC0IOPath);
            }
            catch
            {
                IsInitialized = false;
            }
        }

        public void WriteByte(byte register, byte value)
        {
            byte[] buffer = new byte[] { value };
            this.stream.WriteAtOffset(buffer, 0, buffer.Length, register);
        }

        public void WriteWord(byte register, ushort value)
        {
            // little endian
            byte msb = (byte)(value >> 8);
            byte lsb = (byte)value;

            byte[] buffer = new byte[] { lsb, msb };
            this.stream.WriteAtOffset(buffer, 0, buffer.Length, register);
        }

        public byte ReadByte(byte register)
        {
            byte[] buffer = new byte[1];
            this.stream.ReadAtOffset(buffer, 0, buffer.Length, register);

            return buffer[0];
        }

        public ushort ReadWord(byte register)
        {
            // little endian
            byte[] buffer = new byte[2];
            this.stream.ReadAtOffset(buffer, 0, buffer.Length, register);

            return (ushort)((buffer[1] << 8) | buffer[0]);
        }

        public bool AcquireLock(int timeout)
        {
            bool success = false;

            if (Monitor.TryEnter(syncRoot, timeout))
            {
                if (this.stream == null)
                {
                    int fd = Syscall.open(EC0IOPath, OpenFlags.O_RDWR | OpenFlags.O_EXCL);

                    if (fd == -1)
                    {
                        var e = new Win32Exception(Marshal.GetLastWin32Error());
                        Debug.WriteLine(string.Format("Error opening {0}: {1}", EC0IOPath, e.Message));

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
    }
}
