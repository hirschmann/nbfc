using StagWare.Configurations;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Xunit;

namespace StagWare.FanControl.Configurations.Tests
{
    public class ConfigManagerTests
    {
        public class Constructor
        {
            [Fact]
            public static void ThrowsIfConfigsDirIsNull()
            {
                Assert.Throws<ArgumentNullException>(
                    () => new ConfigManager<FanControlConfigV2>(null, ".xml", new MockFileSystem()));
            }

            [Fact]
            public static void ThrowsIfConfigsFileExtensionIsNull()
            {
                string dir = Environment.CurrentDirectory;
                Assert.Throws<ArgumentNullException>(
                    () => new ConfigManager<FanControlConfigV2>(dir, null, new MockFileSystem()));
            }
        }

        public class GetConfig
        {
            [Fact]
            public static void ReturnsExistingConfigs()
            {
                var cfgMan = new ConfigManager<FanControlConfigV2>(
                    Environment.CurrentDirectory, ".xml", new MockFileSystem());

                int filesCount = 100;

                for (int i = 0; i < filesCount; i++)
                {
                    cfgMan.AddConfig(new FanControlConfigV2(), i.ToString());
                }

                for (int i = 0; i < filesCount; i++)
                {
                    Assert.True(cfgMan.GetConfig(i.ToString()) != null, $"Config {i} should exist but doesn't");
                }
            }

            [Fact]
            public static void ReturnsNullIfConfigDoesNotExist()
            {
                var cfgMan = new ConfigManager<FanControlConfigV2>(
                    Environment.CurrentDirectory, ".xml", new MockFileSystem());

                int filesCount = 100;

                for (int i = 0; i < filesCount; i++)
                {
                    Assert.True(cfgMan.GetConfig(i.ToString()) == null, $"Config {i} shouldn't exist but exists");
                }
            }

            [Fact]
            public static void ReturnsNullIfIdIsNull()
            {
                var cfgMan = new ConfigManager<FanControlConfigV2>(
                    Environment.CurrentDirectory, ".xml", new MockFileSystem());

                Assert.Null(cfgMan.GetConfig(null));
            }
        }

        public class ConfigFileExists
        {
            [Fact]
            public static void ReturnsTrueIfExists()
            {
                var cfgMan = new ConfigManager<FanControlConfigV2>(
                    Environment.CurrentDirectory, ".xml", new MockFileSystem());

                int filesCount = 100;

                for (int i = 0; i < filesCount; i++)
                {
                    cfgMan.AddConfig(new FanControlConfigV2(), i.ToString());
                }

                for (int i = 0; i < filesCount; i++)
                {
                    Assert.True(cfgMan.ConfigFileExists(i.ToString()), $"Config {i} should exist but doesn't");
                }
            }

            [Fact]
            public static void ReturnsFalseIfNotExisting()
            {
                var cfgMan = new ConfigManager<FanControlConfigV2>(
                    Environment.CurrentDirectory, ".xml", new MockFileSystem());

                int filesCount = 100;

                for (int i = 0; i < filesCount; i++)
                {
                    Assert.False(cfgMan.ConfigFileExists(i.ToString()), $"Config {i} shouldn't exist but exists");
                }
            }

            [Fact]
            public static void ReturnsFalseIfIdIsNull()
            {
                var cfgMan = new ConfigManager<FanControlConfigV2>(
                    Environment.CurrentDirectory, ".xml", new MockFileSystem());

                Assert.False(cfgMan.ConfigFileExists(null));
            }
        }

        public class Contains
        {
            [Fact]
            public static void ReturnsTrueIfExists()
            {
                var cfgMan = new ConfigManager<FanControlConfigV2>(
                    Environment.CurrentDirectory, ".xml", new MockFileSystem());

                int filesCount = 100;

                for (int i = 0; i < filesCount; i++)
                {
                    cfgMan.AddConfig(new FanControlConfigV2(), i.ToString());
                }

                for (int i = 0; i < filesCount; i++)
                {
                    Assert.True(cfgMan.Contains(i.ToString()), $"Config {i} should exist but doesn't");
                }
            }

            [Fact]
            public static void ReturnsFalseIfNotExisting()
            {
                var cfgMan = new ConfigManager<FanControlConfigV2>(
                    Environment.CurrentDirectory, ".xml", new MockFileSystem());

                int filesCount = 100;

                for (int i = 0; i < filesCount; i++)
                {
                    Assert.False(cfgMan.Contains(i.ToString()), $"Config {i} shouldn't exist but exists");
                }
            }

            [Fact]
            public static void ReturnsFalseIfIdIsNull()
            {
                var cfgMan = new ConfigManager<FanControlConfigV2>(
                    Environment.CurrentDirectory, ".xml", new MockFileSystem());

                Assert.False(cfgMan.Contains(null));
            }
        }

