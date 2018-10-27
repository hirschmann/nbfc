using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace StagWare.FanControl.Configurations.Tests
{
    public class FanControlConfigManagerTests
    {
        public class SelectConfig
        {
            [Fact]
            public static void ReturnsTrueAndSetsPropertiesIfExisting()
            {
                var cfgMan = new FanControlConfigManager(
                    Environment.CurrentDirectory, ".xml", new MockFileSystem());
                string cfgName = "foo";

                cfgMan.AddConfig(new FanControlConfigV2(), cfgName);

                Assert.True(cfgMan.SelectConfig(cfgName));
                Assert.NotNull(cfgMan.SelectedConfig);
                Assert.True(
                    cfgMan.SelectedConfigName == cfgName,
                    $"Config name should be {cfgName}, but is {cfgMan.SelectedConfigName}");
            }

            [Fact]
            public static void ReturnsFalseClearsPropertiesIfNotExisting()
            {
                var cfgMan = new FanControlConfigManager(
                    Environment.CurrentDirectory, ".xml", new MockFileSystem());
                string cfgName = "foo";

                cfgMan.AddConfig(new FanControlConfigV2(), cfgName);
                cfgMan.SelectConfig(cfgName);

                Assert.False(cfgMan.SelectConfig("bar"));
                Assert.Null(cfgMan.SelectedConfig);
                Assert.Null(cfgMan.SelectedConfigName);
            }
        }

        public class RecommendConfigs
        {
            [Fact]
            public static void ReturnsOnlyValidSuggestions()
            {
                string[] notebooks = new[] { "HP ProBook 1234", "HP EliteBook 1234", "Acer Foo 7683" };
                var cfgMan = new FanControlConfigManager(
                    Environment.CurrentDirectory, ".xml", new MockFileSystem());

                int i = 0;

                foreach (string s in notebooks)
                {
                    var cfg = new FanControlConfigV2()
                    {
                        FanConfigurations = new List<FanConfiguration>()
                        {
                            new FanConfiguration()
                            {
                                WriteRegister = i,
                                ReadRegister = i + 1
                            }
                        }
                    };

                    cfgMan.AddConfig(cfg, s);
                    i++;
                }

                List<string> recommendations = cfgMan.RecommendConfigs("HP ProBook 3334");
                Assert.Contains(notebooks[0], recommendations);
                Assert.Contains(notebooks[1], recommendations);
                Assert.DoesNotContain(notebooks[2], recommendations);
            }

            [Fact]
            public static void DoNotRecommendConfigsWithSameRwRegisters()
            {
                string[] notebooks = new[] { "HP ProBook 1234", "HP ProBook 1235" };
                var cfgMan = new FanControlConfigManager(
                    Environment.CurrentDirectory, ".xml", new MockFileSystem());

                foreach (string s in notebooks)
                {
                    var cfg = new FanControlConfigV2()
                    {
                        FanConfigurations = new List<FanConfiguration>()
                        {
                            new FanConfiguration()
                        }
                    };

                    cfgMan.AddConfig(cfg, s);
                }

                List<string> recommendations = cfgMan.RecommendConfigs("HP ProBook 1234");
                Assert.Contains(notebooks[0], recommendations);
                Assert.DoesNotContain(notebooks[1], recommendations);
            }

            [Fact]
            public static void ReturnsEmptyListIfModelIsNull()
            {
                var cfgMan = new FanControlConfigManager(
                    Environment.CurrentDirectory, ".xml", new MockFileSystem());

                Assert.Empty(cfgMan.RecommendConfigs("HP ProBook 3334"));
            }
        }
    }
}
