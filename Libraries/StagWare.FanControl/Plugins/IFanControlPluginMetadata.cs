using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StagWare.FanControl.Plugins
{
    public interface IFanControlPluginMetadata
    {
        string UniqueId { get; }
        string MinOSVersion { get; }
        string MaxOSVersion { get; }
        PlatformID PlatformId { get; }
        string PlatformString { get; }
    }
}
