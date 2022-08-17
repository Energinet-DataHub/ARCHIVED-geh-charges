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
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.Dtos.Validation.InputValidation;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands.Validation.InputValidation.Factories
{
    public class ChargePriceOperationInputValidationRulesFactory : IInputValidationRulesFactory<ChargePriceOperationDto>
    {
        public IValidationRuleSet CreateRules(ChargePriceOperationDto operation, DocumentDto document)
        {
            ArgumentNullException.ThrowIfNull(operation);
            var rules = GetRulesForOperation(operation, document);
            return ValidationRuleSet.FromRules(rules.ToList());
        }

        private IEnumerable<IValidationRuleContainer> GetRulesForOperation(ChargePriceOperationDto operation, DocumentDto document)
        {
            var rules = new List<IValidationRuleContainer>
            {
                CreateRuleContainer(new MaximumPriceRule(operation), operation.OperationId),
                CreateRuleContainer(new ChargeIdLengthValidationRule(operation), operation.OperationId),
                CreateRuleContainer(new ChargeIdRequiredValidationRule(operation), operation.OperationId),
                CreateRuleContainer(new ChargeOperationIdRequiredRule(operation), operation.OperationId),
                CreateRuleContainer(new ChargeOperationIdLengthValidationRule(operation), operation.OperationId),
                CreateRuleContainer(new ChargeOwnerIsRequiredValidationRule(operation), operation.OperationId),
                CreateRuleContainer(new ChargeTypeIsKnownValidationRule(operation), operation.OperationId),
                CreateRuleContainer(new StartDateTimeRequiredValidationRule(operation), operation.OperationId),
                CreateRuleContainer(new ChargeOwnerTextLengthRule(operation), operation.OperationId),
                CreateRuleContainer(new ChargeOwnerMustMatchSenderRule(document.Sender.MarketParticipantId, operation.ChargeOwner), operation.OperationId),
            };

            return rules;
        }

        private static IValidationRuleContainer CreateRuleContainer(
            IValidationRule validationRule, string operationId)
        {
            return new OperationValidationRuleContainer(validationRule, operationId);
        }
    }
}
