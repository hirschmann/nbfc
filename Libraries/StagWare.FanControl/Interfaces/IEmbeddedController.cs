using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StagWare.FanControl
{
    public interface IEmbeddedController
    {
        void WriteByte(byte register, byte value);
        void WriteWord(byte register, ushort value);
        byte ReadByte(byte register);
        ushort ReadWord(byte register);
        bool AquireLock(int timeout);
        void ReleaseLock();
    }
}
