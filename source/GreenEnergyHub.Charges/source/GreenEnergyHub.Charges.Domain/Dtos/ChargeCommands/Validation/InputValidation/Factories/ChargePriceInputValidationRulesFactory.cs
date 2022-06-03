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
    public class ChargePriceInputValidationRulesFactory : IInputValidationRulesFactory<ChargePriceDto>
    {
        public IValidationRuleSet CreateRules(ChargePriceDto informationDto)
        {
            ArgumentNullException.ThrowIfNull(informationDto);
            var rules = GetRulesForOperation(informationDto).ToList();
            return ValidationRuleSet.FromRules(rules);
        }

        private static IEnumerable<IValidationRuleContainer> GetRulesForOperation(ChargePriceDto chargePriceDto)
        {
            var rules = new List<IValidationRuleContainer>
            {
                CreateRuleContainer(new ChargeIdLengthValidationRule(chargePriceDto), chargePriceDto),
                CreateRuleContainer(new ChargeIdRequiredValidationRule(chargePriceDto), chargePriceDto),
                CreateRuleContainer(new ChargeOperationIdRequiredRule(chargePriceDto), chargePriceDto),
                CreateRuleContainer(new ChargeOwnerIsRequiredValidationRule(chargePriceDto), chargePriceDto),
                CreateRuleContainer(new ChargeTypeIsKnownValidationRule(chargePriceDto), chargePriceDto),
                CreateRuleContainer(new StartDateTimeRequiredValidationRule(chargePriceDto), chargePriceDto),
            };

            rules.AddRange(CreateRulesForChargePrice(chargePriceDto));

            return rules;
        }

        private static IEnumerable<IValidationRuleContainer> CreateRulesForChargePrice(ChargePriceDto chargePriceDto)
        {
            return new List<IValidationRuleContainer>
            {
                CreateRuleContainer(new ChargePriceMaximumDigitsAndDecimalsRule(chargePriceDto), chargePriceDto),
                CreateRuleContainer(new MaximumPriceRule(chargePriceDto), chargePriceDto),
            };
        }

        private static IValidationRuleContainer CreateRuleContainer(
            IValidationRule validationRule, IChargeOperation chargePriceDto)
        {
            return new OperationValidationRuleContainer(validationRule, chargePriceDto.Id);
        }
    }
}
