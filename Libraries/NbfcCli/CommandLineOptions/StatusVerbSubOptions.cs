using CommandLine;

namespace NbfcCli
{
    public class StatusVerbSubOptions
    {
        [Option('a', "all", DefaultValue = true, HelpText = "Get all available info.")]
        public bool GetAll { get; set; }

        [Option('i', "index", HelpText = "Fan index.")]
        public int FanIndex { get; set; }
    }
}
