using System.Linq;

namespace StagWare.FanControl.Configurations.Validation.Rules
{
    public class UpThresholdsMustBeLowerThanCriticalTemperature : IValidationRule<FanControlConfigV2>
    {
        public string Description => "All up-threshold values must be lower than the critical temperature";

        public ValidationResult Validate(FanControlConfigV2 item)
        {
            if(item.FanConfigurations == null)
            {
                return ValidationResult.Success;
            }

            foreach(var cfg in item.FanConfigurations)
            {
                if(cfg.TemperatureThresholds?.Any(x=> x.UpThreshold >= item.CriticalTemperature) == true)
                {
                    return ValidationResult.Error;
                }
            }

            return ValidationResult.Success;
        }
    }
}
