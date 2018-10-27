using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;

namespace StagWare.FanControl.Plugins
{
    public class FanControlPluginLoader<T> where T : IFanControlPlugin
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        T fanControlPlugin;
        string fanControlPluginId;

        public T FanControlPlugin
        {
            get
            {
                if (fanControlPlugin == null)
                {
                    SelectPlugin();
                }

                return fanControlPlugin;
            }
        }

        public string FanControlPluginId
        {
            get
            {
                if (fanControlPlugin == null)
                {
                    SelectPlugin();
                }

                return fanControlPluginId;
            }
        }

        [ImportMany]
        public IEnumerable<Lazy<T, IFanControlPluginMetadata>> Plugins { get; set; }

        public FanControlPluginLoader(string path)
        {
            var dirCatalog = new DirectoryCatalog(path);
            var aggCatalog = new AggregateCatalog(dirCatalog);
            var container = new CompositionContainer(aggCatalog);

            //Fill the imports of this object
            container.ComposeParts(this);
        }

        private void SelectPlugin()
        {
            OperatingSystem os = Environment.OSVersion;
            SupportedPlatforms platform;
            SupportedCpuArchitectures arch;

            switch (os.Platform)
            {
                case PlatformID.Win32NT:
                    platform = SupportedPlatforms.Windows;
                    break;

                case PlatformID.Unix:
                    platform = SupportedPlatforms.Unix;
                    break;

                case PlatformID.MacOSX:
                    platform = SupportedPlatforms.MacOSX;
                    break;

                default:
                    platform = SupportedPlatforms.None;
                    break;
            }

            switch (IntPtr.Size)
            {
                case 4:
                    arch = SupportedCpuArchitectures.x86;
                    break;

                case 8:
                    arch = SupportedCpuArchitectures.x64;
                    break;

                default:
                    arch = SupportedCpuArchitectures.None;
                    break;
            }

            var orderedPlugins = this.Plugins.OrderByDescending(x => x.Metadata.Priority);

            foreach (Lazy<T, IFanControlPluginMetadata> l in orderedPlugins)
            {
                if (!IsPluginCompatible(l.Metadata, platform, os.Version, arch))
                {
                    continue;
                }

                if (TryInitPlugin(l.Value))
                {
                    this.fanControlPlugin = l.Value;
                    this.fanControlPluginId = l.Metadata.UniqueId;
                    break;
                }
            }
        }

        private static bool TryInitPlugin(T plugin)
        {
            bool isPluginInitialized = false;

            try
            {
                plugin.Initialize();
                isPluginInitialized = plugin.IsInitialized;
            }
            catch (Exception e)
            {
                logger.Warn(e, "Plugin initialization failed");
            }

            if (!isPluginInitialized)
            {
                try
                {
                    plugin.Dispose();
                }
                catch (Exception e)
                {
                    logger.Warn(e, "Plugin disposal failed");
                }
            }

            return isPluginInitialized;
        }

        private static bool IsPluginCompatible(
            IFanControlPluginMetadata metadata,
            SupportedPlatforms platform,
            Version platformVersion,
            SupportedCpuArchitectures arch)
        {
            if (!metadata.SupportedPlatforms.HasFlag(platform))
            {
                return false;
            }

            if (!metadata.SupportedCpuArchitectures.HasFlag(arch))
            {
                return false;
            }

            Version version;

            if (Version.TryParse(metadata.MinOSVersion, out version)
                && version > platformVersion)
            {
                return false;
            }

            if (Version.TryParse(metadata.MaxOSVersion, out version)
                && version < platformVersion)
            {
                return false;
            }

            return true;
        }
    }
}
