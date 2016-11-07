using NbfcClient.NbfcService;
using System;

namespace NbfcClient.Services
{
    public interface IFanControlClient : IFanControlService
    {
        event EventHandler<FanControlStatusChangedEventArgs> FanControlStatusChanged;
        FanControlInfo FanControlInfo { get; }
    }
}
