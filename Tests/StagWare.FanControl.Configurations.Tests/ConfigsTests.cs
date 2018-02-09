using StagWare.FanControl.Configurations.Validation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace StagWare.FanControl.Configurations.Tests
{
    public class ConfigsTests
    {
        [Fact]
        public void AreConfigsValid()
        {
            var configMan = new FanControlConfigManager(GetConfigsDir());
            var validator = new FanControlConfigValidator();

            foreach (string name in configMan.ConfigNames)
            {
                var cfg = configMan.GetConfig(name);
                Assert.True(cfg != null, $"{name} config could not be loaded");

                var result = validator.Validate(cfg, false, false);
                StringBuilder message = null;

                if (!result.Success)
                {
                    message = new StringBuilder();
                    message.AppendFormat("{0} config is not valid:", name);
                    message.AppendLine();

                    foreach (var validation in result.Failed)
                    {
                        message.AppendFormat("- {0}", validation.RuleDescription);
                        message.AppendLine();
                        message.AppendFormat("--> {0}", validation.Reason);
                        message.AppendLine();
                        message.AppendLine();
                    }
                }

                Assert.True(result.Success, message?.ToString());
            }
        }

        [Fact]
        public void HaveAllConfigsBeenLoaded()
        {
            string path = GetConfigsDir();
            var configMan = new FanControlConfigManager(path);
            var configsLookup = new HashSet<string>(
                Directory.GetFiles(path).Select(x => Path.GetFileNameWithoutExtension(x)));

            Assert.True(configsLookup.SetEquals(configMan.ConfigNames));
        }

        [Fact]
        public void DoAllConfigsHaveXmlFileExtension()
        {
            string path = GetConfigsDir();
            Assert.True(Directory.GetFiles(path).All(x => x.EndsWith(".xml")));
        }

        private static string GetConfigsDir()
        {
            string path = Directory.GetParent(Environment.CurrentDirectory)?
                .Parent?.Parent?.Parent?.FullName;

            if (path == null)
            {
                return null;
            }

            return Path.Combine(path, "Configs");
        }
    }
}
