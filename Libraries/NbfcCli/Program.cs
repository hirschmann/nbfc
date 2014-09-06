using CommandLine;
using NbfcCli.NbfcService;
using System;
using System.Globalization;
using System.Text;

namespace NbfcCli
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new CommandLineOptions();

            var onVerb = new Action<string, object>((s, o) =>
            {
                switch (s.ToLower())
                {
                    case "start":
                        options.Start = true;
                        break;

                    case "stop":
                        options.Stop = true;
                        break;

                    case "status":
                        options.StatusVerb = (StatusVerbSubOptions)o;
                        break;

                    case "load":
                        options.LoadVerb = (LoadVerbSubOptions)o;
                        break;

                    case "set":
                        options.SetVerb = (SetVerbSubOptions)o;
                        break;
                }
            });

            var settings = new ParserSettings();
            settings.CaseSensitive = false;
            settings.IgnoreUnknownArguments = false;
            settings.MutuallyExclusive = true;
            settings.ParsingCulture = CultureInfo.InvariantCulture;
            settings.HelpWriter = Console.Error;


            using (Parser parser = new Parser(settings))
            {
                if (parser.ParseArgumentsStrict(args, options, onVerb))
                {
                    using (var client = new FanControlServiceClient())
                    {
                        client.Open();

                        if (options.Start)
                        {
                            client.Start();
                        }
                        else if (options.Stop)
                        {
                            client.Stop();
                        }
                        else if (options.LoadVerb != null)
                        {
                            client.SetConfig(options.LoadVerb.ConfigName);
                        }
                        else if (options.SetVerb != null)
                        {
                            client.SetTargetFanSpeed(
                                options.SetVerb.Speed,
                                options.SetVerb.FanIndex);
                        }
                        else if (options.StatusVerb != null)
                        {
                            var info = client.GetFanControlInfo();

                            var sb = new StringBuilder();
                            sb.AppendFormat("Service enabled\t\t: {0}", info.Enabled);
                            sb.AppendLine();
                            sb.AppendFormat("Selected config name\t: {0}", info.SelectedConfig);
                            sb.AppendLine();
                            sb.AppendFormat("Temperature\t\t: {0}", info.CpuTemperature);
                            sb.AppendLine();

                            if (options.StatusVerb.FanIndex >= 0
                                && options.StatusVerb.FanIndex < info.FanStatus.Length)
                            {
                                sb.AppendLine();
                                sb.Append(FanStatusToString(info.FanStatus[options.StatusVerb.FanIndex]));
                            }
                            else if (options.StatusVerb.GetAll)
                            {
                                foreach (FanStatus status in info.FanStatus)
                                {
                                    sb.AppendLine();
                                    sb.Append(FanStatusToString(status));
                                }
                            }
                            else
                            {
                                Console.WriteLine(options.GetHelp());
                            }

                            Console.Write(sb.ToString());
                        }

                        client.Close();
                    }
                }
                else
                {
                    Console.WriteLine(options.GetHelp());
                }
            }
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
    }
}
