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

using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation
{
    public class ChargeCommandValidator : IChargeCommandValidator
    {
        private readonly IBusinessValidator<ChargeCommand> _businessValidator;
        private readonly IChargeCommandInputValidator _chargeCommandInputValidator;

        public ChargeCommandValidator(
            IChargeCommandInputValidator chargeCommandInputValidator,
            IBusinessValidator<ChargeCommand> businessValidator)
        {
            _chargeCommandInputValidator = chargeCommandInputValidator;
            _businessValidator = businessValidator;
        }

        public async Task<ValidationResult> ValidateAsync(ChargeCommand command)
        {
            var inputValidationResult = _chargeCommandInputValidator.Validate(command);
            if (inputValidationResult.IsFailed) return inputValidationResult;

            var businessValidationResult =
                await _businessValidator.ValidateAsync(command).ConfigureAwait(false);
            return businessValidationResult;
        }
    }
}
