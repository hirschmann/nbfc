using clipr;

namespace NbfcCli.CommandLineOptions
{
    public class ConfigVerb
    {
        [MutuallyExclusiveGroup("option")]
        [NamedArgument(
            'l',
            "list",
            Action = ParseAction.StoreTrue,
            Description = "List all available configs (default)")]
        public bool List { get; set; }

        [MutuallyExclusiveGroup("option")]
        [NamedArgument(
            's',
            "set",
            MetaVar = "cfg-name",
            Action = ParseAction.Store,
            Description = "Set a config")]
        public string Set { get; set; }

        [MutuallyExclusiveGroup("option")]
        [NamedArgument(
            'a', 
            "apply", 
            MetaVar = "cfg-name", 
            Action = ParseAction.Store, 
            Description = "Set a config and enable fan control")]
        public string Apply { get; set; }

        [MutuallyExclusiveGroup("option")]
        [NamedArgument(
            'r',
            "recommend",
            Action = ParseAction.StoreTrue,
            Description = "List configs which may work for your device")]
        public bool Recommend { get; set; }
    }
}
