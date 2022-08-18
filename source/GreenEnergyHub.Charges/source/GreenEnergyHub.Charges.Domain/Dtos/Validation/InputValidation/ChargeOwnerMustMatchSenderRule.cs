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

namespace GreenEnergyHub.Charges.Domain.Dtos.Validation.InputValidation
{
    public class ChargeOwnerMustMatchSenderRule : IValidationRule
    {
        private readonly string _sender;
        private readonly string _chargeOwner;

        public ChargeOwnerMustMatchSenderRule(string sender, string chargeOwner)
        {
            _sender = sender;
            _chargeOwner = chargeOwner;
        }

        public ValidationRuleIdentifier ValidationRuleIdentifier =>
            ValidationRuleIdentifier.ChargeOwnerMustMatchSender;

        public bool IsValid => _chargeOwner == _sender;
    }
}
