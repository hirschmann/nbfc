using OpenHardwareMonitor.Hardware;
using StagWare.FanControl.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace StagWare.Windows.CpuTempProvider
{
    [Export(typeof(ITemperatureProvider))]
    [FanControlPluginMetadata("StagWare.Windows.CpuTempProvider", PlatformID.Win32NT, MinOSVersion = "5.0")]
    public class CpuTemperatureProvider : ITemperatureProvider
    {
        #region Private Fields

        private IHardware cpu;
        private ISensor[] cpuTempSensors;

        #endregion

        #region ITemperatureProvider implementation

        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            if (!this.IsInitialized)
            {
                this.cpu = HardwareMonitor.Instance.CPU;

                if (this.cpu != null)
                {
                    this.cpuTempSensors = GetTemperatureSensors(cpu);
                }

                if (this.cpuTempSensors == null || this.cpuTempSensors.Length <= 0)
                {
                    try
                    {
                        Dispose();
                    }
                    finally
                    {
                        throw new PlatformNotSupportedException("No CPU temperature sensor(s) found.");
                    }
                }

                this.IsInitialized = true;
            }
        }

        public double GetTemperature()
        {
            double temperatureSum = 0;
            int count = 0;
            this.cpu.Update();

            foreach (ISensor sensor in this.cpuTempSensors)
            {
                if (sensor.Value.HasValue)
                {
                    temperatureSum += sensor.Value.Value;
                    count++;
                }
            }

            return temperatureSum / count;
        }

        public void Dispose()
        {
        }

        #endregion

        #region Private Methods

        private static ISensor[] GetTemperatureSensors(IHardware cpu)
        {
            if (cpu == null)
            {
                throw new PlatformNotSupportedException("Failed to access CPU temperature sensors(s).");
            }

            var sensors = new List<ISensor>();
            cpu.Update();

            foreach (ISensor s in cpu.Sensors)
            {
                if (s.SensorType == SensorType.Temperature)
                {
                    string name = s.Name.ToUpper();

                    if (name.Contains("PACKAGE") || name.Contains("TOTAL"))
                    {
                        return new ISensor[] { s };
                    }
                    else
                    {
                        sensors.Add(s);
                    }
                }
            }

            return sensors.ToArray();
        }

        #endregion
    }
}
