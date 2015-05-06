using clipr.Usage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using clipr;
using clipr.Core;
using clipr.Triggers;
using System.Reflection;

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
            var sb = new StringBuilder();
            sb.Append(GetUsage());
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("commands:");

            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                var attrib = prop.GetCustomAttributes(typeof(VerbAttribute), false).FirstOrDefault() as VerbAttribute;

                if (attrib != null)
                {
                    string cmd = attrib.Name;

                    if (prop.PropertyType.GetProperties().Length > 0)
                    {
                        cmd += " [options]";
                    }

                    sb.AppendFormat("  {0,-25}{1}",cmd , attrib.Description);
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        public string GetUsage()
        {
            var attrib = typeof(T).GetCustomAttributes(typeof(ApplicationInfoAttribute), false)
                .FirstOrDefault() as ApplicationInfoAttribute;

            return string.Format("usage: {0} [--version] [--help] <command> [<args>]", attrib.Name);
        }

        public void OnParse(IParserConfig<T> config)
        {
            Console.WriteLine(GetHelp(config));
        }

        public string PluginName
        {
            get { return "HelpGenerator"; }
        }
    }
}
