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
using GreenEnergyHub.Charges.Domain.Charges;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
{
    public class ChargePriceMaximumDigitsAndDecimalsRule : IValidationRule
    {
        private const int MaximumDigitsInPrice = 8;
        private const int MaximumDecimalsInPrice = 6;
        private readonly ChargeCommand _chargeCommand;

        public ChargePriceMaximumDigitsAndDecimalsRule(ChargeCommand chargeCommand)
        {
            _chargeCommand = chargeCommand;
        }

        public bool IsValid
        {
            get
            {
                foreach (Point point in _chargeCommand.ChargeOperation.Points)
                {
                    if (GetNumberOfDigits(point.Price) > MaximumDigitsInPrice)
                    {
                        return false;
                    }

                    if (GetNumberOfDecimals(point.Price) > MaximumDecimalsInPrice)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        // https://stackoverflow.com/a/21546928
        private static int GetNumberOfDigits(decimal d)
        {
            var abs = Math.Abs(d);

            return abs < 1 ? 0 : (int)(Math.Log10(decimal.ToDouble(abs)) + 1);
        }

        // https://stackoverflow.com/a/13477756
        private int GetNumberOfDecimals(decimal d, int i = 0)
        {
            var multiplied = (decimal)((double)d * Math.Pow(10, i));
            if (Math.Round(multiplied) == multiplied)
            {
                return i;
            }

            return GetNumberOfDecimals(d, i + 1);
        }

        public ValidationError ValidationError { get; } = new(
            ValidationRuleIdentifier.ChargePriceMaximumDigitsAndDecimals,
            ValidationErrorMessageParameterType.EnergyPrice,
            ValidationErrorMessageParameterType.PartyChargeTypeId);
    }
}
