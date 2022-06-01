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

using System.Collections.Generic;
using System.Linq;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using NodaTime;

namespace GreenEnergyHub.Charges.Tests.Builders.Command
{
   public class ChargePriceDtoBuilder
    {
        private List<Point> _points;
        private string _operationId;
        private string _chargeId;
        private string _owner;
        private ChargeType _chargeType;
        private Instant _startDateTime;
        private Instant? _endDateTime;
        private Instant? _pointsStartInterval;
        private Instant? _pointsEndInterval;

        public ChargePriceDtoBuilder()
        {
            _operationId = "operationId";
            _chargeId = "some charge id";
            _owner = "owner";
            _chargeType = ChargeType.Fee;
            _startDateTime = SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromDays(500));
            _endDateTime = SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromDays(1000));
            _points = new List<Point>();
            _pointsStartInterval = null;
            _pointsEndInterval = null;
        }

        public ChargePriceDtoBuilder WithChargeOperationId(string operationId)
        {
            _operationId = operationId;
            return this;
        }

        public ChargePriceDtoBuilder WithChargeId(string chargeId)
        {
            _chargeId = chargeId;
            return this;
        }

        public ChargePriceDtoBuilder WithOwner(string owner)
        {
            _owner = owner;
            return this;
        }

        public ChargePriceDtoBuilder WithChargeType(ChargeType type)
        {
            _chargeType = type;
            return this;
        }

        public ChargePriceDtoBuilder WithStartDateTime(Instant startDateTime)
        {
            _startDateTime = startDateTime;
            return this;
        }

        public ChargePriceDtoBuilder WithEndDateTime(Instant endDateTime)
        {
            _endDateTime = endDateTime;
            return this;
        }

        public ChargePriceDtoBuilder WithPoints(List<Point> points)
        {
            _points = points;
            _pointsStartInterval = _points.Min(x => x.Time);
            _pointsEndInterval = _points.Max(x => x.Time) + Duration.FromMinutes(1);
            return this;
        }

        public ChargePriceDtoBuilder WithPoint(int position, decimal price)
        {
            _points.Add(new Point(position, price, SystemClock.Instance.GetCurrentInstant()));
            _pointsStartInterval = _points.Min(x => x.Time);
            _pointsEndInterval = _points.Max(x => x.Time) + Duration.FromMinutes(1);
            return this;
        }

        public ChargePriceDtoBuilder WithPointWithXNumberOfPrices(int numberOfPrices)
        {
            for (var i = 0; i < numberOfPrices; i++)
            {
                var point = new Point(i + 1, i * 10, SystemClock.Instance.GetCurrentInstant());
                _points.Add(point);
            }

            _pointsStartInterval = _points.Min(x => x.Time);
            _pointsEndInterval = _points.Max(x => x.Time) + Duration.FromMinutes(1);
            return this;
        }

        public ChargePriceDto Build()
        {
            return new ChargePriceDto(
                _operationId,
                _chargeType,
                _chargeId,
                _owner,
                _startDateTime,
                _endDateTime,
                _pointsStartInterval,
                _pointsEndInterval,
                _points);
        }
    }
}
