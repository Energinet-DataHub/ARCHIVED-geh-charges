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

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
{
    public class SenderIsMandatoryTypeValidationRule : IValidationRule
    {
        private readonly ChargeCommand _chargeCommand;

        public SenderIsMandatoryTypeValidationRule(ChargeCommand chargeCommand)
        {
            _chargeCommand = chargeCommand;
        }

        public bool IsValid => MarketParticipantMrIdValidator.IsValid(_chargeCommand.Document.Sender.Id);

        public ValidationError? ValidationError
        {
            get
            {
                if (IsValid) return null;

                return
                    new(
                        ValidationRuleIdentifier.SenderIsMandatoryTypeValidation,
                        new ValidationErrorMessageParameter(
                            _chargeCommand.Document.Id,
                            ValidationErrorMessageParameterType.DocumentId));
            }
        }
    }
}
