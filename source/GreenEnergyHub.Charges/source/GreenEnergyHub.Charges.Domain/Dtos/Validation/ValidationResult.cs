﻿// Copyright 2020 Energinet DataHub A/S
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

namespace GreenEnergyHub.Charges.Domain.Dtos.Validation
{
    public class ValidationResult
    {
        private readonly IList<IValidationRuleContainer> _invalidRules = new List<IValidationRuleContainer>();

        private ValidationResult()
            : this(Array.Empty<IValidationRuleContainer>())
        {
        }

        private ValidationResult(IList<IValidationRuleContainer> invalidRules)
        {
            InvalidRules = invalidRules;
        }

        public IList<IValidationRuleContainer> InvalidRules
        {
            get => _invalidRules;
            private init
            {
                if (value.Any(r => r.ValidationRule.IsValid))
                {
                    throw new ArgumentException("All validation rules must be invalid", nameof(InvalidRules));
                }

                _invalidRules = value;
            }
        }

        public bool IsFailed => InvalidRules.Select(r => !r.ValidationRule.IsValid).Any();

        public static ValidationResult CreateSuccess()
        {
            return new ValidationResult();
        }

        public static ValidationResult CreateFailure(IList<IValidationRuleContainer> invalidRules)
        {
            return new ValidationResult(invalidRules.ToArray());
        }
    }
}
