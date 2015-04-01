using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StagWare.Settings
{
    public sealed partial class AppSettings
    {
        public string Config { get; set; }
        public bool Enabled { get; set; }
        public float[] FanSpeeds { get; set; }
    }
}
