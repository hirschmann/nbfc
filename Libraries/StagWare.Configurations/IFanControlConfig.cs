using StagWare.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StagWare.FanControl.Configurations
{
    public interface IFanControlConfig : ICloneable
    {
        string NotebookModel { get; set; }
        int EcPollInterval { get; set; }
        bool ReadWriteWords { get; set; }
    }
}
