using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StagWare.FanControl.Plugins
{
    public interface ITemperatureMonitor : IDisposable
    {
        bool IsInitialized { get; }
        string TemperatureSourceDisplayName { get; }
        void Initialize();
        double GetTemperature();
    }
}
