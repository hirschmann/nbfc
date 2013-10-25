using ServiceTestClient.FanControlService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NbfcServiceTestClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
        REINITIALIZE:

            Console.Write("Connecting to service...");

            var client = new FanControlServiceClient();

            try
            {
                client.Open();
            }
            catch
            {
                Console.Write("failed");
                Console.ReadKey();
                return;
            }

            Console.Write("successful\n");

        RESTART:

            string input = string.Empty;

            do
            {
                try
                {
                    int i = 0;
                    var info = client.GetFanControlInfo();

                    Console.Clear();
                    Console.WriteLine("Selected config: " + info.SelectedConfig);
                    Console.WriteLine("Initialized: " + info.IsInitialized);
                    Console.WriteLine("CPU temperature: " + info.CpuTemperature);

                    if (info.FanStatus != null)
                    {
                        foreach (FanStatus s in info.FanStatus)
                        {
                            Console.WriteLine();
                            Console.WriteLine("----------------------------------");

                            if (string.IsNullOrWhiteSpace(s.FanDisplayName))
                            {
                                Console.WriteLine("Fan " + i);
                            }
                            else
                            {
                                Console.WriteLine(s.FanDisplayName);
                            }

                            Console.WriteLine("----------------------------------");
                            Console.WriteLine("Auto control enabled: " + s.AutoControlEnabled);
                            Console.WriteLine("Critical mode enabled: " + s.CriticalModeEnabled);
                            Console.WriteLine("Target fan speed: " + s.TargetFanSpeed);
                            Console.WriteLine("Current fan speed: " + s.CurrentFanSpeed);
                            Console.WriteLine("----------------------------------");

                            i++;
                        }
                    }

                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine("command: ");

                    input = Console.ReadLine();

                    if (input.StartsWith("set:", StringComparison.OrdinalIgnoreCase))
                    {
                        int speed = 0;
                        int fanIndex = 0;
                        string[] splitted = input.Substring(4).Split(new char[] { ':', ',', '|' }, StringSplitOptions.RemoveEmptyEntries);

                        if (int.TryParse(splitted[0], out speed))
                        {
                            if (splitted.Length > 1)
                            {
                                int.TryParse(splitted[1], out fanIndex);
                            }

                            client.SetTargetFanSpeed(speed, fanIndex);
                        }
                    }
                    else if (input.Equals("restart", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.Clear();
                        client.Restart();

                        goto RESTART;
                    }
                    else if (input.Equals("start", StringComparison.OrdinalIgnoreCase))
                    {
                        client.Start();
                    }
                    else if (input.Equals("stop", StringComparison.OrdinalIgnoreCase))
                    {
                        client.Stop();
                    }
                    else if (input.StartsWith("config:", StringComparison.OrdinalIgnoreCase))
                    {
                        client.SetConfig(input.Substring(7));
                    }
                }
                catch
                {
                    client.Abort();
                    client = null;

                    goto REINITIALIZE;
                }
            } while (!input.Equals("exit", StringComparison.OrdinalIgnoreCase));
        }
    }
}
