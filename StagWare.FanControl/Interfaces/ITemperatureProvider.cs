using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StagWare.FanControl
{
    public interface ITemperatureProvider
    {
        double GetTemperature();
    }
}
