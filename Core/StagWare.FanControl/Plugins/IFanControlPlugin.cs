using System;

namespace StagWare.FanControl.Plugins
{
    public interface IFanControlPlugin : IDisposable
    {
        bool IsInitialized { get; }
        void Initialize();
    }
}
