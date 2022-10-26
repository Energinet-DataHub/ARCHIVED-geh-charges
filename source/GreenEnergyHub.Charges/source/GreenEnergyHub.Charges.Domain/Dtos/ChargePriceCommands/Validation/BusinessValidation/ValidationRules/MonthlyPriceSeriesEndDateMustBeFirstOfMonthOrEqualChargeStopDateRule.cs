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

using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands.Validation.BusinessValidation.ValidationRules
{
    public class MonthlyPriceSeriesEndDateMustBeFirstOfMonthOrEqualChargeStopDateRule : IValidationRule
    {
        private readonly Resolution _resolution;
        private readonly Instant _pointsIntervalEndDate;
        private readonly Instant _chargeStopDate;

        public MonthlyPriceSeriesEndDateMustBeFirstOfMonthOrEqualChargeStopDateRule(Resolution resolution, Instant pointsIntervalEndDate, Instant chargeStopDate)
        {
            _resolution = resolution;
            _pointsIntervalEndDate = pointsIntervalEndDate;
            _chargeStopDate = chargeStopDate;
        }

        public bool IsValid =>
            _resolution is not Resolution.P1M ||
            _pointsIntervalEndDate == _chargeStopDate ||
            IsFirstLocalDayInMonth(_pointsIntervalEndDate);

        public ValidationRuleIdentifier ValidationRuleIdentifier =>
            ValidationRuleIdentifier.MonthlyPriceSeriesEndDateMustBeFirstOfMonthOrEqualChargeStopDate;

        private static bool IsFirstLocalDayInMonth(Instant priceEndDate)
        {
            return priceEndDate != InstantExtensions.GetEndDefault() &&
                   priceEndDate.Plus(Duration.FromDays(1)).ToDateTimeUtc().Day == 1;
        }
    }
}
