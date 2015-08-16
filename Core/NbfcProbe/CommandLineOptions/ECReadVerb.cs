using clipr;

namespace NbfcProbe.CommandLineOptions
{
    public class ECReadVerb
    {
        [PositionalArgument(
            0,
            Action = ParseAction.Store,
            Constraint = NumArgsConstraint.Exactly,
            NumArgs = 1,
            MetaVar = "register")]
        public byte Register { get; set; }
    }
}
