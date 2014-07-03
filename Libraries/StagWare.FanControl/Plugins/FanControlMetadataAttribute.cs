using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace StagWare.FanControl.Plugins
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class FanControlPluginMetadataAttribute : ExportAttribute, IFanControlPluginMetadata
    {
        public FanControlPluginMetadataAttribute(string uniqueId, PlatformID id)
            : base(typeof(IFanControlPluginMetadata))
        {
            this.UniqueId = uniqueId;
            this.PlatformId = id;
        }

        public string UniqueId { get; set; }
        public string MinOSVersion { get; set; }
        public string MaxOSVersion { get; set; }
        public PlatformID PlatformId { get; set; }
        public string PlatformString { get; set; }
    }
}
