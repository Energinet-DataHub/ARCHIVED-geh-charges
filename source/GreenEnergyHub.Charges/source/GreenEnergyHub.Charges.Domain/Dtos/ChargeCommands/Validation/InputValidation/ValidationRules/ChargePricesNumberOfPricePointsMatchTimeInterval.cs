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
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
{
    public class ChargePricesNumberOfPricePointsMatchTimeInterval : IValidationRule
    {
        private readonly Instant _startTime;
        private readonly Instant _endTime;
        private readonly Resolution _resolution;
        private readonly int _actualPointCount;
        private double _expectedPointCount;

        public ChargePricesNumberOfPricePointsMatchTimeInterval(ChargeOperationDto chargeOperationDto)
        {
            _resolution = chargeOperationDto.Resolution;
            _startTime = chargeOperationDto.PointsStartInterval.GetValueOrDefault();
            _endTime = chargeOperationDto.PointsEndInterval.GetValueOrDefault();
            SetExpectedPointCount();
            _actualPointCount = chargeOperationDto.Points.Count;
        }

        private void SetExpectedPointCount()
        {
            var interval = new Interval(_startTime, _endTime);

            _expectedPointCount = _resolution switch
            {
                Resolution.Unknown => throw new ArgumentException("Resolution may not be unknown"),
                Resolution.PT15M => interval.Duration.TotalMinutes / 15,
                Resolution.PT1H => interval.Duration.TotalHours,
                Resolution.P1D => interval.Duration.TotalDays,
                Resolution.P1M =>
                    // https://stackoverflow.com/a/4639057
                    ((_endTime.InUtc().Year - _startTime.InUtc().Year) * 12) + _endTime.InUtc().Month -
                    _startTime.InUtc().Month,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        public ValidationRuleIdentifier ValidationRuleIdentifier =>
            ValidationRuleIdentifier.ChargeDescriptionHasMaximumLength;

        public bool IsValid => _expectedPointCount == _actualPointCount;
    }
}
