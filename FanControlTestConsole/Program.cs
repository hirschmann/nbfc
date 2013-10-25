using StagWare.FanControl;
using StagWare.FanControl.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FanControlTestApp
{
    public class Program
    {
        private static FanControl fc;

        public static void Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;

            FanControlConfigManager cm = new FanControlConfigManager("Configs");           

            if (!cm.AutoSelectConfig())
            {
                return;
            }

            fc = new FanControl(cm.SelectedConfig);
            fc.Start();
            fc.SetTargetFanSpeed(101, 0);

            while (true)
            {
                Console.WriteLine("CPU temperature: " + fc.CpuTemperature);
                Console.WriteLine();

                foreach (FanInformation info in fc.FanInformation)
                {
                    Console.WriteLine(info.FanDisplayName);
                    Console.WriteLine("-----------------------------------");
                    Console.WriteLine("Auto control enabled: " + info.AutoFanControlEnabled);
                    Console.WriteLine("Target speed: " + info.TargetFanSpeed);
                    Console.WriteLine("Current speed: " + info.CurrentFanSpeed);
                    Console.WriteLine("-----------------------------------");
                    Console.WriteLine();
                }

                Thread.Sleep(1000);
                Console.Clear();
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (fc != null)
            {
                fc.Dispose();
                fc = null;
            }
        }

        private static FanControlConfigV2 CreateNewConfig()
        {
            var cfg = new FanControlConfigV2()
            {
                CriticalTemperature = 75,
                EcPollInterval = 3000,
                NotebookModel = "HP ProBook 6465b",
                ReadWriteWords = false
            };

            cfg.FanConfigurations.Add(new FanConfiguration()
            {
                FanSpeedResetValue = 255,
                MaxSpeedValue = 48,
                MinSpeedValue = 88,
                ReadRegister = 46,
                WriteRegister = 47,
                ResetRequired = true
            });

            cfg.FanConfigurations[0].FanSpeedPercentageOverrides.Add(new FanSpeedPercentageOverride()
            {
                FanSpeedPercentage = 0,
                FanSpeedValue = 255
            });

            cfg.FanConfigurations[0].TemperatureThresholds.Add(new TemperatureThreshold(1, 1, 0));
            cfg.RegisterWriteConfigurations.Add(new RegisterWriteConfiguration()
            {
                Description = "cpu",
                Register = 123,
                ResetRequired = true,
                ResetValue = 123,
                Value = 123,
                WriteOccasion = RegisterWriteOccasion.OnInitialization,
                WriteMode = StagWare.FanControl.Configurations.RegisterWriteMode.Set
            });

            return cfg;
        }
    }
}
