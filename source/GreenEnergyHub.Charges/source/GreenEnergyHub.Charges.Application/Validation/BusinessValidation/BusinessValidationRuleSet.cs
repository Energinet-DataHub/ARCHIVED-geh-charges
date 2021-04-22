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
            var invalidRules = _rules.Where(r => !r.IsValid).ToList();

            if (invalidRules.Any())
            {
                return ChargeCommandValidationResult.CreateFailure(invalidRules);
            }

            return ChargeCommandValidationResult.CreateSuccess();
        }
    }
}
