using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StagWare.FanControl.Plugins
{
    [Flags]
    public enum SupportedPlatforms
    {
        None = 0,
        Windows = 1,
        Unix = 2,
        MacOSX = 4
    }

    [Flags]
    public enum SupportedCpuArchitectures
    {
        None = 0,
        x86 = 1,
        x64 = 2
    }

    public interface IFanControlPluginMetadata
    {
        string UniqueId { get; }
        string MinOSVersion { get; }
        string MaxOSVersion { get; }
        SupportedPlatforms SupportedPlatforms { get; }
        SupportedCpuArchitectures SupportedCpuArchitectures { get; }
    }
}
