using System.Collections.Generic;
using System.Linq;

namespace StagWare.FanControl.Configurations.Validation
{
    public class ValidationSummary<T>
    {
        public bool Success { get; set; }
        public List<Validation> Passed { get; set; } = new List<Validation>();
        public List<Validation> Warnings { get; set; } = new List<Validation>();
        public List<Validation> Failed { get; set; } = new List<Validation>();
    }
}