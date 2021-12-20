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

using GreenEnergyHub.Charges.Domain.Charges;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
{
    public class ResolutionTariffValidationRule : IValidationRule
    {
        private readonly ChargeCommand _chargeCommand;

        public ResolutionTariffValidationRule(ChargeCommand chargeCommand)
        {
            _chargeCommand = chargeCommand;
        }

        public bool IsValid
        {
            get
            {
                if (_chargeCommand.ChargeOperation.Type == ChargeType.Tariff)
                {
                    return _chargeCommand.ChargeOperation.Resolution is
                        Resolution.P1D or
                        Resolution.PT1H or
                        Resolution.PT15M;
                }

                return true;
            }
        }

        public ValidationError ValidationError => new(
                ValidationRuleIdentifier.ResolutionTariffValidation,
                new ValidationErrorMessageParameter(
                    _chargeCommand.ChargeOperation.Resolution.ToString(),
                    ValidationErrorMessageParameterType.ResolutionDuration),
                new ValidationErrorMessageParameter(
                    _chargeCommand.ChargeOperation.Type.ToString(), // TODO: which one?
                    ValidationErrorMessageParameterType.PartyChargeTypeId),
                new ValidationErrorMessageParameter(
                    _chargeCommand.ChargeOperation.Type.ToString(),
                    ValidationErrorMessageParameterType.ChargeType));
    }
}
