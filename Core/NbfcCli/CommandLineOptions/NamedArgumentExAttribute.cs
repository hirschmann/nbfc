using clipr;

namespace NbfcCli.CommandLineOptions
{
    public class NamedArgumentExAttribute : NamedArgumentAttribute
    {
        public NamedArgumentExAttribute(char shortName)
            : base(shortName)
        {
        }

        public NamedArgumentExAttribute(string longName)
            : base(longName)
        {
        }

        public NamedArgumentExAttribute(char shortName, string longName)
            : base(shortName, longName)
        {
        }

        public string ArgumentName { get; set; }
    }
}
