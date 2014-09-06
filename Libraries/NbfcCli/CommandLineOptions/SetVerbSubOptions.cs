using CommandLine;

namespace NbfcCli
{
    public class SetVerbSubOptions
    {
        [Option(
            's',
            "speed",
            Required = true,
            HelpText = "Fan speed in percent.")]
        public float Speed { get; set; }

        [Option('i', "index", HelpText = "Fan index.")]
        public int FanIndex { get; set; }
    }
}
