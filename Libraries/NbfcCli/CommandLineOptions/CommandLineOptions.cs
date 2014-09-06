using CommandLine;
using System.Text;

namespace NbfcCli
{
    public class CommandLineOptions
    {
        [VerbOption(
            "load",
            MutuallyExclusiveSet = "load",
            HelpText = "Load and apply a config file.")]
        public LoadVerbSubOptions LoadVerb { get; set; }

        [VerbOption(
            "set",
            MutuallyExclusiveSet = "set",
            HelpText = "Set the fan speed of a single fan.")]
        public SetVerbSubOptions SetVerb { get; set; }

        [VerbOption(
            "status",
            MutuallyExclusiveSet = "status",
            HelpText = "Show the fan control status.")]
        public StatusVerbSubOptions StatusVerb { get; set; }

        [VerbOption(
            "start",
            MutuallyExclusiveSet = "start",
            HelpText = "Start the fan control service.")]
        public bool Start { get; set; }

        [VerbOption(
            "stop",
            MutuallyExclusiveSet = "stop",
            HelpText = "Stop the fan control service.")]
        public bool Stop { get; set; }

        [HelpOption(HelpText = "Display this help screen.")]
        public string GetHelp()
        {
            var help = new StringBuilder();

            help.AppendLine("Guide Application Help Screen!");

            return help.ToString();
        }
    }
}
