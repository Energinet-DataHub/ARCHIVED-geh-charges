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

namespace GreenEnergyHub.Charges.Domain.Dtos.Validation
{
    public class ValidationRuleSet : IValidationRuleSet
    {
        private readonly List<IValidationError> _rules;

        private ValidationRuleSet(List<IValidationError> rules)
        {
            _rules = rules;
        }

        public static IValidationRuleSet FromRules(List<IValidationError> rules)
        {
            return new ValidationRuleSet(rules);
        }

        public IReadOnlyCollection<IValidationError> GetRules() => _rules.AsReadOnly();

        public ValidationResult Validate()
        {
            var invalidRules = _rules.Where(r => !r.ValidationRule.IsValid).ToList();
            return invalidRules.Any() ?
                ValidationResult.CreateFailure(invalidRules) :
                ValidationResult.CreateSuccess();
        }
    }
}
