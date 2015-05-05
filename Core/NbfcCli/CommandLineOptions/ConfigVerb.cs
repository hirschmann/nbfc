using clipr;

namespace NbfcCli.CommandLineOptions
{
    public class ConfigVerb
    {
        [MutuallyExclusiveGroup("option")]
        [NamedArgument('a', "apply", Action = ParseAction.Store, Description = "Apply a config.")]
        public string Apply { get; set; }
    }
}
