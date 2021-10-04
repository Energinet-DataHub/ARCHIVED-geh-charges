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
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommands;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted;
using GreenEnergyHub.Messaging.Protobuf;
using GreenEnergyHub.Messaging.Transport;

namespace GreenEnergyHub.Charges.Infrastructure.Internal.Mappers
{
    public class LinkCommandAcceptedInboundMapper : ProtobufInboundMapper<ChargeLinkCommandAcceptedContract>
    {
        protected override IInboundMessage Convert([NotNull]ChargeLinkCommandAcceptedContract chargeLinkCommandAcceptedContract)
        {
            return new ChargeLinkCommandAcceptedEvent(
                chargeLinkCommandAcceptedContract.CorrelationId,
                ConvertDocument(chargeLinkCommandAcceptedContract.Document),
                ConvertChargeLink(chargeLinkCommandAcceptedContract.ChargeLink));
        }

        private static Document ConvertDocument(DocumentContract document)
        {
            return new Document
            {
                Id = document.Id,
                RequestDate = document.RequestDate.ToInstant(),
                Type = (DocumentType)document.Type,
                CreatedDateTime = document.CreatedDateTime.ToInstant(),
                Sender = MapMarketParticipant(document.Sender),
                Recipient = MapMarketParticipant(document.Recipient),
                IndustryClassification = (IndustryClassification)document.IndustryClassification,
                BusinessReasonCode = (BusinessReasonCode)document.BusinessReasonCode,
            };
        }

        private static MarketParticipant MapMarketParticipant(MarketParticipantContract marketParticipant)
        {
            return new MarketParticipant
            {
                Id = marketParticipant.Id,
                BusinessProcessRole = (MarketParticipantRole)marketParticipant.BusinessProcessRole,
            };
        }

        private static ChargeLinkDto ConvertChargeLink(ChargeLinkContract link)
        {
            return new ChargeLinkDto
            {
                OperationId = link.OperationId,
                MeteringPointId = link.MeteringPointId,
                StartDateTime = link.StartDateTime.ToInstant(),
                EndDateTime = link.EndDateTime.ToInstant(),
                SenderProvidedChargeId = link.ChargeId,
                Factor = link.Factor,
                ChargeOwner = link.ChargeOwner,
                ChargeType = (ChargeType)link.ChargeType,
            };
        }
    }
}
