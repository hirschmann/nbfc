namespace StagWare.FanControl.Plugins
{
    public interface IEmbeddedController : IFanControlPlugin
    {
        void WriteByte(byte register, byte value);
        void WriteWord(byte register, ushort value);
        byte ReadByte(byte register);
        ushort ReadWord(byte register);
        bool AcquireLock(int timeout);
        void ReleaseLock();
    }
}
