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

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands.Validation.BusinessValidation.ValidationRules
{
    public class ChargeMustExistRule : IValidationRuleWithExtendedData
    {
        private readonly Charge? _existingCharge;
        private readonly ChargeLinkDto _chargeLinkDto;

        public ChargeMustExistRule(Charge? existingCharge, ChargeLinkDto chargeLinkDto)
        {
            _existingCharge = existingCharge;
            _chargeLinkDto = chargeLinkDto;
        }

        public ValidationRuleIdentifier ValidationRuleIdentifier => ValidationRuleIdentifier.ChargeDoesNotExist;

        public bool IsValid => _existingCharge is not null;

        /// <summary>
        /// This validation rule validates each ChargeLink in a list of ChargeLink(s). This property will
        /// tell which ChargeLink triggered the rule. The ChargeLink is identified by SenderProvidedChargeId.
        /// </summary>
        public string TriggeredBy => _chargeLinkDto.SenderProvidedChargeId;
    }
}
