using OpenHardwareMonitor.Hardware;
using StagWare.FanControl.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

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
                    this.cpuTempSensors = InitializeTempSensors(cpu);
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

        private static ISensor[] InitializeTempSensors(IHardware cpu)
        {
            if (cpu == null)
            {
                throw new PlatformNotSupportedException("Failed to access CPU temperature sensors(s).");
            }

            cpu.Update();

            IEnumerable<ISensor> sensors = cpu.Sensors
                .Where(x => x.SensorType == SensorType.Temperature);

            ISensor packageSensor = sensors.FirstOrDefault(x =>
            {
                string upper = x.Name.ToUpperInvariant();
                return upper.Contains("PACKAGE") || upper.Contains("TOTAL");
            });

            return packageSensor != null
                ? new ISensor[] { packageSensor }
                : sensors.ToArray();
        }

        #endregion
    }
}
