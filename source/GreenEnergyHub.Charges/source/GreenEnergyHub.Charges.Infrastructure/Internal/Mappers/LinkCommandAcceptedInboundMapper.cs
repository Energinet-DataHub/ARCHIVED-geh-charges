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
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommands;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted;
using GreenEnergyHub.Messaging.Protobuf;
using GreenEnergyHub.Messaging.Transport;
using ChargeLinkCommand = GreenEnergyHub.Charges.Domain.ChargeLinkCommands.ChargeLinkCommand;
using Document = GreenEnergyHub.Charges.Domain.MarketParticipants.Document;
using MarketParticipant = GreenEnergyHub.Charges.Domain.MarketParticipants.MarketParticipant;

namespace GreenEnergyHub.Charges.Infrastructure.Internal.Mappers
{
    public class LinkCommandAcceptedInboundMapper : ProtobufInboundMapper<GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.ChargeLinkCommandAccepted>
    {
        protected override IInboundMessage Convert([NotNull]GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.ChargeLinkCommandAccepted chargeLinkCommandAcceptedContract)
        {
            return new ChargeLinkCommandAcceptedEvent(
                chargeLinkCommandAcceptedContract.CorrelationId,
                chargeLinkCommandAcceptedContract.ChargeLinkCommands.Select(
                    chargeLinkCommandContract =>
                        new ChargeLinkCommand(chargeLinkCommandContract.CorrelationId)
                {
                  Document = ConvertDocument(chargeLinkCommandContract.Document),
                  ChargeLink = ConvertChargeLink(chargeLinkCommandContract.ChargeLink),
                }).ToList(),
                chargeLinkCommandAcceptedContract.PublishedTime.ToInstant());
        }

        private static Document ConvertDocument(GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.Document document)
        {
            return new Document
            {
                Id = document.Id,
                RequestDate = document.RequestDate.ToInstant(),
                Type = (GreenEnergyHub.Charges.Domain.MarketParticipants.DocumentType)document.Type,
                CreatedDateTime = document.CreatedDateTime.ToInstant(),
                Sender = MapMarketParticipant(document.Sender),
                Recipient = MapMarketParticipant(document.Recipient),
                IndustryClassification = (GreenEnergyHub.Charges.Domain.MarketParticipants.IndustryClassification)document.IndustryClassification,
                BusinessReasonCode = (GreenEnergyHub.Charges.Domain.MarketParticipants.BusinessReasonCode)document.BusinessReasonCode,
            };
        }

        private static MarketParticipant MapMarketParticipant(GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.MarketParticipant marketParticipant)
        {
            return new MarketParticipant
            {
                Id = marketParticipant.Id,
                BusinessProcessRole = (GreenEnergyHub.Charges.Domain.MarketParticipants.MarketParticipantRole)marketParticipant.BusinessProcessRole,
            };
        }

        private static ChargeLinkDto ConvertChargeLink(GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.ChargeLink link)
        {
            return new ChargeLinkDto
            {
                OperationId = link.OperationId,
                MeteringPointId = link.MeteringPointId,
                StartDateTime = link.StartDateTime.ToInstant(),
                EndDateTime = link.EndDateTime.ToInstant(),
                SenderProvidedChargeId = link.SenderProvidedChargeId,
                Factor = link.Factor,
                ChargeOwner = link.ChargeOwner,
                ChargeType = (GreenEnergyHub.Charges.Domain.Charges.ChargeType)link.ChargeType,
            };
        }
    }
}
