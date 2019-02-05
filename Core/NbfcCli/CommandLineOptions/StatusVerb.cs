using clipr;
using System.Collections.Generic;

namespace NbfcCli.CommandLineOptions
{
    public class StatusVerb
    {
        [MutuallyExclusiveGroup("fan")]
        [MutuallyExclusiveGroup("service")]
        [NamedArgument(
            'a',
            "all",
            Action = ParseAction.StoreTrue,
            Description = "Show service and fan status (default)")]
        public bool All { get; set; }

        [MutuallyExclusiveGroup("service")]
        [NamedArgument(
            's',
            "service",
            Action = ParseAction.StoreTrue,
            Description = "Show service status")]
        public bool Service { get; set; }

        [MutuallyExclusiveGroup("fan")]
        [NamedArgument(
            'f',
            "fan",
            MetaVar = "index",
            Action = ParseAction.Append,
            NumArgs = 0,
            Constraint = NumArgsConstraint.AtLeast,
            Description = "Show fan status")]
        public List<int> Fan { get; set; }

        [NamedArgument(
            'p',
            "poll",
            Constraint = NumArgsConstraint.Optional,
            Action = ParseAction.Store,
            Description = "Output selected status every <value> seconds (default: 1)",
            Const = 1f)]
        public float? Poll { get; set; }
    }
}