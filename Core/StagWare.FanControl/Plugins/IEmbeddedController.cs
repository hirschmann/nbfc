using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StagWare.FanControl.Plugins
{
    public interface IEmbeddedController : IDisposable
    {
        bool IsInitialized { get; }
        void Initialize();
        void WriteByte(byte register, byte value);
        void WriteWord(byte register, ushort value);
        byte ReadByte(byte register);
        ushort ReadWord(byte register);
        bool AcquireLock(int timeout);
        void ReleaseLock();
    }
}
