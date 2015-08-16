using clipr;

namespace NbfcProbe.CommandLineOptions
{
    public class ECWriteVerb
    {
        [PositionalArgument(
            0,
            Action = ParseAction.Store,
            Constraint = NumArgsConstraint.Exactly,
            NumArgs = 1,
            MetaVar = "register")]
        public byte Register { get; set; }

        [PositionalArgument(
            1,
            Action = ParseAction.Store,
            Constraint = NumArgsConstraint.Exactly,
            NumArgs = 1,
            MetaVar = "value")]
        public byte Value { get; set; }
        
        [NamedArgument(
            'v',
            "verbose",
            Action = ParseAction.StoreTrue,
            Description = "Print extra info")]
        public bool Verbose { get; set; }
    }
}
