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
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using NodaTime;

namespace GreenEnergyHub.Charges.Tests.Builders.Command
{
   public class ChargeCommandBuilder
    {
        private readonly List<Point> _points;
        private string _chargeId;
        private Instant _startDateTime;
        private Instant? _endDateTime;
        private VatClassification _vatClassification;
        private bool _taxIndicator;
        private string _owner;
        private string _description;
        private string _chargeName;
        private string _documentId;
        private BusinessReasonCode _documentBusinessReasonCode;
        private DocumentType _documentType;
        private MarketParticipantDto _sender;
        private ChargeType _chargeType;
        private Resolution _resolution;
        private string _operationId;
        private OperationType _operationType;

        public ChargeCommandBuilder()
        {
            _operationId = "id";
            _chargeId = "some charge id";
            _startDateTime = SystemClock.Instance.GetCurrentInstant()
                .Plus(Duration.FromDays(500));
            _endDateTime = SystemClock.Instance.GetCurrentInstant()
                .Plus(Duration.FromDays(1000));
            _vatClassification = VatClassification.Vat25;
            _taxIndicator = false;
            _owner = "owner";
            _description = "some description";
            _chargeName = "some charge name";
            _documentId = "some document id";
            _documentBusinessReasonCode = BusinessReasonCode.UpdateChargeInformation;
            _documentType = DocumentType.RequestUpdateChargeInformation;
            _sender = new MarketParticipantDto { Id = "0", BusinessProcessRole = MarketParticipantRole.EnergySupplier };
            _chargeType = ChargeType.Fee;
            _operationType = OperationType.Create;
            _points = new List<Point>();
            _resolution = Resolution.PT1H;
        }

        public ChargeCommandBuilder WithEndDateTimeAsNull()
        {
            _endDateTime = null;
            return this;
        }

        public ChargeCommandBuilder WithDescription(string description)
        {
            _description = description;
            return this;
        }

        public ChargeCommandBuilder WithDocumentId(string documentId)
        {
            _documentId = documentId;
            return this;
        }

        public ChargeCommandBuilder WithDocumentBusinessReasonCode(BusinessReasonCode businessReasonCode)
        {
            _documentBusinessReasonCode = businessReasonCode;
            return this;
        }

        public ChargeCommandBuilder WithDocumentType(DocumentType documentType)
        {
            _documentType = documentType;
            return this;
        }

        public ChargeCommandBuilder WithChargeName(string name)
        {
            _chargeName = name;
            return this;
        }

        public ChargeCommandBuilder WithChargeId(string chargeId)
        {
            _chargeId = chargeId;
            return this;
        }

        public ChargeCommandBuilder WithOperationId(string id)
        {
            _operationId = id;
            return this;
        }

        public ChargeCommandBuilder WithValidityStartDateDays(int days)
        {
            _startDateTime = SystemClock.Instance.GetCurrentInstant()
                .Plus(Duration.FromDays(days));
            return this;
        }

        public ChargeCommandBuilder WithTaxIndicator(bool taxIndicator)
        {
            _taxIndicator = taxIndicator;
            return this;
        }

        public ChargeCommandBuilder WithOwner(string owner)
        {
            _owner = owner;
            return this;
        }

        public ChargeCommandBuilder WithVatClassification(VatClassification vatClassification)
        {
            _vatClassification = vatClassification;
            return this;
        }

        public ChargeCommandBuilder WithStatus(int status)
        {
            return this;
        }

        public ChargeCommandBuilder WithChargeType(ChargeType type)
        {
            _chargeType = type;
            return this;
        }

        public ChargeCommandBuilder WithSender(MarketParticipantDto sender)
        {
            _sender = sender;
            return this;
        }

        public ChargeCommandBuilder WithStartDateTime(Instant startDateTime)
        {
            _startDateTime = startDateTime;
            return this;
        }

        public ChargeCommandBuilder WithEndDateTime(Instant endDateTime)
        {
            _endDateTime = endDateTime;
            return this;
        }

        public ChargeCommandBuilder WithPoint(int position, decimal price)
        {
            _points.Add(new Point(position, price, SystemClock.Instance.GetCurrentInstant()));
            return this;
        }

        public ChargeCommandBuilder WithOperationType(OperationType operationType)
        {
            _operationType = operationType;
            return this;
        }

        public ChargeCommandBuilder WithPointWithXNumberOfPrices(int numberOfPrices)
        {
            for (var i = 0; i < numberOfPrices; i++)
            {
                var point = new Point(i + 1, i * 10, SystemClock.Instance.GetCurrentInstant());
                _points.Add(point);
            }

            return this;
        }

        public ChargeCommandBuilder WithResolution(Resolution resolution)
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
                    Type = _documentType,
                    RequestDate = SystemClock.Instance.GetCurrentInstant(),
                    IndustryClassification = IndustryClassification.Electricity,
                    CreatedDateTime = SystemClock.Instance.GetCurrentInstant(),
                    Recipient = new MarketParticipantDto
                    {
                        Id = "0",
                        BusinessProcessRole = MarketParticipantRole.EnergySupplier,
                    },
                    Sender = _sender,
                    BusinessReasonCode = _documentBusinessReasonCode,
                },
                ChargeOperation = new ChargeOperationDto(
                    _operationId,
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
                    _operationType,
                    _points),
            };
        }
    }
}
