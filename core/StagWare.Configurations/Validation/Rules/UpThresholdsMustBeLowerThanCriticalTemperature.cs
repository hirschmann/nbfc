using System.Linq;

namespace StagWare.FanControl.Configurations.Validation.Rules
{
    public class UpThresholdsMustBeLowerThanCriticalTemperature : IValidationRule<FanControlConfigV2>
    {
        public string Description => "All up-threshold values must be lower than the critical temperature";

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

            foreach (var cfg in item.FanConfigurations)
            {
                var threshold = cfg.TemperatureThresholds?.FirstOrDefault(x => x.UpThreshold >= item.CriticalTemperature);

                if (threshold != null)
                {
                    v.Result = ValidationResult.Error;
                    v.Reason = "At least one up-threshold is higher than the critical temperature: " + threshold.UpThreshold;
                    return v;
                }

                threshold = cfg.TemperatureThresholds?.FirstOrDefault(x => x.UpThreshold >= (item.CriticalTemperature - 5));

                if (threshold != null)
                {
                    v.Result = ValidationResult.Warning;
                    v.Reason = "At least one up-threshold is less than 5 degrees below the critical temperature: " + threshold.UpThreshold;
                    return v;
                }
            }

            return v;
        }
    }
}
