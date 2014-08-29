using System;

namespace StagWare.FanControl.Configurations
{
    public interface IFanControlConfig : ICloneable
    {
        string NotebookModel { get; set; }
        int EcPollInterval { get; set; }
        bool ReadWriteWords { get; set; }
    }
}
