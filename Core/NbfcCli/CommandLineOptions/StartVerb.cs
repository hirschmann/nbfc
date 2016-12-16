using clipr;

namespace NbfcCli.CommandLineOptions
{
    public class StartVerb
    {
        [MutuallyExclusiveGroup("option")]
        [NamedArgument(
            'e',
            "enabled",
            Action = ParseAction.StoreTrue,
            Description = "Start in enabled mode (default)")]
        public bool Enabled { get; set; }

        [MutuallyExclusiveGroup("option")]
        [NamedArgument(
            'r',
            "readonly",
            Action = ParseAction.StoreTrue,
            Description = "Start in read-only mode")]
        public bool ReadOnly { get; set; }
    }
}
