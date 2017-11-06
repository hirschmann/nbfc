using System.Linq;

namespace StagWare.FanControl.Configurations.Validation.Rules
{
    public class ZeroFanSpeedThresholdRequired : IValidationRule<FanControlConfigV2>
    {
        public string Description => "Each fan must have a threshold with a fan speed of 0";

        public ValidationResult Validate(FanControlConfigV2 item)
        {
            if (item.FanConfigurations == null)
            {
                return ValidationResult.Success;
            }

            foreach (FanConfiguration cfg in item.FanConfigurations)
            {
                // ignore empty thresholds, because in this case the defaults will be applied
                if(cfg.TemperatureThresholds == null || cfg.TemperatureThresholds.Count == 0)
                {
                    continue;
                }

                if(!cfg.TemperatureThresholds.Any(x => x.FanSpeed == 0))
                {
                    return ValidationResult.Error;
                }
            }

            return ValidationResult.Success;
        }
    }
}
