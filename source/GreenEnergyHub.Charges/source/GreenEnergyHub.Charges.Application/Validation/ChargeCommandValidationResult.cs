// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using GreenEnergyHub.Charges.Application.Validation.BusinessValidation;
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
