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

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Energinet.DataHub.Core.Messaging.Protobuf;
using Energinet.DataHub.Core.Messaging.Transport;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;

namespace GreenEnergyHub.Charges.Infrastructure.Internal.Mappers
{
    public class LinkCommandAcceptedInboundMapper
        : ProtobufInboundMapper<GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.ChargeLinkCommandAccepted>
    {
        protected override IInboundMessage Convert(
            [NotNull]GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.ChargeLinkCommandAccepted chargeLinkCommandAcceptedContract)
        {
            return new ChargeLinksAcceptedEvent(
                new ChargeLinksCommand(
                    chargeLinkCommandAcceptedContract.ChargeLinksCommand.MeteringPointId,
                    ConvertDocument(chargeLinkCommandAcceptedContract.ChargeLinksCommand.Document),
                    chargeLinkCommandAcceptedContract.ChargeLinksCommand.ChargeLinks.Select(ConvertChargeLink).ToList()),
                chargeLinkCommandAcceptedContract.PublishedTime.ToInstant());
        }

        private static DocumentDto ConvertDocument(ChargeLinkCommandAccepted.Document document)
        {
            return new DocumentDto
            {
                Id = document.Id,
                RequestDate = document.RequestDate.ToInstant(),
                Type = (Domain.MarketParticipants.DocumentType)document.Type,
                CreatedDateTime = document.CreatedDateTime.ToInstant(),
                Sender = MapMarketParticipant(document.Sender),
                Recipient = MapMarketParticipant(document.Recipient),
                IndustryClassification = (Domain.MarketParticipants.IndustryClassification)document.IndustryClassification,
                BusinessReasonCode = (Domain.MarketParticipants.BusinessReasonCode)document.BusinessReasonCode,
            };
        }

        private static MarketParticipantDto MapMarketParticipant(
            ChargeLinkCommandAccepted.MarketParticipant marketParticipant)
        {
            return new MarketParticipantDto
            {
                Id = marketParticipant.Id,
                BusinessProcessRole = (Domain.MarketParticipants.MarketParticipantRole)marketParticipant.BusinessProcessRole,
            };
        }

        private static ChargeLinkDto ConvertChargeLink(ChargeLinkCommandAccepted.ChargeLink link)
        {
            return new ChargeLinkDto
            {
                OperationId = link.OperationId,
                StartDateTime = link.StartDateTime.ToInstant(),
                EndDateTime = link.EndDateTime.ToInstant(),
                SenderProvidedChargeId = link.SenderProvidedChargeId,
                Factor = link.Factor,
                ChargeOwnerId = link.ChargeOwnerId,
                ChargeType = (GreenEnergyHub.Charges.Domain.Charges.ChargeType)link.ChargeType,
            };
        }
    }
}
