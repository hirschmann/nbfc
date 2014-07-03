using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace StagWare.FanControl.Plugins
{
    internal abstract class FanControlPluginLoader<T>
    {
        T fanControlPlugin;

        public T FanControlPlugin
        {
            get
            {
                if (fanControlPlugin == null)
                {
                    OperatingSystem os = Environment.OSVersion;

                    foreach (Lazy<T, IFanControlPluginMetadata> l in this.Plugins)
                    {
                        if (l.Metadata.PlatformId != os.Platform)
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

                        if (!string.IsNullOrWhiteSpace(l.Metadata.PlatformString)
                            && os.VersionString.Contains(l.Metadata.PlatformString))
                        {
                            continue;
                        }

                        this.fanControlPlugin = l.Value;
                    }
                }

                return fanControlPlugin;
            }
        }
        
        public abstract IEnumerable<Lazy<T, IFanControlPluginMetadata>> Plugins { get; set; }

        public FanControlPluginLoader(string path)
        {
            var catalog = new AggregateCatalog(new DirectoryCatalog(path));
            var container = new CompositionContainer(catalog);

            //Fill the imports of this object
            container.ComposeParts(this);
        }
    }
}
