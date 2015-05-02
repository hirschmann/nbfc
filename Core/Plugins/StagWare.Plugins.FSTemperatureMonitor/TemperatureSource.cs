using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StagWare.Plugins
{
    public class TemperatureSource
    {
        public TemperatureSource()
        {
        }

        public TemperatureSource(string filePath, double multiplier)
        {
            this.FilePath = filePath;
            this.Multiplier = multiplier;
        }

        public string FilePath { get; set; }
        public double Multiplier { get; set; }
    }
}
