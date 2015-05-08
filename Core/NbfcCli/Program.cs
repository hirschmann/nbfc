using clipr;
using clipr.Core;
using NbfcCli.CommandLineOptions;
using NbfcCli.NbfcService;
using System;
using System.Collections.Generic;
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
            var opt = new Verbs();
            var parser = new CliParser<Verbs>(opt, ParserOptions.CaseInsensitive, new VerbsHelpGenerator());
            parser.StrictParse(args);

            if (opt.Start != null)
            {
                StartService();
            }
            else if (opt.Stop != null)
            {
                StopService();
            }
            else if (opt.Set != null)
            {
                SetFanSpeed(opt.Set);
            }
            else if (opt.Config != null)
            {
                ConfigureService(opt.Config);
            }
            else if (opt.Status != null)
            {
                PrintStatus(opt.Status);
            }
        }

        private static void PrintStatus(StatusVerb verb)
        {
            FanControlInfo info = GetFanControlInfo();

            if (verb.Service)
            {
                PrintServiceStatus(info);
            }
            else if (verb.Fan != null)
            {
                if (verb.Fan.Count > 0)
                {
                    foreach (int idx in verb.Fan)
                    {
                        if (idx < info.FanStatus.Length)
                        {
                            PrintFanStatus(info.FanStatus[idx]);
                        }
                        else
                        {
                            Console.Error.WriteLine(string.Format("A fan with index {0} does not exist.", idx));
                        }
                    }
                }
                else
                {
                    foreach (FanStatus status in info.FanStatus)
                    {
                        PrintFanStatus(status);
                    }
                }
            }
            else
            {
                PrintServiceStatus(info);

                foreach (FanStatus status in info.FanStatus)
                {
                    PrintFanStatus(status);
                }
            }
        }

        private static void ConfigureService(ConfigVerb verb)
        {
            if (!string.IsNullOrEmpty(verb.Apply))
            {
                ApplyConfig(verb.Apply);
            }
            else
            {
                Console.Error.WriteLine("Invalid config name.");
            }
        }

        private static void SetFanSpeed(SetVerb verb)
        {
            float speed = -1;
            List<int> indices = verb.Fan;

            if (indices == null)
            {
                indices = new List<int>();
                indices.Add(0);
            }

            if (verb.Auto || float.TryParse(verb.Speed, out speed))
            {
                foreach (int idx in indices)
                {
                    SetFanSpeed(speed, idx);
                }
            }
            else
            {
                Console.Error.WriteLine("Invalid speed value.");
            }
        }

        private static void SetFanSpeed(float speed, int index)
        {
            Action<FanControlServiceClient> action = client => client.SetTargetFanSpeed(speed, index);
            CallServiceMethod(action);
        }

        private static void ApplyConfig(string configName)
        {
            Action<FanControlServiceClient> action = client => client.SetConfig(configName);
            CallServiceMethod(action);
        }

        private static FanControlInfo GetFanControlInfo()
        {
            FanControlInfo info = null;

            Action<FanControlServiceClient> action = client =>
            {
                info = client.GetFanControlInfo();
            };

            CallServiceMethod(action);

            return info;
        }

        private static void PrintServiceStatus(FanControlInfo info)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Service enabled\t\t: {0}", info.Enabled);
            sb.AppendLine();
            sb.AppendFormat("Selected config name\t: {0}", info.SelectedConfig);
            sb.AppendLine();
            sb.AppendFormat("Temperature\t\t: {0}", info.Temperature);
            sb.AppendLine();

            Console.WriteLine(sb.ToString());
        }

        private static void PrintFanStatus(FanStatus status)
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
            sb.AppendLine();

            Console.WriteLine(sb.ToString());
        }

        private static void StartService()
        {
            Action<FanControlServiceClient> action = client => client.Start();
            CallServiceMethod(action);
        }

        private static void StopService()
        {
            Action<FanControlServiceClient> action = client => client.Stop();
            CallServiceMethod(action);
        }

        private static void CallServiceMethod(Action<FanControlServiceClient> action)
        {
            try
            {
                using (var client = new FanControlServiceClient())
                {
                    client.Open();
                    action(client);
                    client.Close();
                }
            }
            catch (Exception e)
            {
                string msg = "Failed to execute the command";

                if (!string.IsNullOrWhiteSpace(e.Message))
                {
                    msg += ": " + e.Message;
                }

                Console.Error.WriteLine(msg);
            }
        }

        #endregion
    }
}
