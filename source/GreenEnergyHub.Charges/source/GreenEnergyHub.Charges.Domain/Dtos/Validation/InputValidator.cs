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

using GreenEnergyHub.Charges.Domain.Dtos.Messages.Command;

namespace GreenEnergyHub.Charges.Domain.Dtos.Validation
{
    public class InputValidator<TCommand, TOperation> : IInputValidator<TCommand, TOperation>
        where TCommand : CommandBase
        where TOperation : OperationBase
    {
        private readonly IInputValidationRulesFactory<TCommand, TOperation> _inputValidationRulesFactory;

        public InputValidator(IInputValidationRulesFactory<TCommand, TOperation> inputValidationRulesFactory)
        {
            _inputValidationRulesFactory = inputValidationRulesFactory;
        }

        public ValidationResult Validate(TCommand command)
        {
            IValidationRuleSet ruleSet = _inputValidationRulesFactory.CreateRulesForCommand(command);
            return ruleSet.Validate();
        }

        public ValidationResult Validate(TOperation operation)
        {
            IValidationRuleSet ruleSet = _inputValidationRulesFactory.CreateRulesForOperation(operation);
            return ruleSet.Validate();
        }
    }
}
