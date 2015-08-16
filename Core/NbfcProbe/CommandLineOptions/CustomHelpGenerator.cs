using clipr;
using clipr.Core;
using clipr.Triggers;
using clipr.Usage;
using System;
using System.Linq;
using System.Text;

namespace NbfcProbe.CommandLineOptions
{
    public class CustomHelpGenerator : TriggerBase, IHelpGenerator<Verbs>
    {
        #region Constructors

        public CustomHelpGenerator()
        {
            this.ShortName = 'h';
            this.LongName = "help";
        }

        #endregion

        #region IHelpGenerator implementation

        public override string Description
        {
            get { return "Get help."; }
        }

        public override string Name
        {
            get { return "Help"; }
        }

        public string PluginName
        {
            get { return "HelpGenerator"; }
        }

        public string GetHelp(IParserConfig<Verbs> config)
        {
            var sb = new StringBuilder();
            sb.Append(GetUsage());
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("commands:");
            sb.AppendLine(" * ec-dump");
            sb.AppendLine("    Dump all registers that are present in EC memory as hexadecimal table.");
            sb.AppendLine(" * ec-read <register>");
            sb.AppendLine("    Print the value of the given EC register number.");
            sb.AppendLine(" * ec-write <register> <value> [-v|--verbose]");
            sb.AppendLine("    Write the given value to the given EC register number.");
            sb.AppendLine("    If the verbose option is set, read and print the updated value.");
            sb.AppendLine();
            sb.AppendLine("Input values are interpreted as decimal numbers by default.");
            sb.AppendLine("Hexadecimal values may be entered by prefixing them with \"0x\".");
           
            return sb.ToString();
        }        

        public string GetUsage()
        {
            var attrib = typeof(Verbs).GetCustomAttributes(typeof(ApplicationInfoAttribute), false)
                .FirstOrDefault() as ApplicationInfoAttribute;

            return string.Format("usage: {0} [--version] [--help] <command> [<args>]", attrib.Name);
        }

        public void OnParse(IParserConfig<Verbs> config)
        {
            Console.WriteLine(GetHelp(config));
        }

        #endregion
    }
}
