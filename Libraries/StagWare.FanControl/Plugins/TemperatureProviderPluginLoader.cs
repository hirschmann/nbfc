using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace StagWare.FanControl.Plugins
{
    internal class TemperatureProviderPluginLoader : FanControlPluginLoader<ITemperatureProvider>
    {
        [ImportMany(typeof(ITemperatureProvider), AllowRecomposition = true)]
        public override IEnumerable<Lazy<ITemperatureProvider, IFanControlPluginMetadata>> Plugins { get; set; }

        public TemperatureProviderPluginLoader(string pluginsPath)
            : base(pluginsPath)
        {
        }
    }
}
