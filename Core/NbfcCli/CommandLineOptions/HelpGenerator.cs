using clipr;
using clipr.Core;
using clipr.Triggers;
using clipr.Usage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NbfcCli.CommandLineOptions
{
    public class HelpGenerator<T> : TriggerBase, IHelpGenerator
    {
        #region Constructors

        public HelpGenerator()
        {
            this.ShortName = 'h';
            this.LongName = "help";
            this.DescriptionDistance = 25;
        }

        #endregion

        #region Properties

        public int DescriptionDistance { get; set; }
        public string GenericDescription { get; set; }

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

            string descr = GetApplicationDescription();

            if (!string.IsNullOrWhiteSpace(descr))
            {
                sb.AppendLine(descr);
                sb.AppendLine();
            }

            sb.Append(GetUsage(config));
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("commands:");

            foreach (PropertyInfo verb in typeof(T).GetProperties())
            {
                var attrib = GetAttribute<VerbAttribute>(verb);

                if (attrib != null)
                {
                    PropertyInfo[] properties = verb.PropertyType.GetProperties();
                    AppendVerbHelpText(sb, attrib, config.ArgumentPrefix, properties);
                }
            }

            if (!string.IsNullOrWhiteSpace(GenericDescription))
            {
                sb.AppendLine(GenericDescription);
            }

            return sb.ToString();
        }        

        public string GetUsage(IParserConfig config)
        {
            var attrib = GetAttribute<ApplicationInfoAttribute>(typeof(T));
            return string.Format("usage: {0} [--version] [--help] <command> [<args>]", attrib.Name);
        }

        public void OnParse(IParserConfig config)
        {
            Console.WriteLine(GetHelp(config));
        }

        #endregion

        #region Private Methods

        private static string GetApplicationDescription()
        {
            return GetAttribute<ApplicationInfoAttribute>(typeof(T))?.Description;
        }

        private static TAttribute GetAttribute<TAttribute>(MemberInfo info) where TAttribute : Attribute
        {
            return info.GetCustomAttributes(typeof(TAttribute), false).FirstOrDefault() as TAttribute;
        }

        private void AppendVerbHelpText(StringBuilder sb, VerbAttribute attrib, char argPrefix, PropertyInfo[] verbProperties)
        {
            var namedArgAttributes = new List<NamedArgumentAttribute>();
            var cmd = new StringBuilder(attrib.Name);

            foreach (PropertyInfo param in verbProperties)
            {
                var positionalArg = GetAttribute<PositionalArgumentAttribute>(param);
                var namedArg = GetAttribute<NamedArgumentAttribute>(param);

                if (positionalArg?.MetaVar != null)
                {
                    cmd.AppendFormat(" <{0}>", positionalArg.MetaVar);
                }

                if(namedArg != null)
                {
                    namedArgAttributes.Add(namedArg);
                }
            }

            if (namedArgAttributes.Count > 0)
            {
                cmd.Append(" [options]");
            }

            sb.Append("  ");
            sb.AppendFormat($"{{0,{-DescriptionDistance}}}", cmd.ToString());

            if (cmd.Length >= DescriptionDistance)
            {
                sb.AppendLine();
                sb.Append("  ");
                sb.AppendFormat($"{{0,{-DescriptionDistance}}}", "");
            }

            sb.Append(attrib.Description);
            sb.AppendLine();

            foreach (NamedArgumentAttribute arg in namedArgAttributes)
            {
                AppendArgHelpText(sb, argPrefix, arg);
            }

            sb.AppendLine();
        }

        private void AppendArgHelpText(StringBuilder sb, char argPrefix, NamedArgumentAttribute paramAttrib)
        {
            string s = string.Format(
                    "{0}{1}, {0}{0}{2}",
                    argPrefix,
                    paramAttrib.ShortName,
                    paramAttrib.LongName);

            if ((paramAttrib.MetaVar != null) && (paramAttrib.MetaVar != paramAttrib.ShortName.ToString()))
            {
                string format = " <{0}>";

                if (paramAttrib.NumArgs < 1)
                {
                    format = " [<{0}>]";
                }

                s += string.Format(format, paramAttrib.MetaVar);
            }

            sb.Append("    ");
            sb.AppendFormat($"{{0,{-DescriptionDistance}}}{{1}}", s, paramAttrib.Description);
            sb.AppendLine();
        }

        #endregion
    }
}
