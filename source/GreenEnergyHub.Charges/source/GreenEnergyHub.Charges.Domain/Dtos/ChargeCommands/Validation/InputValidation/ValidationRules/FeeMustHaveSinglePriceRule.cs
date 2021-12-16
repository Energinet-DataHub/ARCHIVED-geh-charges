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
    public class FeeMustHaveSinglePriceRule : IValidationRule
    {
        private const int PricePointsRequired = 1;
        private readonly ChargeCommand _chargeCommand;

        public FeeMustHaveSinglePriceRule(ChargeCommand chargeCommand)
        {
            _chargeCommand = chargeCommand;
        }

        public bool IsValid
        {
            get
            {
                if (_chargeCommand.ChargeOperation.Type is ChargeType.Fee)
                {
                    return _chargeCommand.ChargeOperation.Points.Count == PricePointsRequired;
                }

                return true;
            }
        }

        public ValidationRuleIdentifier ValidationRuleIdentifier => ValidationRuleIdentifier.FeeMustHaveSinglePrice;
    }
}
