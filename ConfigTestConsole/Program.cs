using StagWare.FanControl;
using StagWare.FanControl.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConfigTestConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string path = @"D:\Users\Stefan\Downloads\";

            var cfgMan = new FanControlConfigManager(path);
            var cfg = cfgMan.Configs.First();

            FanSpeedManager fsm = new FanSpeedManager(cfg.FanConfigurations[0], cfg.CriticalTemperature);

            while (true)
            {
                int temp = 0;

                if (int.TryParse(Console.ReadLine(), out temp))
                {
                    fsm.UpdateFanSpeed(temp, true);
                    Console.WriteLine();
                    Console.WriteLine(fsm.FanSpeedPercentage + "%");
                    Console.WriteLine(fsm.FanSpeedValue);
                    Console.WriteLine();
                    
                }
            }
        }
    }
}