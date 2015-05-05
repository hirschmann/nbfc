using clipr.Usage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using clipr;
using clipr.Core;
using clipr.Triggers;

namespace NbfcCli.CommandLineOptions
{
    public class HelpGenerator<T> : TriggerBase, IHelpGenerator<T> where T : class
    {
        public HelpGenerator()
        {
            this.ShortName = 'h';
            this.LongName = "help";
        }

        public override string Description
        {
            get { return "Get help."; }
        }

        public override string Name
        {
            get { return "Help"; }
        }

        public string GetHelp(IParserConfig<T> config)
        {
            return "";
        }

        public string GetUsage()
        {
            var attrib = typeof(T).GetCustomAttributes(typeof(ApplicationInfoAttribute), false)
                .FirstOrDefault() as ApplicationInfoAttribute;

            return string.Format("usage: {0} [--version] [--help] <command> [<args>]", attrib.Name);
        }

        public void OnParse(IParserConfig<T> config)
        {
            Console.WriteLine(GetUsage());
        }

        public string PluginName
        {
            get { return "HelpGenerator"; }
        }
    }
}
