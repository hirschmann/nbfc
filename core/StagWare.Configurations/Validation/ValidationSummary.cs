using System.Collections.Generic;

namespace StagWare.FanControl.Configurations.Validation
{
    public class ValidationSummary<T>
    {
        public bool Success { get; set; }
        public List<IValidationRule<T>> PassedRules { get; set; } = new List<IValidationRule<T>>();
        public List<IValidationRule<T>> WarningRules { get; set; } = new List<IValidationRule<T>>();
        public List<IValidationRule<T>> FailedRules { get; set; } = new List<IValidationRule<T>>();
    }
}