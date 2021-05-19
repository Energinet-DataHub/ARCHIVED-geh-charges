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

using System;
using System.Linq;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application.Validation.InputValidation.ValidationRules
{
    public class MaximumPriceRule : IValidationRule
    {
        private const int MaximumDigitsInPrice = 6;
        private readonly ChargeCommand _chargeCommand;

        public MaximumPriceRule(ChargeCommand chargeCommand)
        {
            _chargeCommand = chargeCommand;
        }

        public bool IsValid =>
            _chargeCommand.ChargeOperation.Points.All(point => GetNumberOfDigits(point.Price) <= MaximumDigitsInPrice);

        // https://stackoverflow.com/a/21546928
        private static int GetNumberOfDigits(decimal d)
        {
            var abs = Math.Abs(d);

            return abs < 1 ? 0 : (int)(Math.Log10(decimal.ToDouble(abs)) + 1);
        }

        public ValidationRuleIdentifier ValidationRuleIdentifier => ValidationRuleIdentifier.MaximumPriceRule;
    }
}
