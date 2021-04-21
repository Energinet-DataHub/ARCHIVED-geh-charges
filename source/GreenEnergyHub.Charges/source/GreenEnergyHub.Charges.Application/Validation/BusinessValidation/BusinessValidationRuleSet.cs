using System.Collections.Generic;
using System.Linq;

namespace GreenEnergyHub.Charges.Application.Validation.BusinessValidation
{
    public class BusinessValidationRuleSet : IBusinessValidationRuleSet
    {
        private readonly List<IBusinessValidationRule> _rules;

        private BusinessValidationRuleSet(List<IBusinessValidationRule> rules)
        {
            _rules = rules;
        }

        public static IBusinessValidationRuleSet FromRules(List<IBusinessValidationRule> rules)
        {
            return new BusinessValidationRuleSet(rules);
        }

        public ChargeCommandValidationResult Validate()
        {
            var invalidRules = _rules.Where(r => !r.IsValid);
            if (invalidRules.Any())
            {
                return ChargeCommandValidationResult.CreateFailure(invalidRules);
            }

            return ChargeCommandValidationResult.CreateSuccess();
        }
    }
}
