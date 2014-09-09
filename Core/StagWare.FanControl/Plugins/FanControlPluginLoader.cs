using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace StagWare.FanControl.Plugins
{
    public class FanControlPluginLoader<T>
    {
        T fanControlPlugin;

        public T FanControlPlugin
        {
            get
            {
                if (fanControlPlugin == null)
                {
                    OperatingSystem os = Environment.OSVersion;
                    var platform = SupportedPlatforms.None;
                    var arch = SupportedCpuArchitectures.None;

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
                    }

                    switch (IntPtr.Size)
                    {
                        case 4:
                            arch = SupportedCpuArchitectures.x86;
                            break;

                        case 8:
                            arch = SupportedCpuArchitectures.x64;
                            break;
                    }

                    foreach (Lazy<T, IFanControlPluginMetadata> l in this.Plugins)
                    {
                        if (!l.Metadata.SupportedPlatforms.HasFlag(platform))
                        {
                            continue;
                        }

                        if (!l.Metadata.SupportedCpuArchitectures.HasFlag(arch))
                        {
                            continue;
                        }

                        Version version;

                        if (Version.TryParse(l.Metadata.MinOSVersion, out version)
                            && version > os.Version)
                        {                            
                            continue;
                        }

                        if (Version.TryParse(l.Metadata.MaxOSVersion, out version)
                            && version < os.Version)
                        {
                            continue;
                        }

                        this.fanControlPlugin = l.Value;
                    }
                }

                return fanControlPlugin;
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
    }
}
