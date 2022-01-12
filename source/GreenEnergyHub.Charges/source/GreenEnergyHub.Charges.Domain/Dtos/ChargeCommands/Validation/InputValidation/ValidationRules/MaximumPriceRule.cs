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

using System.Linq;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
{
    public class MaximumPriceRule : IValidationRule
    {
        private const int PriceUpperBound = 1000000;
        private readonly ChargeCommand _chargeCommand;

        public MaximumPriceRule(ChargeCommand chargeCommand)
        {
            _chargeCommand = chargeCommand;
        }

        public ValidationRuleIdentifier ValidationRuleIdentifier => ValidationRuleIdentifier.MaximumPrice;

        public bool IsValid => _chargeCommand.ChargeOperation.Points.All(point => point.Price < PriceUpperBound);
    }
}
