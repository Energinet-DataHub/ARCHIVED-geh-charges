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

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands.Validation.InputValidation.ValidationRules
{
    public class ResolutionValidationRule : IValidationRule
    {
        private readonly ChargePriceOperationDto _chargePriceOperationDto;
        private readonly ChargeType _chargeType;
        private readonly List<Resolution> _allowedResolutions;
        private readonly ValidationRuleIdentifier _validationRuleIdentifier;

        public ResolutionValidationRule(
            ChargePriceOperationDto chargePriceOperationDto,
            ChargeType chargeType,
            List<Resolution> allowedResolutions,
            ValidationRuleIdentifier validationRuleIdentifier)
        {
            _chargePriceOperationDto = chargePriceOperationDto;
            _chargeType = chargeType;
            _allowedResolutions = allowedResolutions;
            _validationRuleIdentifier = validationRuleIdentifier;
        }

        public virtual bool IsValid =>
            _chargePriceOperationDto.ChargeType != _chargeType ||
            _allowedResolutions.Contains(_chargePriceOperationDto.Resolution)
            || _chargePriceOperationDto.Resolution == Resolution.Unknown;

        public ValidationRuleIdentifier ValidationRuleIdentifier => _validationRuleIdentifier;
    }
}
