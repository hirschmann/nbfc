using clipr;

namespace NbfcCli.CommandLineOptions
{
    public class ConfigVerb
    {
        [MutuallyExclusiveGroup("option")]
        [NamedArgumentEx(
            'l',
            "list",
            Action = ParseAction.StoreTrue,
            Description = "List all available configs (default)")]
        public bool List { get; set; }

        [MutuallyExclusiveGroup("option")]
        [NamedArgumentEx(
            's',
            "set",
            ArgumentName = "cfg-name",
            Action = ParseAction.Store,
            Description = "Set a config")]
        public string Set { get; set; }

        [MutuallyExclusiveGroup("option")]
        [NamedArgumentEx(
            'a', 
            "apply", 
            ArgumentName = "cfg-name", 
            Action = ParseAction.Store, 
            Description = "Set a config and enable fan control")]
        public string Apply { get; set; }
    }
}
