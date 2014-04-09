using StagWare.FanControl.Impl.Linux;
using StagWare.FanControl.Impl.Windows;
using System;

namespace StagWare.FanControl
{
    public abstract class CpuTemperatureProvider : ITemperatureProvider
    {
        public static CpuTemperatureProvider Create()
        {
            CpuTemperatureProvider instance = null;

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                instance = new WindowsCpuTemperatureProvider();
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                instance = new LinuxCpuTemperatureProvider();
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            return instance;
        }

        public abstract double GetTemperature();
    }
}
