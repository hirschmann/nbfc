using clipr;
using System.Collections.Generic;

namespace NbfcCli.CommandLineOptions
{
    public class StatusVerb
    {
        [MutuallyExclusiveGroup("fan")]
        [MutuallyExclusiveGroup("service")]
        [NamedArgumentEx(
            'a',
            "all",
            Action = ParseAction.StoreTrue,
            Description = "Show service and fan status")]
        public bool All { get; set; }

        [MutuallyExclusiveGroup("service")]
        [NamedArgumentEx(
            's',
            "service",
            Action = ParseAction.StoreTrue,
            Description = "Show service status")]
        public bool Service { get; set; }

        [MutuallyExclusiveGroup("fan")]
        [NamedArgumentEx(
            'f',
            "fan",
            ArgumentName = "index",
            Action = ParseAction.Append,
            Description = "Show fan status")]
        public List<int> Fan { get; set; }
    }
}