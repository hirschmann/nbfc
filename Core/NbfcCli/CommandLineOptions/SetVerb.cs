using clipr;
using System.Collections.Generic;

namespace NbfcCli.CommandLineOptions
{
    public class SetVerb
    {
        [MutuallyExclusiveGroup("speed")]
        [NamedArgument('a',"auto", Action= ParseAction.StoreTrue, Description="Set fan speed to 'auto'")]
        public bool Auto { get; set; }

        [MutuallyExclusiveGroup("speed")]
        [NamedArgument('s', "speed", Action = ParseAction.Store, Description = "Set fan speed.")]
        public string Speed { get; set; }

        [NamedArgument('f', "fan", Action = ParseAction.Append, Description = "Fan index (zero based).")]
        public List<int> Fan { get; set; }
    }
}
