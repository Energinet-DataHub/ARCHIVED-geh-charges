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
using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules
{
    public class ChangingTariffTaxValueNotAllowedRule : IValidationRule
    {
        private readonly ChargeDto _chargeDto;
        private readonly Charge _charge;

        public ChangingTariffTaxValueNotAllowedRule(ChargeDto chargeDto, Charge charge)
        {
            _chargeDto = chargeDto;
            _charge = charge;
        }

        public ValidationRuleIdentifier ValidationRuleIdentifier =>
            ValidationRuleIdentifier.ChangingTariffTaxValueNotAllowed;

        public bool IsValid => _chargeDto.ChargeOperation.TaxIndicator == _charge.TaxIndicator;
    }
}
