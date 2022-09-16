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
using System.Collections.Generic;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using NodaTime;

namespace GreenEnergyHub.Charges.TestCore.Builders.Command
{
   public class ChargeInformationOperationDtoBuilder
    {
        private List<Point> _points;
        private string _chargeId;
        private Instant _startDateTime;
        private Instant? _endDateTime;
        private VatClassification _vatClassification;
        private TransparentInvoicing _transparentInvoicing;
        private TaxIndicator _taxIndicator;
        private string _owner;
        private string _description;
        private string _chargeName;
        private ChargeType _chargeType;
        private Resolution _resolution;
        private Resolution _priceResolution;
        private string _operationId;
        private Instant? _pointsStartInterval;
        private Instant? _pointsEndInterval;

        public ChargeInformationOperationDtoBuilder()
        {
            _operationId = "operationId";
            _chargeId = "some charge id";
            _startDateTime = InstantHelper.GetTodayPlusDaysAtMidnightUtc(31);
            _endDateTime = InstantHelper.GetEndDefault();
            _vatClassification = VatClassification.Vat25;
            _taxIndicator = TaxIndicator.Tax;
            _owner = "owner";
            _description = "some description";
            _chargeName = "some charge name";
            _chargeType = ChargeType.Fee;
            _points = new List<Point>();
            _resolution = Resolution.PT1H;
            _priceResolution = Resolution.PT1H;
            _pointsStartInterval = _startDateTime;
            _pointsEndInterval = _endDateTime;
        }

        public ChargeInformationOperationDtoBuilder WithDescription(string description)
        {
            _description = description;
            return this;
        }

        public ChargeInformationOperationDtoBuilder WithChargeName(string name)
        {
            _chargeName = name;
            return this;
        }

        public ChargeInformationOperationDtoBuilder WithChargeOperationId(string operationId)
        {
            _operationId = operationId;
            return this;
        }

        public ChargeInformationOperationDtoBuilder WithChargeId(string chargeId)
        {
            _chargeId = chargeId;
            return this;
        }

        public ChargeInformationOperationDtoBuilder WithTaxIndicator(TaxIndicator taxIndicator)
        {
            _taxIndicator = taxIndicator;
            return this;
        }

        public ChargeInformationOperationDtoBuilder WithOwner(string owner)
        {
            _owner = owner;
            return this;
        }

        public ChargeInformationOperationDtoBuilder WithVatClassification(VatClassification vatClassification)
        {
            _vatClassification = vatClassification;
            return this;
        }

        public ChargeInformationOperationDtoBuilder WithTransparentInvoicing(TransparentInvoicing transparentInvoicing)
        {
            _transparentInvoicing = transparentInvoicing;
            return this;
        }

        public ChargeInformationOperationDtoBuilder WithChargeType(ChargeType type)
        {
            _chargeType = type;
            return this;
        }

        public ChargeInformationOperationDtoBuilder WithStartDateTime(Instant startDateTime)
        {
            _startDateTime = startDateTime;
            return this;
        }

        public ChargeInformationOperationDtoBuilder WithEndDateTime(Instant? endDateTime)
        {
            _endDateTime = endDateTime;
            return this;
        }

        public ChargeInformationOperationDtoBuilder WithPoints(List<Point> points)
        {
            _points = points;
            return this;
        }

        public ChargeInformationOperationDtoBuilder WithPoint(decimal price)
        {
            _points.Add(new Point(price, _startDateTime));
            return this;
        }

        public ChargeInformationOperationDtoBuilder WithPointsInterval(Instant startTime, Instant endTime)
        {
            _pointsStartInterval = startTime;
            _pointsEndInterval = endTime;
            return this;
        }

        public ChargeInformationOperationDtoBuilder WithPointWithXNumberOfPrices(int numberOfPrices)
        {
            for (var i = 0; i < numberOfPrices; i++)
            {
                var point = new Point(i * 10, GetTimeForPoint(_startDateTime, i));
                _points.Add(point);
            }

            return this;
        }

        public ChargeInformationOperationDtoBuilder WithResolution(Resolution resolution)
        {
            _resolution = resolution;
            return this;
        }

        public ChargeInformationOperationDtoBuilder WithPriceResolution(Resolution priceResolution)
        {
            _priceResolution = priceResolution;
            return this;
        }

        public ChargeInformationOperationDto Build()
        {
            return new ChargeInformationOperationDto(
                _operationId,
                _chargeType,
                _chargeId,
                _chargeName,
                _description,
                _owner,
                _resolution,
                _priceResolution,
                _taxIndicator,
                _transparentInvoicing,
                _vatClassification,
                _startDateTime,
                _endDateTime,
                _pointsStartInterval,
                _pointsEndInterval,
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
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }
}
