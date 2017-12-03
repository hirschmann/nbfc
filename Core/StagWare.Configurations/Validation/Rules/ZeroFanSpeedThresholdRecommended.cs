using System.Linq;

namespace StagWare.FanControl.Configurations.Validation.Rules
{
    public class ZeroFanSpeedThresholdRecommended : IValidationRule<FanControlConfigV2>
    {
        public string Description => "Each fan should have a threshold with a fan speed of 0, otherwise the fan never stops";

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

                if (!cfg.TemperatureThresholds.Any(x => x.FanSpeed == 0))
                {
                    string fanName = "Fan #" + i;

                    if (!string.IsNullOrWhiteSpace(cfg.FanDisplayName))
                    {
                        fanName += $" ({cfg.FanDisplayName})";
                    }

                    v.Result = ValidationResult.Warning;                    
                    v.Reason = "There is no threshold with a fan speed of 0: " + fanName;
                    return v;
                }
            }

            return v;
        }
    }
}
