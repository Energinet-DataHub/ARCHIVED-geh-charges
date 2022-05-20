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
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
{
    public abstract class ResolutionValidationRule : IValidationRule
    {
        private readonly ChargeOperationDto _chargeOperationDto;
        private readonly ChargeType _chargeType;
        private readonly List<Resolution> _allowedResolutions;
        private readonly ValidationRuleIdentifier _validationRuleIdentifier;

        protected ResolutionValidationRule(
            ChargeOperationDto chargeOperationDto,
            ChargeType chargeType,
            List<Resolution> allowedResolutions,
            ValidationRuleIdentifier validationRuleIdentifier)
        {
            _chargeOperationDto = chargeOperationDto;
            _chargeType = chargeType;
            _allowedResolutions = allowedResolutions;
            _validationRuleIdentifier = validationRuleIdentifier;
        }

        public virtual bool IsValid =>
            _chargeOperationDto.Type != _chargeType ||
            _allowedResolutions.Contains(_chargeOperationDto.Resolution);

        public ValidationRuleIdentifier ValidationRuleIdentifier => _validationRuleIdentifier;
    }
}
