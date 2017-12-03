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
                var validation = rule.Validate(item);

                switch (validation.Result)
                {
                    case ValidationResult.Success:
                        summary.Passed.Add(validation);
                        break;

                    case ValidationResult.Warning:
                        summary.Warnings.Add(validation);
                        if (failOnWarnings) summary.Success = false;
                        break;

                    case ValidationResult.Error:
                        summary.Failed.Add(validation);
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
