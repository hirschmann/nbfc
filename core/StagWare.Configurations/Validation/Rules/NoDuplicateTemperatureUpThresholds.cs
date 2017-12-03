using System.Collections.Generic;

namespace StagWare.FanControl.Configurations.Validation.Rules
{
    public class NoDuplicateTemperatureUpThresholds : IValidationRule<FanControlConfigV2>
    {
        public string Description => "A fan's temperature thresholds must have unique up-thresholds";

        public Validation Validate(FanControlConfigV2 item)
        {
            var v = new Validation()
            {
                RuleDescription = this.Description,
                Result = ValidationResult.Success
            };

            if (item.FanConfigurations == null)
            {
                return v;
            }

            foreach (FanConfiguration cfg in item.FanConfigurations)
            {
                if (cfg.TemperatureThresholds == null)
                {
                    continue;
                }

                var lookup = new HashSet<int>();

                foreach (var threshold in cfg.TemperatureThresholds)
                {
                    if (lookup.Contains(threshold.UpThreshold))
                    {
                        v.Result = ValidationResult.Error;
                        v.Reason = "There is at least one duplicate up-threshold: " + threshold.UpThreshold;
                        return v;
                    }
                    else
                    {
                        lookup.Add(threshold.UpThreshold);
                    }
                }
            }

            return v;
        }
    }
}
