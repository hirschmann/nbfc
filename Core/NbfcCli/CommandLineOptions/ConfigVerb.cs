using clipr;

namespace NbfcCli.CommandLineOptions
{
    public class ConfigVerb
    {
        [MutuallyExclusiveGroup("option")]
        [NamedArgumentEx(
            'a', 
            "apply", 
            ArgumentName = "cfg-name", 
            Action = ParseAction.Store, 
            Description = "Load and apply a config")]
        public string Apply { get; set; }

        [MutuallyExclusiveGroup("option")]
        [NamedArgumentEx(
            'l',
            "list",
            Action = ParseAction.StoreTrue,
            Description = "List all available configs")]
        public bool List { get; set; }
    }
}
