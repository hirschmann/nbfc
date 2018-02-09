using System.Linq;

namespace StagWare.FanControl.Configurations.Validation.Rules
{
    public class UpThresholdMayNotBeLowerThanDownThreshold : IValidationRule<FanControlConfigV2>
    {
        public string Description => "Each threshold's, up-threshold may not be lower than its corresponding down-threshold";

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

            foreach (var t in item.FanConfigurations.SelectMany(x => x.TemperatureThresholds))
            {
                if (t.UpThreshold < t.DownThreshold)
                {
                    v.Result = ValidationResult.Error;
                    v.Reason = $"At least one up-threshold ({t.UpThreshold}) is less than its corresponding down-threshold ({t.DownThreshold})";
                    return v;
                }
            }

            return v;
        }
    }
}
