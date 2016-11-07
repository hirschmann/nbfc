using clipr;

namespace NbfcCli.CommandLineOptions
{
    public class StartVerb
    {
        [NamedArgumentEx(
            'r',
            "readonly",
            Action = ParseAction.StoreTrue,
            Description = "Start in read-only mode")]
        public bool ReadOnly { get; set; }
    }
}
