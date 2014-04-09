using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StagWare.FanControl.Impl.Linux
{
    public class LinuxEmbeddedController : EmbeddedController
    {
        public override void WriteByte(byte register, byte value)
        {
            throw new NotImplementedException();
        }

        public override void WriteWord(byte register, ushort value)
        {
            throw new NotImplementedException();
        }

        public override byte ReadByte(byte register)
        {
            throw new NotImplementedException();
        }

        public override ushort ReadWord(byte register)
        {
            throw new NotImplementedException();
        }

        public override bool AquireLock(int timeout)
        {
            throw new NotImplementedException();
        }

        public override void ReleaseLock()
        {
            throw new NotImplementedException();
        }
    }
}
