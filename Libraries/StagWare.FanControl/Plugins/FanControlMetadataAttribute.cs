using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace StagWare.FanControl.Plugins
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class FanControlPluginMetadataAttribute : ExportAttribute
    {
        public FanControlPluginMetadataAttribute(
            string uniqueId, 
            SupportedPlatforms supportedPlatforms,
            SupportedCpuArchitectures supportedCpuArchitectures)
            : base(typeof(IFanControlPluginMetadata))
        {
            this.UniqueId = uniqueId;
            this.SupportedPlatforms = supportedPlatforms;
        }

        public string UniqueId { get; set; }
        public string MinOSVersion { get; set; }
        public string MaxOSVersion { get; set; }
        public SupportedPlatforms SupportedPlatforms { get; set; }
        public SupportedCpuArchitectures SupportedCpuArchitectures { get; set; }
    }
}
