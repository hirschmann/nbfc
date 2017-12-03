namespace StagWare.FanControl.Configurations.Validation.Rules
{
    public class FanConfigurationsNotEmtpy : IValidationRule<FanControlConfigV2>
    {
        public string Description => "At least one fan configuration must exist";

        public Validation Validate(FanControlConfigV2 item)
        {
            var v = new Validation()
            {
                RuleDescription = this.Description,
                Result = ValidationResult.Success
            };

            if (item.FanConfigurations == null || item.FanConfigurations.Count == 0)
            {
                v.Result = ValidationResult.Error;
                return v;
            }
            else
            {
                return v;
            }
        }
    }
}
