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
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands.Validation.InputValidation.ValidationRules
{
    public class NumberOfPointsMatchTimeIntervalAndResolutionRule : IValidationRule
    {
        private readonly Instant _startTime;
        private readonly Instant _endTime;
        private readonly int _actualPointCount;
        private double _expectedPointCount;

        public NumberOfPointsMatchTimeIntervalAndResolutionRule(ChargePriceOperationDto chargePriceOperationDto)
        {
            _startTime = chargePriceOperationDto.PointsStartInterval;
            _endTime = chargePriceOperationDto.PointsEndInterval;
            SetExpectedPointCount(chargePriceOperationDto.Resolution);
            _actualPointCount = chargePriceOperationDto.Points.Count;
        }

        private void SetExpectedPointCount(Resolution resolution)
        {
            var interval = new Interval(_startTime, _endTime);

            _expectedPointCount = resolution switch
            {
                Resolution.Unknown => 0,
                Resolution.PT15M => interval.Duration.TotalMinutes / 15,
                Resolution.PT1H => interval.Duration.TotalHours,
                Resolution.P1D => interval.Duration.TotalDays,
                Resolution.P1M => CalculateMonthsSpannedByInterval(),
                _ => throw new ArgumentOutOfRangeException(nameof(resolution)),
            };
        }

        public ValidationRuleIdentifier ValidationRuleIdentifier =>
            ValidationRuleIdentifier.NumberOfPointsMatchTimeIntervalAndResolution;

        public bool IsValid =>
            Convert.ToInt32(Math.Round(_expectedPointCount, MidpointRounding.AwayFromZero)) == _actualPointCount;

        private int CalculateMonthsSpannedByInterval()
        {
            // https://stackoverflow.com/a/4639057
            var months = ((_endTime.InUtc().Year - _startTime.InUtc().Year) * 12) + _endTime.InUtc().Month -
                         _startTime.InUtc().Month;

            // The 'months' calculation above does not consider the interval to contain an irregular price series;
            // where the startTime is within the same month as the endTime. Thus returning 0 spanned month.
            // To counter for this, below code checks if the interval startTime contains an irregular price series,
            // and if so, adds 1 to 'months', as an irregular price series spans a single month.
            // Below code also works when the interval contains several points and is also an irregular price series.
            if (IsIrregularPriceSeries(_startTime))
            {
                months++;
            }

            return months;
        }

        private static bool IsIrregularPriceSeries(Instant startDateTime)
        {
            var startDateTimeInUtc = startDateTime.InUtc();

            return startDateTimeInUtc.Day !=
                   startDateTimeInUtc.Calendar.GetDaysInMonth(startDateTimeInUtc.Year, startDateTimeInUtc.Month);
        }
    }
}
