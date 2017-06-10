using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace StagWare.FanControl.Configurations.Tests
{
    public class ConfigsTests
    {
        [Fact]
        public void AreConfigsValid()
        {
            var configMan = new FanControlConfigManager(GetConfigsDir());

            foreach (string name in configMan.ConfigNames)
            {
                Assert.True(configMan.GetConfig(name) != null, $"{name} is invalid");
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
