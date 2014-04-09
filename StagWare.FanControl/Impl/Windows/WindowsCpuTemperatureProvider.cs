using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StagWare.FanControl.Impl.Windows
{
    public class WindowsCpuTemperatureProvider : CpuTemperatureProvider
    {
        private readonly IHardware cpu;
        private readonly ISensor[] cpuTempSensors;

        public WindowsCpuTemperatureProvider()
        {
            var computer = new Computer();
            computer.CPUEnabled = true;
            computer.Open();
            this.cpu = computer.Hardware.FirstOrDefault(x => x.HardwareType == HardwareType.CPU);

            if (this.cpu != null)
            {
                this.cpuTempSensors = InitializeTempSensors(cpu);
            }

            if (this.cpuTempSensors == null || this.cpuTempSensors.Length <= 0)
            {
                throw new PlatformNotSupportedException("No CPU temperature sensor(s) found.");
            }
        }

        public override double GetTemperature()
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
    }
}
