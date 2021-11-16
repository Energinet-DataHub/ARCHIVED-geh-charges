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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinkCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinkCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Messaging.Protobuf;
using GreenEnergyHub.Messaging.Transport;
using ChargeLinkCommand = GreenEnergyHub.Charges.Domain.Dtos.ChargeLinkCommands.ChargeLinkCommand;
using MarketParticipant = GreenEnergyHub.Charges.Domain.MarketParticipants.MarketParticipant;

namespace GreenEnergyHub.Charges.Infrastructure.Internal.Mappers
{
    public class LinkCommandAcceptedInboundMapper
        : ProtobufInboundMapper<GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.ChargeLinkCommandAccepted>
    {
        protected override IInboundMessage Convert(
            [NotNull]GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.ChargeLinkCommandAccepted chargeLinkCommandAcceptedContract)
        {
            return new ChargeLinkCommandAcceptedEvent(
                chargeLinkCommandAcceptedContract.ChargeLinkCommands.Select(
                    chargeLinkCommandContract =>
                        new ChargeLinkCommand
                {
                  Document = ConvertDocument(chargeLinkCommandContract.Document),
                  ChargeLink = ConvertChargeLink(chargeLinkCommandContract.ChargeLink),
                }).ToList(),
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

        private static MarketParticipant MapMarketParticipant(
            GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.MarketParticipant marketParticipant)
        {
            return new MarketParticipant
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
