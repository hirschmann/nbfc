using System;
using System.Linq;

namespace StagWare.FanControl.Configurations.Validation
{
    public class FanControlConfigValidator : Validator<FanControlConfigV2>
    {
        public FanControlConfigValidator()
        {
            var rules = typeof(IValidationRule<FanControlConfigV2>)?.Assembly
                .GetTypes()
                .Where(x => x.IsClass && typeof(IValidationRule<FanControlConfigV2>).IsAssignableFrom(x))
                .Select(x => (IValidationRule <FanControlConfigV2>)Activator.CreateInstance(x));

            Rules.AddRange(rules);
        }
    }
}