        public class AddConfig
        {
            [Fact]
            public static void AddsValidConfigs()
            {
                string extension = ".xml";
                var fs = new MockFileSystem();

                var cfgMan = new ConfigManager<FanControlConfigV2>(
                    Environment.CurrentDirectory, extension, fs);

                int filesCount = 100;

                for (int i = 0; i < filesCount; i++)
                {
                    cfgMan.AddConfig(new FanControlConfigV2(), i.ToString());
                }

                for (int i = 0; i < filesCount; i++)
                {
                    string path = Path.Combine(
                        Environment.CurrentDirectory, i.ToString() + extension);
                    Assert.True(fs.FileExists(path), $"Config {i} should exist but doesn't");
                }
            }

            [Fact]
            public static void ThrowsOnDuplicates()
            {
                string cfgName = "foo";

                var cfgMan = new ConfigManager<FanControlConfigV2>(
                    Environment.CurrentDirectory, ".xml", new MockFileSystem());

                cfgMan.AddConfig(new FanControlConfigV2(), cfgName);

                Assert.Throws<ArgumentException>(
                    () => cfgMan.AddConfig(new FanControlConfigV2(), cfgName));
            }

            [Fact]
            public static void ThrowsIfConfigIsNull()
            {
                var cfgMan = new ConfigManager<FanControlConfigV2>(
                    Environment.CurrentDirectory, ".xml", new MockFileSystem());

                Assert.Throws<ArgumentNullException>(
                    () => cfgMan.AddConfig(null, "foo"));
            }

            [Fact]
            public static void ThrowsIfIdIsNull()
            {
                var cfgMan = new ConfigManager<FanControlConfigV2>(
                    Environment.CurrentDirectory, ".xml", new MockFileSystem());

                Assert.Throws<ArgumentException>(
                    () => cfgMan.AddConfig(new FanControlConfigV2(), null));
            }

            [Fact]
            public static void ThrowsIfIdContainsInvalidFileNameChars()
            {
                var cfgMan = new ConfigManager<FanControlConfigV2>(
                    Environment.CurrentDirectory, ".xml", new MockFileSystem());

                string id = "foo" + Path.GetInvalidFileNameChars().First();

                Assert.Throws<ArgumentException>(
                    () => cfgMan.AddConfig(new FanControlConfigV2(), id));
            }
        }

        public class RemoveConfig
        {
            [Fact]
            public static void RemovesConfigs()
            {
                string extension = ".xml";
                var fs = new MockFileSystem();

                var cfgMan = new ConfigManager<FanControlConfigV2>(
                    Environment.CurrentDirectory, extension, fs);

                int filesCount = 100;

                for (int i = 0; i < filesCount; i++)
                {
                    cfgMan.AddConfig(new FanControlConfigV2(), i.ToString());
                }

                for (int i = 0; i < filesCount; i += 2)
                {
                    string path = Path.Combine(
                        Environment.CurrentDirectory, i.ToString() + extension);

                    cfgMan.RemoveConfig(i.ToString());

                    Assert.False(fs.FileExists(path), $"Config {i} shouldn't exist but exists");
                }
            }

            [Fact]
            public static void DoesNothingIfNotExisting()
            {
                var cfgMan = new ConfigManager<FanControlConfigV2>(
                    Environment.CurrentDirectory, ".xml", new MockFileSystem());

                for (int i = 0; i < 100; i++)
                {
                    cfgMan.RemoveConfig(i.ToString());
                }
            }

            [Fact]
            public static void ThrowsIfIdIsNull()
            {
                var cfgMan = new ConfigManager<FanControlConfigV2>(
                    Environment.CurrentDirectory, ".xml", new MockFileSystem());

                Assert.Throws<ArgumentNullException>(() => cfgMan.RemoveConfig(null));
            }
        }

        public class UpdateConfig
        {
            [Fact]
            public static void UpdatesExistingConfigs()
            {
                var cfgMan = new ConfigManager<FanControlConfigV2>(
                   Environment.CurrentDirectory, ".xml", new MockFileSystem());

                string id = "foo";
                var cfg = new FanControlConfigV2();
                cfgMan.AddConfig(cfg, id);

                cfg.Author = "bar";
                cfgMan.UpdateConfig(id, cfg);

                var updatedCfg = cfgMan.GetConfig(id);
                Assert.True(updatedCfg.Author == cfg.Author);
            }

            [Fact]
            public static void ThrowsIfIdNotExisting()
            {
                var cfgMan = new ConfigManager<FanControlConfigV2>(
                   Environment.CurrentDirectory, ".xml", new MockFileSystem());

                Assert.Throws<KeyNotFoundException>(
                    () => cfgMan.UpdateConfig("foo", new FanControlConfigV2()));
            }

            [Fact]
            public static void ThrowsIfIdIsNull()
            {
                var cfgMan = new ConfigManager<FanControlConfigV2>(
                   Environment.CurrentDirectory, ".xml", new MockFileSystem());

                Assert.Throws<KeyNotFoundException>(
                    () => cfgMan.UpdateConfig(null, new FanControlConfigV2()));
            }

            [Fact]
            public static void ThrowsIfConfigIsNull()
            {
                var cfgMan = new ConfigManager<FanControlConfigV2>(
                   Environment.CurrentDirectory, ".xml", new MockFileSystem());

                cfgMan.AddConfig(new FanControlConfigV2(), "foo");

                Assert.Throws<ArgumentNullException>(
                    () => cfgMan.UpdateConfig("foo", null));
            }
        }
    }
}
