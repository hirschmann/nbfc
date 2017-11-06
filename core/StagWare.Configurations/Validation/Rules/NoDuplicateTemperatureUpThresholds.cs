using System.Collections.Generic;

namespace StagWare.FanControl.Configurations.Validation.Rules
{
    public class NoDuplicateTemperatureUpThresholds : IValidationRule<FanControlConfigV2>
    {
        public string Description => "A fan's temperature thresholds must have unique up-thresholds";

        public ValidationResult Validate(FanControlConfigV2 item)
        {
            if (item.FanConfigurations == null)
            {
                return ValidationResult.Success;
            }

            foreach (FanConfiguration cfg in item.FanConfigurations)
            {
                if(cfg.TemperatureThresholds == null)
                {
                    continue;
                }

                var lookup = new HashSet<int>();

                foreach (var threshold in cfg.TemperatureThresholds)
                {
                    if (lookup.Contains(threshold.UpThreshold))
                    {
                        return ValidationResult.Error;
                    }
                    else
                    {
                        lookup.Add(threshold.UpThreshold);
                    }
                }
            }

            return ValidationResult.Success;
        }
    }
}
