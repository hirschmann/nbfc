using clipr;
using clipr.Core;
using clipr.Triggers;
using clipr.Usage;
using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NbfcCli.CommandLineOptions
{
    public class VerbsHelpGenerator : TriggerBase, IHelpGenerator<Verbs>
    {
        public VerbsHelpGenerator()
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

        public string GetHelp(IParserConfig<Verbs> config)
        {
            var sb = new StringBuilder();
            sb.Append(GetUsage());
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("commands:");

            foreach (PropertyInfo verb in typeof(Verbs).GetProperties())
            {
                var verbAttrib = verb.GetCustomAttributes(typeof(VerbAttribute), false)
                    .FirstOrDefault() as VerbAttribute;

                if (verbAttrib != null)
                {
                    string cmd = verbAttrib.Name;

                    if (verb.PropertyType.GetProperties().Length > 0)
                    {
                        cmd += " [options]";
                    }

                    sb.AppendFormat("  {0,-25}{1}",cmd , verbAttrib.Description);
                    sb.AppendLine();

                    foreach (PropertyInfo param in verb.PropertyType.GetProperties())
                    {
                        var paramAttrib = param.GetCustomAttributes(typeof(NamedArgumentExAttribute), false)
                            .FirstOrDefault() as NamedArgumentExAttribute;

                        if (paramAttrib != null)
                        {
                            cmd = string.Format(
                                "{0}{1}, {0}{0}{2}", 
                                config.ArgumentPrefix, 
                                paramAttrib.ShortName, 
                                paramAttrib.LongName);

                            if (paramAttrib.ArgumentName != null)
                            {
                                cmd += string.Format(" <{0}>", paramAttrib.ArgumentName);
                            }
                            
                            sb.AppendFormat("    {0,-27}{1}", cmd, paramAttrib.Description);
                            sb.AppendLine();
                        }
                    }

                    sb.AppendLine();
                }
            }

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

        public string PluginName
        {
            get { return "HelpGenerator"; }
        }
    }
}
