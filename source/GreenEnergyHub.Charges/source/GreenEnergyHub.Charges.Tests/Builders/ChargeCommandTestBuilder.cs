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

using System.Collections.Generic;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using NodaTime;

namespace GreenEnergyHub.Charges.Tests.Builders
{
   public class ChargeCommandTestBuilder
    {
        private string _chargeId;
        private Instant _startDateTime;
        private Instant _endDateTime;
        private VatClassification _vatClassification;
        private bool _taxIndicator;
        private int _status;
        private string _owner;
        private string _description;
        private string _chargeName;
        private string _documentId;
        private MarketParticipant _sender;
        private ChargeType _chargeType;
        private List<Point> _points;
        private Resolution _resolution;
        private string _id;

        public ChargeCommandTestBuilder()
        {
            _id = "id";
            _chargeId = "some charge id";
            _startDateTime = SystemClock.Instance.GetCurrentInstant()
                .Plus(Duration.FromDays(500));
            _endDateTime = SystemClock.Instance.GetCurrentInstant()
                .Plus(Duration.FromDays(1000));
            _vatClassification = VatClassification.Vat25;
            _status = 2; // Create
            _taxIndicator = false;
            _owner = "owner";
            _description = "some description";
            _chargeName = "some charge name";
            _documentId = "some document id";
            _sender = new MarketParticipant { Id = "0", BusinessProcessRole = MarketParticipantRole.EnergySupplier };
            _chargeType = ChargeType.Fee;
            _points = new List<Point>();
            _resolution = Resolution.PT1H;
        }

        public ChargeCommandTestBuilder WithDescription(string description)
        {
            _description = description;
            return this;
        }

        public ChargeCommandTestBuilder WithDocumentId(string documentId)
        {
            _documentId = documentId;
            return this;
        }

        public ChargeCommandTestBuilder WithChargeName(string name)
        {
            _chargeName = name;
            return this;
        }

        public ChargeCommandTestBuilder WithChargeId(string chargeId)
        {
            _chargeId = chargeId;
            return this;
        }

        public ChargeCommandTestBuilder WithId(string id)
        {
            _id = id;
            return this;
        }

        public ChargeCommandTestBuilder WithValidityStartDateDays(int days)
        {
            _startDateTime = SystemClock.Instance.GetCurrentInstant()
                .Plus(Duration.FromDays(days));
            return this;
        }

        public ChargeCommandTestBuilder WithTaxIndicator(bool taxIndicator)
        {
            _taxIndicator = taxIndicator;
            return this;
        }

        public ChargeCommandTestBuilder WithOwner(string owner)
        {
            _owner = owner;
            return this;
        }

        public ChargeCommandTestBuilder WithVatClassification(VatClassification vatClassification)
        {
            _vatClassification = vatClassification;
            return this;
        }

        public ChargeCommandTestBuilder WithStatus(int status)
        {
            _status = status;
            return this;
        }

        public ChargeCommandTestBuilder WithChargeType(ChargeType type)
        {
            _chargeType = type;
            return this;
        }

        public ChargeCommandTestBuilder WithSender(MarketParticipant sender)
        {
            _sender = sender;
            return this;
        }

        public ChargeCommandTestBuilder WithStartDateTime(Instant startDateTime)
        {
            _startDateTime = startDateTime;
            return this;
        }

        public ChargeCommandTestBuilder WithPoint(decimal price)
        {
            _points.Add(new Point(0, price, SystemClock.Instance.GetCurrentInstant()));
            return this;
        }

        public ChargeCommandTestBuilder WithPointWithXNumberOfPrices(int numberOfPrices)
        {
            for (var i = 0; i < numberOfPrices; i++)
            {
                var point = new Point(i, i * 10, SystemClock.Instance.GetCurrentInstant());
                _points.Add(point);
            }

            return this;
        }

        public ChargeCommandTestBuilder WithResolution(Resolution resolution)
        {
            _resolution = resolution;
            return this;
        }

        public ChargeCommand Build()
        {
            return new()
            {
                Document = new DocumentDto
                {
                    Id = _documentId,
                    Type = DocumentType.RequestUpdateChargeInformation,
                    RequestDate = SystemClock.Instance.GetCurrentInstant(),
                    IndustryClassification = IndustryClassification.Electricity,
                    CreatedDateTime = SystemClock.Instance.GetCurrentInstant(),
                    Recipient = new MarketParticipant
                    {
                        Id = "0",
                        BusinessProcessRole = MarketParticipantRole.EnergySupplier,
                    },
                    Sender = _sender,
                    BusinessReasonCode = BusinessReasonCode.UpdateChargeInformation,
                },
                ChargeOperation = new ChargeOperationDto(
                    _id,
                    _chargeType,
                    _chargeId,
                    _chargeName,
                    _description,
                    _owner,
                    _resolution,
                    _taxIndicator,
                    true,
                    _vatClassification,
                    _startDateTime,
                    _endDateTime,
                    _points),
            };
        }
    }
}
