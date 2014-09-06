using CommandLine;

namespace NbfcCli
{
    public class LoadVerbSubOptions
    {
        [Option(
            'c',
            "config",
            Required = true,
            HelpText = "The name of the config file (without extension).")]
        public string ConfigName { get; set; }
    }
}
