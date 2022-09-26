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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;

namespace GreenEnergyHub.Charges.TestCore.Builders.Command
{
    public class ChargeLinksCommandBuilder
    {
        private DocumentDto _document;
        private static MarketParticipantRole _senderRole = MarketParticipantRole.MeteringPointAdministrator;
        private List<ChargeLinkOperationDto> _links = new();

        public ChargeLinksCommandBuilder()
        {
            _document = new DocumentDtoBuilder()
                .WithDocumentId(Guid.NewGuid().ToString())
                .WithDocumentType(DocumentType.RequestChangeBillingMasterData)
                .WithBusinessReasonCode(BusinessReasonCode.UpdateMasterDataSettlement)
                .WithIndustryClassification(IndustryClassification.Electricity)
                .WithSender(new MarketParticipantDtoBuilder()
                    .WithMarketParticipantId(SeededData.MarketParticipants.MeteringPointAdministrator.Gln)
                    .WithMarketParticipantRole(_senderRole)
                    .Build())
                .WithRecipient(new MarketParticipantDtoBuilder()
                    .WithMarketParticipantId(SeededData.MarketParticipants.GridAccessProviderOfMeteringPoint571313180000000005.Gln)
                    .WithMarketParticipantRole(MarketParticipantRole.GridAccessProvider)
                    .Build())
                .Build();

            _links = new List<ChargeLinkOperationDto>
            {
                new ChargeLinkDtoBuilder().Build(),
                new ChargeLinkDtoBuilder().Build(),
                new ChargeLinkDtoBuilder().Build(),
            };
        }

        public ChargeLinksCommandBuilder WithDocument(DocumentDto documentDto)
        {
            _document = documentDto;
            return this;
        }

        public ChargeLinksCommandBuilder WithChargeLinks(List<ChargeLinkOperationDto> links)
        {
            _links = links;
            return this;
        }

        public ChargeLinksCommand Build()
        {
            return new ChargeLinksCommand(_document, _links);
        }
    }
}
