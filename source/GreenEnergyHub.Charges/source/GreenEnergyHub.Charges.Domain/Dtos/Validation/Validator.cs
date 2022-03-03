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

using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.Dtos.Messages.Command;

namespace GreenEnergyHub.Charges.Domain.Dtos.Validation
{
    public class Validator<TCommand, TOperation> : IValidator<TCommand, TOperation>
        where TCommand : CommandBase
        where TOperation : OperationBase
    {
        private readonly IBusinessValidator<TCommand, TOperation> _businessValidator;
        private readonly IInputValidator<TCommand, TOperation> _inputValidator;

        public Validator(
            IInputValidator<TCommand, TOperation> inputValidator,
            IBusinessValidator<TCommand, TOperation> businessValidator)
        {
            _inputValidator = inputValidator;
            _businessValidator = businessValidator;
        }

        public ValidationResult InputValidate(TCommand command)
        {
            var inputValidationResult = _inputValidator.Validate(command);
            return inputValidationResult;
        }

        public async Task<ValidationResult> BusinessValidateAsync(TCommand command)
        {
            var businessValidationResult = await _businessValidator.ValidateAsync(command).ConfigureAwait(false);
            return businessValidationResult;
        }

        public ValidationResult InputValidate(TOperation operation)
        {
            var inputValidationResult = _inputValidator.Validate(operation);
            return inputValidationResult;
        }

        public async Task<ValidationResult> BusinessValidateAsync(TOperation operation)
        {
            var businessValidationResult = await _businessValidator.ValidateAsync(operation).ConfigureAwait(false);
            return businessValidationResult;
        }
    }
}
