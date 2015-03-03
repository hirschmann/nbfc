using NbfcCli.NbfcService;
using System;
using System.Globalization;
using System.Text;

namespace NbfcCli
{
    public class Program
    {
        #region Constants

        const string VerbStart = "start";
        const string VerbStop = "stop";
        const string VerbStatus = "status";
        const string VerbLoad = "load";
        const string VerbSet = "set";

        static readonly string[] LoadConfigOptions = { "-c", "--config" };
        static readonly string[] SetSpeedOptions = { "-s", "--speed" };
        static readonly string[] SetIndexOptions = { "-i", "--index" };

        #endregion

        #region Main

        static void Main(string[] args)
        {
            try
            {
                ParseArgs(args);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }

        #endregion

        #region Private Methods

        private static void ParseArgs(string[] args)
        {
            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case VerbStart:
                        if (args.Length == 1)
                        {
                            StartService();
                        }
                        else
                        {
                            Console.Error.WriteLine("Invalid number of arguments");
                        }
                        break;

                    case VerbStop:
                        if (args.Length == 1)
                        {
                            StopService();
                        }
                        else
                        {
                            Console.Error.WriteLine("Invalid number of arguments");
                        }
                        break;

                    case VerbStatus:
                        if (args.Length == 1)
                        {
                            PrintStatus();
                        }
                        else
                        {
                            Console.Error.WriteLine("Invalid number of arguments");
                        }
                        break;

                    case VerbLoad:
                        if (args.Length == 2)
                        {
                            LoadConfig(args[1]);
                        }
                        else
                        {
                            Console.Error.WriteLine("Invalid number of arguments");
                        }

                        break;

                    case VerbSet:
                        if (args.Length > 1 && args.Length < 4)
                        {
                            float speed;
                            int index = 0;

                            if (float.TryParse(args[1], out speed))
                            {
                                if ((args.Length == 3) && !int.TryParse(args[2], out index))
                                {
                                    Console.Error.WriteLine("Invalid index.");
                                }

                                SetFanSpeed(speed, index);
                            }
                            else
                            {
                                Console.Error.WriteLine("Invalid speed value.");
                            }
                        }
                        else
                        {
                            Console.Error.WriteLine("Invalid number of arguments");
                        }

                        break;

                    default:
                        PrintHelp();
                        break;
                }
            }
            else
            {
                PrintHelp();
            }
        }

        private static void SetFanSpeed(float speed, int index)
        {
            using (var client = new FanControlServiceClient())
            {
                client.Open();
                client.SetTargetFanSpeed(speed, index);
                client.Close();
            }
        }

        private static void LoadConfig(string configName)
        {
            using (var client = new FanControlServiceClient())
            {
                client.Open();
                client.SetConfig(configName);
                client.Close();
            }
        }

        private static void PrintStatus()
        {
            using (var client = new FanControlServiceClient())
            {
                client.Open();

                var info = client.GetFanControlInfo();

                var sb = new StringBuilder();
                sb.AppendFormat("Service enabled\t\t: {0}", info.Enabled);
                sb.AppendLine();
                sb.AppendFormat("Selected config name\t: {0}", info.SelectedConfig);
                sb.AppendLine();
                sb.AppendFormat("Temperature\t\t: {0}", info.Temperature);
                sb.AppendLine();

                foreach (FanStatus status in info.FanStatus)
                {
                    sb.AppendLine();
                    sb.Append(FanStatusToString(status));
                }

                Console.WriteLine(sb.ToString());
                client.Close();
            }
        }

        private static void StartService()
        {
            using (var client = new FanControlServiceClient())
            {
                client.Open();
                client.Start();
                client.Close();
            }
        }

        private static void StopService()
        {
            using (var client = new FanControlServiceClient())
            {
                client.Open();
                client.Stop();
                client.Close();
            }
        }

        private static void PrintHelp()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Usage: nbfc <command>");
            sb.AppendLine();
            sb.AppendLine("Commands:");
            sb.AppendFormat("  {0,-10}Start the fan control service", VerbStart);
            sb.AppendLine();
            sb.AppendFormat("  {0,-10}Stop the fan control service", VerbStop);
            sb.AppendLine();
            sb.AppendFormat("  {0,-10}Show the fan control status", VerbStatus);
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendFormat("  {0} <speed> [<fan-index>]", VerbSet);
            sb.AppendLine();
            sb.AppendFormat("  {0,-10}Set the speed for a single fan", "");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendFormat("  {0} <config-name>", VerbLoad);
            sb.AppendLine();
            sb.AppendFormat("  {0,-10}Load and apply a config", "");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("Examples:");
            sb.AppendLine("  nfbc load \"HP ProBook 6465b\"");
            sb.AppendLine("  nbfc set 12.5   # set fan 0 to 12.5%");
            sb.AppendLine("  nbfc set -1 1   # set fan 1 to 'auto'");

            Console.WriteLine(sb.ToString());
        }

        private static string FanStatusToString(FanStatus status)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Fan display name\t: {0}", status.FanDisplayName);
            sb.AppendLine();
            sb.AppendFormat("Auto control enabled\t: {0}", status.AutoControlEnabled);
            sb.AppendLine();
            sb.AppendFormat("Critical mode enabled\t: {0}", status.CriticalModeEnabled);
            sb.AppendLine();
            sb.AppendFormat(CultureInfo.InvariantCulture, "Current fan speed\t: {0:0.00}", status.CurrentFanSpeed);
            sb.AppendLine();
            sb.AppendFormat(CultureInfo.InvariantCulture, "Target fan speed\t: {0:0.00}", status.TargetFanSpeed);
            sb.AppendLine();
            sb.AppendFormat("Fan speed steps\t\t: {0}", status.FanSpeedSteps);

            return sb.ToString();
        }

        #endregion
    }
}
