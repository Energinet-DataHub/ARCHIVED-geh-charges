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
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommands;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Domain.SharedDtos;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived;
using GreenEnergyHub.Messaging.Protobuf;
using GreenEnergyHub.Messaging.Transport;
using ChargeLinkCommand = GreenEnergyHub.Charges.Domain.ChargeLinkCommands.ChargeLinkCommand;
using MarketParticipant = GreenEnergyHub.Charges.Domain.MarketParticipants.MarketParticipant;

namespace GreenEnergyHub.Charges.Infrastructure.Internal.Mappers
{
    public class LinkCommandReceivedInboundMapper : ProtobufInboundMapper<GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.ChargeLinkCommandReceived>
    {
        protected override IInboundMessage Convert(
            [NotNull]GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.ChargeLinkCommandReceived chargeLinkCommandReceived)
        {
            return new ChargeLinkCommandReceivedEvent(
                chargeLinkCommandReceived.PublishedTime.ToInstant(),
                chargeLinkCommandReceived.CorrelationId,
                chargeLinkCommandReceived.ChargeLinkCommands.Select(linkCommand =>
                    new ChargeLinkCommand(linkCommand.CorrelationId)
                {
                    Document = ConvertDocument(linkCommand.Document),
                    ChargeLink = ConvertChargeLink(linkCommand.ChargeLink),
                }).ToList());
        }

        private static DocumentDto ConvertDocument(GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.Document document)
        {
            return new DocumentDto
            {
                Id = document.Id,
                RequestDate = document.RequestDate.ToInstant(),
                Type = (GreenEnergyHub.Charges.Domain.MarketParticipants.DocumentType)document.Type,
                CreatedDateTime = document.CreatedDateTime.ToInstant(),
                Sender = new MarketParticipant
                {
                    Id = document.Sender.Id,
                    BusinessProcessRole = (GreenEnergyHub.Charges.Domain.MarketParticipants.MarketParticipantRole)document.Sender.BusinessProcessRole,
                },
                Recipient = new MarketParticipant
                {
                    Id = document.Recipient.Id,
                    BusinessProcessRole = (GreenEnergyHub.Charges.Domain.MarketParticipants.MarketParticipantRole)document.Recipient.BusinessProcessRole,
                },
                IndustryClassification = (GreenEnergyHub.Charges.Domain.MarketParticipants.IndustryClassification)document.IndustryClassification,
                BusinessReasonCode = (GreenEnergyHub.Charges.Domain.MarketParticipants.BusinessReasonCode)document.BusinessReasonCode,
            };
        }

        private static ChargeLinkDto ConvertChargeLink(GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.ChargeLink chargeLink)
        {
            return new ChargeLinkDto
            {
                OperationId = chargeLink.OperationId,
                MeteringPointId = chargeLink.MeteringPointId,
                SenderProvidedChargeId = chargeLink.SenderProvidedChargeId,
                ChargeOwner = chargeLink.ChargeOwner,
                Factor = chargeLink.Factor,
                ChargeType = (GreenEnergyHub.Charges.Domain.Charges.ChargeType)chargeLink.ChargeType,
                StartDateTime = chargeLink.StartDateTime.ToInstant(),
                EndDateTime = chargeLink.EndDateTime?.ToInstant(),
            };
        }
    }
}
