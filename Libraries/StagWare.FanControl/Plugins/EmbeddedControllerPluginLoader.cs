using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace StagWare.FanControl.Plugins
{
    internal class EmbeddedControllerPluginLoader : FanControlPluginLoader<IEmbeddedController>
    {
        [ImportMany(typeof(IEmbeddedController), AllowRecomposition = true)]
        public override IEnumerable<Lazy<IEmbeddedController, IFanControlPluginMetadata>> Plugins { get; set; }

        public EmbeddedControllerPluginLoader(string pluginsPath)
            : base(pluginsPath)
        {
        }
    }
}
