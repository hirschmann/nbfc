using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StagWare.Settings
{
    public sealed partial class ServiceSettings
    {
        public string SelectedConfigId { get; set; }
        public bool Autostart { get; set; }
        public float[] TargetFanSpeeds { get; set; }
    }
}
