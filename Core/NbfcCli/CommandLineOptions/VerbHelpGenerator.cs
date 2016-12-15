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
    public class VerbsHelpGenerator : TriggerBase, IHelpGenerator
    {
        #region Constructors

        public VerbsHelpGenerator()
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

        public string GetHelp(IParserConfig config)
        {
            var sb = new StringBuilder();
            sb.Append(GetUsage(config));
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("commands:");

            foreach (PropertyInfo verb in typeof(Verbs).GetProperties())
            {
                var attrib = verb.GetCustomAttributes(typeof(VerbAttribute), false)
                    .FirstOrDefault() as VerbAttribute;

                if (attrib != null)
                {
                    PropertyInfo[] properties = verb.PropertyType.GetProperties();
                    AppendVerbHelpText(sb, attrib, config.ArgumentPrefix, properties);
                }
            }

            return sb.ToString();
        }        

        public string GetUsage(IParserConfig config)
        {
            var attrib = typeof(Verbs).GetCustomAttributes(typeof(ApplicationInfoAttribute), false)
                .FirstOrDefault() as ApplicationInfoAttribute;            

            return string.Format("usage: {0} [--version] [--help] <command> [<args>]", attrib.Name);
        }

        public void OnParse(IParserConfig config)
        {
            Console.WriteLine(GetHelp(config));
        }

        #endregion

        #region Private Methods

        private static void AppendVerbHelpText(StringBuilder sb, VerbAttribute attrib, char argPrefix, PropertyInfo[] verbProperties)
        {
            string cmd = attrib.Name;

            if (verbProperties.Length > 0)
            {
                cmd += " [options]";
            }

            sb.AppendFormat("  {0,-25}{1}", cmd, attrib.Description);
            sb.AppendLine();

            foreach (PropertyInfo param in verbProperties)
            {
                var paramAttrib = param.GetCustomAttributes(typeof(NamedArgumentExAttribute), false)
                    .FirstOrDefault() as NamedArgumentExAttribute;

                if (paramAttrib != null)
                {
                    AppendArgHelpText(sb, argPrefix, paramAttrib);
                }
            }

            sb.AppendLine();
        }

        private static void AppendArgHelpText(StringBuilder sb, char argPrefix, NamedArgumentExAttribute paramAttrib)
        {
            string s = string.Format(
                    "{0}{1}, {0}{0}{2}",
                    argPrefix,
                    paramAttrib.ShortName,
                    paramAttrib.LongName);

            if (paramAttrib.ArgumentName != null)
            {
                string format = " <{0}>";

                if (paramAttrib.NumArgs < 1)
                {
                    format = " [<{0}>]";
                }

                s += string.Format(format, paramAttrib.ArgumentName);
            }

            sb.AppendFormat("    {0,-27}{1}", s, paramAttrib.Description);
            sb.AppendLine();
        }

        #endregion
    }
}
