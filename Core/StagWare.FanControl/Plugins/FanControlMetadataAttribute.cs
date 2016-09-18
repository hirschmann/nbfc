using System;
using System.ComponentModel.Composition;

namespace StagWare.FanControl.Plugins
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class FanControlPluginMetadataAttribute : ExportAttribute
    {
        public const int DefaultPriority = 10;

        public FanControlPluginMetadataAttribute(
            string uniqueId, SupportedPlatforms supportedPlatforms, 
            SupportedCpuArchitectures supportedCpuArchitectures) 
            : this(uniqueId, supportedPlatforms, supportedCpuArchitectures, DefaultPriority)
        {
        }

        public FanControlPluginMetadataAttribute(
            string uniqueId,
            SupportedPlatforms supportedPlatforms,
            SupportedCpuArchitectures supportedCpuArchitectures,
            int priority)
            : base(typeof(IFanControlPluginMetadata))
        {
            this.UniqueId = uniqueId;
            this.SupportedPlatforms = supportedPlatforms;
            this.SupportedCpuArchitectures = supportedCpuArchitectures;
            this.Priority = priority;
        }

        public string UniqueId { get; set; }
        public string MinOSVersion { get; set; }
        public string MaxOSVersion { get; set; }
        public SupportedPlatforms SupportedPlatforms { get; set; }
        public SupportedCpuArchitectures SupportedCpuArchitectures { get; set; }
        public int Priority { get; set; }
    }
}
