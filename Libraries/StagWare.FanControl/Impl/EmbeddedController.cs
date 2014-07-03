using StagWare.FanControl.Impl.Linux;
using StagWare.FanControl.Impl.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StagWare.FanControl
{
    public abstract class EmbeddedController : IEmbeddedController
    {
        public static EmbeddedController Create()
        {
            EmbeddedController instance = null;

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                instance = new WindowsEmbeddedController();
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                instance = new LinuxEmbeddedController();
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            return instance;
        }

        public abstract void WriteByte(byte register, byte value);

        public abstract void WriteWord(byte register, ushort value);

        public abstract byte ReadByte(byte register);

        public abstract ushort ReadWord(byte register);

        public abstract bool AquireLock(int timeout);

        public abstract void ReleaseLock();
    }
}
