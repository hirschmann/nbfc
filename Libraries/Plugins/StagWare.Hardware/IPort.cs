
namespace StagWare.Hardware
{
    public interface IPort
    {
        void Write(int port, byte value);
        byte Read(int port);
    }
}
