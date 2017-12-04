using System.Linq;

namespace StagWare.FanControl.Configurations.Validation.Rules
{
    public class MaxFanSpeedThresholdRequired : IValidationRule<FanControlConfigV2>
    {
        public string Description => "Each fan must have a threshold with a fan speed of 100";

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

            int i = 0;

            foreach (FanConfiguration cfg in item.FanConfigurations)
            {
                i++;

                // ignore empty thresholds, because in this case the defaults will be applied
                if (cfg.TemperatureThresholds == null || cfg.TemperatureThresholds.Count == 0)
                {
                    continue;
                }

                if (!cfg.TemperatureThresholds.Any(x => x.FanSpeed == 100))
                {
                    string fanName = "Fan #" + i;

                    if (!string.IsNullOrWhiteSpace(cfg.FanDisplayName))
                    {
                        fanName += $" ({cfg.FanDisplayName})";
                    }

                    v.Result = ValidationResult.Error;
                    v.Reason = "There is no threshold with a fan speed of 100: " + fanName;
                    return v;
                }
            }

            return v;
        }
    }
}
