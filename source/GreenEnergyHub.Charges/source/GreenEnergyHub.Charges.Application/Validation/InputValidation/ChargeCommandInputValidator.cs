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
using System.Diagnostics.CodeAnalysis;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application.Validation.InputValidation
{
    public class ChargeCommandInputValidator : IChargeCommandInputValidator
    {
        private readonly IInputValidationRulesFactory _inputValidationRulesFactory;

        public ChargeCommandInputValidator(IInputValidationRulesFactory inputValidationRulesFactory)
        {
            _inputValidationRulesFactory = inputValidationRulesFactory;
        }

        public ChargeCommandValidationResult Validate([NotNull] ChargeCommand chargeCommand)
        {
            IValidationRuleSet ruleSet;
            switch (chargeCommand.ChargeOperation.OperationType)
            {
                case OperationType.Unknown:
                    throw new NotSupportedException(chargeCommand.ChargeOperation.OperationType.ToString());
                case OperationType.Addition:
                    ruleSet = _inputValidationRulesFactory.CreateRulesForChargeCreateCommand(chargeCommand);
                    return ruleSet.Validate();
                case OperationType.Deletion:
                    ruleSet = _inputValidationRulesFactory.CreateRulesForChargeStopCommand(chargeCommand);
                    return ruleSet.Validate();
                case OperationType.Change:
                    ruleSet = _inputValidationRulesFactory.CreateRulesForChargeUpdateCommand(chargeCommand);
                    return ruleSet.Validate();
                default:
                    throw new ArgumentOutOfRangeException(chargeCommand.ChargeOperation.OperationType.ToString());
            }
        }
    }
}
