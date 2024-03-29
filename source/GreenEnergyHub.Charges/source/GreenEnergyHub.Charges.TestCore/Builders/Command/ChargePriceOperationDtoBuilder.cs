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
using System.Collections.Generic;
using System.Linq;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.TestCore.TestHelpers;
using NodaTime;

namespace GreenEnergyHub.Charges.TestCore.Builders.Command
{
   public class ChargePriceOperationDtoBuilder
    {
        private List<Point> _points;
        private string _chargeId;
        private Instant _startDateTime;
        private Instant _endDateTime;
        private string _owner;
        private ChargeType _chargeType;
        private Resolution _priceResolution;
        private string _operationId;
        private Instant _pointsStartInterval;
        private Instant _pointsEndInterval;

        public ChargePriceOperationDtoBuilder()
        {
            _operationId = Guid.NewGuid().ToString();
            _chargeId = "some charge id";
            _startDateTime = InstantHelper.GetTodayPlusDaysAtMidnightUtc(31);
            _endDateTime = InstantHelper.GetEndDefault();
            _owner = "owner";
            _chargeType = ChargeType.Fee;
            _points = new List<Point>();
            _priceResolution = Resolution.PT1H;
            _pointsStartInterval = _startDateTime;
            _pointsEndInterval = _endDateTime;
        }

        public ChargePriceOperationDtoBuilder WithChargePriceOperationId(string operationId)
        {
            _operationId = operationId;
            return this;
        }

        public ChargePriceOperationDtoBuilder WithChargeId(string chargeId)
        {
            _chargeId = chargeId;
            return this;
        }

        public ChargePriceOperationDtoBuilder WithOwner(string owner)
        {
            _owner = owner;
            return this;
        }

        public ChargePriceOperationDtoBuilder WithChargeType(ChargeType type)
        {
            _chargeType = type;
            return this;
        }

        public ChargePriceOperationDtoBuilder WithStartDateTime(Instant startDateTime)
        {
            _startDateTime = startDateTime;
            return this;
        }

        public ChargePriceOperationDtoBuilder WithEndDateTime(Instant endDateTime)
        {
            _endDateTime = endDateTime;
            return this;
        }

        public ChargePriceOperationDtoBuilder WithPoints(List<Point> points)
        {
            _points = points;
            return this;
        }

        public ChargePriceOperationDtoBuilder WithPoint(decimal price)
        {
            _points.Add(new Point(price, _startDateTime));
            return this;
        }

        public ChargePriceOperationDtoBuilder WithPointsInterval(Instant startTime, Instant endTime)
        {
            _pointsStartInterval = startTime;
            _pointsEndInterval = endTime;
            return this;
        }

        public ChargePriceOperationDtoBuilder WithPointWithXNumberOfPrices(int numberOfPrices)
        {
            for (var i = 0; i < numberOfPrices; i++)
            {
                var point = new Point(i * 10, GetTimeForPoint(_startDateTime, i));
                _points.Add(point);
            }

            return this;
        }

        public ChargePriceOperationDtoBuilder WithPriceResolution(Resolution priceResolution)
        {
            _priceResolution = priceResolution;
            return this;
        }

        public ChargePriceOperationDto Build()
        {
            if (!_points.Any())
            {
                _points.Add(new Point(1.00m, _startDateTime));
            }

            return new ChargePriceOperationDto(
                _operationId,
                _chargeType,
                _chargeId,
                _owner,
                _startDateTime,
                _endDateTime,
                _pointsStartInterval,
                _pointsEndInterval,
                _priceResolution,
                _points);
        }

        private Instant GetTimeForPoint(Instant startTime, int index)
        {
            return _priceResolution switch
            {
                Resolution.Unknown => SystemClock.Instance.GetCurrentInstant(),
                Resolution.PT15M => startTime.Plus(Duration.FromMinutes(15) * (index + 1)),
                Resolution.PT1H => startTime.Plus(Duration.FromHours(1) * (index + 1)),
                Resolution.P1D => startTime.Plus(Duration.FromDays(1) * (index + 1)),
                Resolution.P1M => startTime.Plus(Duration.FromDays(30) * (index + 1)),
                _ => throw new ArgumentOutOfRangeException($"{_priceResolution} is out of range of {nameof(Resolution)}"),
            };
        }
    }
}
