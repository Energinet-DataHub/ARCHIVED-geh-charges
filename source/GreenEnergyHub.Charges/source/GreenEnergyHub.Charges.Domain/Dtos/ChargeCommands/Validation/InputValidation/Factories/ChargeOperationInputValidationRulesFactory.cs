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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.Factories
{
    public class ChargeOperationInputValidationRulesFactory : IInputValidationRulesFactory<ChargeOperationDto>
    {
        public IValidationRuleSet CreateRules(ChargeOperationDto operation)
        {
            ArgumentNullException.ThrowIfNull(operation);
            var rules = GetRulesForOperation(operation).ToList();
            return ValidationRuleSet.FromRules(rules);
        }

        private static IList<IValidationError> GetRulesForOperation(ChargeOperationDto chargeOperationDto)
        {
            var rules = new List<IValidationError>
            {
                new ValidationError(
                    new ChargeDescriptionHasMaximumLengthRule(chargeOperationDto), chargeOperationDto.Id),
                new ValidationError(new ChargeIdLengthValidationRule(chargeOperationDto), chargeOperationDto.Id),
                new ValidationError(new ChargeIdRequiredValidationRule(chargeOperationDto), chargeOperationDto.Id),
                new ValidationError(new ChargeNameHasMaximumLengthRule(chargeOperationDto), chargeOperationDto.Id),
                new ValidationError(new ChargeOperationIdRequiredRule(chargeOperationDto), chargeOperationDto.Id),
                new ValidationError(new ChargeOwnerIsRequiredValidationRule(chargeOperationDto), chargeOperationDto.Id),
                new ValidationError(
                    new ChargePriceMaximumDigitsAndDecimalsRule(chargeOperationDto), chargeOperationDto.Id),
                new ValidationError(new ChargeTypeIsKnownValidationRule(chargeOperationDto), chargeOperationDto.Id),
                new ValidationError(new ChargeTypeTariffPriceCountRule(chargeOperationDto), chargeOperationDto.Id),
                new ValidationError(new MaximumPriceRule(chargeOperationDto), chargeOperationDto.Id),
                new ValidationError(new ResolutionFeeValidationRule(chargeOperationDto), chargeOperationDto.Id),
                new ValidationError(
                    new ResolutionSubscriptionValidationRule(chargeOperationDto), chargeOperationDto.Id),
                new ValidationError(new ResolutionTariffValidationRule(chargeOperationDto), chargeOperationDto.Id),
                new ValidationError(new StartDateTimeRequiredValidationRule(chargeOperationDto), chargeOperationDto.Id),
                new ValidationError(new VatClassificationValidationRule(chargeOperationDto), chargeOperationDto.Id),
                new ValidationError(
                    new TransparentInvoicingIsNotAllowedForFeeValidationRule(chargeOperationDto), chargeOperationDto.Id),
            };

            return rules;
        }
    }
}
