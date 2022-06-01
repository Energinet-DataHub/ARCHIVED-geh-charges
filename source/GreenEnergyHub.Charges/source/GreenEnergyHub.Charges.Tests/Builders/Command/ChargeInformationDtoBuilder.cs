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
   public class ChargeInformationDtoBuilder
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
        private string _operationId;
        private Instant? _pointsStartInterval;
        private Instant? _pointsEndInterval;

        public ChargeInformationDtoBuilder()
        {
            _operationId = "operationId";
            _chargeId = "some charge id";
            _startDateTime = SystemClock.Instance.GetCurrentInstant()
                .Plus(Duration.FromDays(500));
            _endDateTime = SystemClock.Instance.GetCurrentInstant()
                .Plus(Duration.FromDays(1000));
            _vatClassification = VatClassification.Vat25;
            _taxIndicator = TaxIndicator.Tax;
            _owner = "owner";
            _description = "some description";
            _chargeName = "some charge name";
            _chargeType = ChargeType.Fee;
            _points = new List<Point>();
            _resolution = Resolution.PT1H;
            _pointsStartInterval = null;
            _pointsEndInterval = null;
        }

        public ChargeInformationDtoBuilder WithDescription(string description)
        {
            _description = description;
            return this;
        }

        public ChargeInformationDtoBuilder WithChargeName(string name)
        {
            _chargeName = name;
            return this;
        }

        public ChargeInformationDtoBuilder WithChargeOperationId(string operationId)
        {
            _operationId = operationId;
            return this;
        }

        public ChargeInformationDtoBuilder WithChargeId(string chargeId)
        {
            _chargeId = chargeId;
            return this;
        }

        public ChargeInformationDtoBuilder WithTaxIndicator(TaxIndicator taxIndicator)
        {
            _taxIndicator = taxIndicator;
            return this;
        }

        public ChargeInformationDtoBuilder WithOwner(string owner)
        {
            _owner = owner;
            return this;
        }

        public ChargeInformationDtoBuilder WithVatClassification(VatClassification vatClassification)
        {
            _vatClassification = vatClassification;
            return this;
        }

        public ChargeInformationDtoBuilder WithTransparentInvoicing(TransparentInvoicing transparentInvoicing)
        {
            _transparentInvoicing = transparentInvoicing;
            return this;
        }

        public ChargeInformationDtoBuilder WithChargeType(ChargeType type)
        {
            _chargeType = type;
            return this;
        }

        public ChargeInformationDtoBuilder WithStartDateTime(Instant startDateTime)
        {
            _startDateTime = startDateTime;
            return this;
        }

        public ChargeInformationDtoBuilder WithEndDateTime(Instant endDateTime)
        {
            _endDateTime = endDateTime;
            return this;
        }

        public ChargeInformationDtoBuilder WithPoints(List<Point> points)
        {
            _points = points;
            _pointsStartInterval = _points.Min(x => x.Time);
            _pointsEndInterval = _points.Max(x => x.Time) + Duration.FromMinutes(1);
            return this;
        }

        public ChargeInformationDtoBuilder WithPoint(int position, decimal price)
        {
            _points.Add(new Point(position, price, SystemClock.Instance.GetCurrentInstant()));
            _pointsStartInterval = _points.Min(x => x.Time);
            _pointsEndInterval = _points.Max(x => x.Time) + Duration.FromMinutes(1);
            return this;
        }

        public ChargeInformationDtoBuilder WithPointWithXNumberOfPrices(int numberOfPrices)
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

        public ChargeInformationDtoBuilder WithResolution(Resolution resolution)
        {
            _resolution = resolution;
            return this;
        }

        public ChargeInformationDto Build()
        {
            return new ChargeInformationDto(
                _operationId,
                _chargeType,
                _chargeId,
                _chargeName,
                _description,
                _owner,
                _resolution,
                _taxIndicator,
                _transparentInvoicing,
                _vatClassification,
                _startDateTime,
                _endDateTime,
                _pointsStartInterval,
                _pointsEndInterval,
                _points);
        }
    }
}
