using System;
using System.Collections.Generic;
using System.Linq;
using GreenEnergyHub.Messaging.Validation;

namespace GreenEnergyHub.Charges.Application.Validation
{
    // TODO: Consider allowing to add both valid and invalid rules
    public class ChargeCommandValidationResult
    {
        private readonly IBusinessValidationRule[] _invalidRules;

        private ChargeCommandValidationResult()
            : this(Array.Empty<IBusinessValidationRule>())
        {
        }

        private ChargeCommandValidationResult(IBusinessValidationRule[] invalidRules)
        {
            _invalidRules = invalidRules;
        }

        public bool IsFailed => _invalidRules.Select(r => !r.IsValid).Any();

        public static ChargeCommandValidationResult CreateSuccess()
        {
            return new ();
        }

        // TODO: Does this belong here? Consider using a ChargeCommandValidationResult.AddFailedRule()?
        public static ChargeCommandValidationResult CreateFailureFromRuleResultCollection(RuleResultCollection result)
        {
            return new (result.Select(r => new BusinessValidationRule(false)).ToArray<IBusinessValidationRule>());
        }

        public static ChargeCommandValidationResult CreateFailure(IEnumerable<IBusinessValidationRule> invalidRules)
        {
            return new (invalidRules.ToArray());
        }
    }
}
