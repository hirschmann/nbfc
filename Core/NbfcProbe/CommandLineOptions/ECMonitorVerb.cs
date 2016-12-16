using clipr;

namespace NbfcProbe.CommandLineOptions
{
    public class ECMonitorVerb
    {
        public ECMonitorVerb()
        {
            Interval = 5;
        }

        [NamedArgument(
            't',
            "timespan",
            Action = ParseAction.Store,
            Constraint = NumArgsConstraint.Exactly,
            NumArgs = 1,
            MetaVar ="seconds",
            Description = "Monitored timespan (default: infinite)")]
        public int Timespan { get; set; }

        [NamedArgument(
            'i',
            "interval",
            Action = ParseAction.Store,
            Constraint = NumArgsConstraint.Exactly,
            NumArgs = 1,
            MetaVar = "seconds",
            Description = "Set poll interval (default: 5)")]
        public int Interval { get; set; }

        [NamedArgument(
            'r',
            "report",
            Action = ParseAction.Store,
            Constraint = NumArgsConstraint.Exactly,
            NumArgs = 1,
            MetaVar = "path",
            Description = "Creates a report file at the specified path")]
        public string ReportPath { get; set; }

        [NamedArgument(
            'c',
            "clearly",
            Action = ParseAction.StoreTrue,
            Description = "Blanks out consecutive duplicate readings")]
        public bool Clearly { get; set; }
    }
}
