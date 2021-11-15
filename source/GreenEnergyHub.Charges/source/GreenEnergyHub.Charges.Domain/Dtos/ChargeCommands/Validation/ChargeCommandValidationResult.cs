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

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation
{
    public class ChargeCommandValidationResult
    {
        private IEnumerable<IValidationRule> _invalidRules = new List<IValidationRule>();

        private ChargeCommandValidationResult()
            : this(Array.Empty<IValidationRule>())
        {
        }

        private ChargeCommandValidationResult(IList<IValidationRule> invalidRules)
        {
            InvalidRules = invalidRules;
        }

        public IEnumerable<IValidationRule> InvalidRules
        {
            get => _invalidRules;
            private set
            {
                if (value.Any(r => r.IsValid))
                {
                    throw new ArgumentException("All validation rules must be valid", nameof(InvalidRules));
                }

                _invalidRules = value;
            }
        }

        public bool IsFailed => InvalidRules.Select(r => !r.IsValid).Any();

        public static ChargeCommandValidationResult CreateSuccess()
        {
            return new ChargeCommandValidationResult();
        }

        public static ChargeCommandValidationResult CreateFailure(IList<IValidationRule> invalidRules)
        {
            return new ChargeCommandValidationResult(invalidRules.ToArray());
        }
    }
}
