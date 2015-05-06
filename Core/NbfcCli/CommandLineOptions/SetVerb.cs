using clipr;
using System.Collections.Generic;

namespace NbfcCli.CommandLineOptions
{
    public class SetVerb
    {
        [MutuallyExclusiveGroup("speed")]
        [NamedArgumentEx(
            'a',
            "auto",
            Action = ParseAction.StoreTrue,
            Description = "Set fan speed to 'auto'")]
        public bool Auto { get; set; }

        [MutuallyExclusiveGroup("speed")]
        [NamedArgumentEx(
            's',
            "speed",
            ArgumentName = "value",
            Action = ParseAction.Store,
            Description = "Set fan speed")]
        public string Speed { get; set; }

        [NamedArgumentEx(
            'f',
            "fan",
            ArgumentName = "index",
            Action = ParseAction.Append,
            Description = "Fan index (zero based)")]
        public List<int> Fan { get; set; }
    }
}
