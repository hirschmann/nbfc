using System.Collections.Generic;

namespace StagWare.FanControl.Configurations.Validation
{
    public class Validator<T>
    {
        public List<IValidationRule<T>> Rules { get; private set; } = new List<IValidationRule<T>>();

        public ValidationSummary<T> Validate(T item, bool breakOnFailure = true, bool failOnWarnings = false)
        {
            var summary = new ValidationSummary<T>();
            summary.Success = true;

            foreach (var rule in Rules)
            {
                switch (rule.Validate(item))
                {
                    case ValidationResult.Success:
                        summary.PassedRules.Add(rule);
                        break;

                    case ValidationResult.Warning:
                        summary.WarningRules.Add(rule);
                        if (failOnWarnings) summary.Success = false;
                        break;

                    case ValidationResult.Error:
                        summary.FailedRules.Add(rule);
                        summary.Success = false;
                        break;
                }

                if (breakOnFailure && !summary.Success)
                {
                    break;
                }
            }

            return summary;
        }
    }
}
