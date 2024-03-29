﻿// Copyright 2020 Energinet DataHub A/S
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
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands.Validation.InputValidation.ValidationRules
{
    public class ChargePriceMaximumDigitsAndDecimalsRule : IValidationRuleWithExtendedData
    {
        private const int MaximumDigitsInPrice = 8;
        private const int MaximumDecimalsInPrice = 6;
        private readonly ChargePriceOperationDto _chargePriceOperationDto;

        public ChargePriceMaximumDigitsAndDecimalsRule(ChargePriceOperationDto chargePriceOperationDto)
        {
            _chargePriceOperationDto = chargePriceOperationDto;
        }

        public ValidationRuleIdentifier ValidationRuleIdentifier =>
            ValidationRuleIdentifier.ChargePriceMaximumDigitsAndDecimals;

        public bool IsValid => _chargePriceOperationDto.Points.All(PointIsValid);

        /// <summary>
        /// This validation rule validates each Price in a list of Point(s). This property
        /// will tell which Point triggered the rule. The Point is identified by Position.
        /// </summary>
        public string TriggeredBy => _chargePriceOperationDto
            .Points.GetPositionOfPoint(
                _chargePriceOperationDto.Points.First(point => !PointIsValid(point))).ToString();

        private bool PointIsValid(Point point)
        {
            if (GetNumberOfDigits(point.Price) > MaximumDigitsInPrice)
            {
                return false;
            }

            return GetNumberOfDecimals(point.Price) <= MaximumDecimalsInPrice;
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
    }
}
