using StagWare.FanControl.Configurations.Validation.Rules;

namespace StagWare.FanControl.Configurations.Validation
{
    public class FanControlConfigValidator : Validator<FanControlConfigV2>
    {
        public FanControlConfigValidator()
        {
            Rules.AddRange(new IValidationRule<FanControlConfigV2>[]
            {
                new MaxFanSpeedThresholdRequired(),
                new NoDuplicateTemperatureUpThresholds(),
                new UpThresholdsMustBeLowerThanCriticalTemperature(),
                new ZeroFanSpeedThresholdRequired(),
            });
        }
    }
}
