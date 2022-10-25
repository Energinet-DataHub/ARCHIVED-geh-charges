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
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.TestCore.Data;
using NodaTime;

namespace GreenEnergyHub.Charges.TestCore.Builders.Command
{
    public class DocumentDtoBuilder
    {
        private readonly Instant _requestDate = SystemClock.Instance.GetCurrentInstant();
        private readonly Instant _createdDateTime = SystemClock.Instance.GetCurrentInstant();
        private IndustryClassification _industryClassification = IndustryClassification.Unknown;
        private string _id;
        private DocumentType _type = DocumentType.Unknown;
        private MarketParticipantDto _recipient;
        private MarketParticipantDto _sender;
        private BusinessReasonCode _businessReasonCode = BusinessReasonCode.Unknown;

        public DocumentDtoBuilder()
        {
            _id = Guid.NewGuid().ToString("N");
            _recipient = new MarketParticipantDtoBuilder()
                .WithMarketParticipantId(SeededData.MarketParticipants.MeteringPointAdministrator.Gln)
                .WithMarketParticipantRole(MarketParticipantRole.MeteringPointAdministrator)
                .Build();
            _sender = new MarketParticipantDtoBuilder()
                .WithMarketParticipantId(SeededData.GridAreaLink.Provider8100000000030.MarketParticipantId)
                .WithMarketParticipantRole(MarketParticipantRole.GridAccessProvider)
                .Build();
        }

        public DocumentDtoBuilder WithDocumentId(string id)
        {
            _id = id;
            return this;
        }

        public DocumentDtoBuilder WithSender(MarketParticipantDto sender)
        {
            _sender = sender;
            return this;
        }

        public DocumentDtoBuilder WithRecipient(MarketParticipantDto recipient)
        {
            _recipient = recipient;
            return this;
        }

        public DocumentDtoBuilder WithBusinessReasonCode(BusinessReasonCode businessReasonCode)
        {
            _businessReasonCode = businessReasonCode;
            return this;
        }

        public DocumentDtoBuilder WithDocumentType(DocumentType type)
        {
            _type = type;
            return this;
        }

        public DocumentDtoBuilder WithIndustryClassification(IndustryClassification industryClassification)
        {
            _industryClassification = industryClassification;
            return this;
        }

        public DocumentDto Build()
        {
            return new DocumentDto(
                _id,
                _requestDate,
                _type,
                _createdDateTime,
                _sender,
                _recipient,
                _industryClassification,
                _businessReasonCode);
        }
    }
}
