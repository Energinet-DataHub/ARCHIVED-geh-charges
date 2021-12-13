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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;

namespace GreenEnergyHub.Charges.Infrastructure.Internal.Mappers
{
    public class LinkCommandReceivedInboundMapper : ProtobufInboundMapper<GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.ChargeLinkCommandReceived>
    {
        protected override IInboundMessage Convert(
            [NotNull]GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.ChargeLinkCommandReceived chargeLinkCommandReceived)
        {
            return new ChargeLinksReceivedEvent(
                chargeLinkCommandReceived.PublishedTime.ToInstant(),
                new ChargeLinksCommand(
                    chargeLinkCommandReceived.ChargeLinksCommand.MeteringPointId,
                    ConvertDocument(chargeLinkCommandReceived.ChargeLinksCommand.Document),
                    chargeLinkCommandReceived.ChargeLinksCommand.ChargeLinks.Select(ConvertChargeLink).ToList()));
        }

        private static DocumentDto ConvertDocument(ChargeLinkCommandReceived.Document document)
        {
            return new DocumentDto
            {
                Id = document.Id,
                RequestDate = document.RequestDate.ToInstant(),
                Type = (Domain.MarketParticipants.DocumentType)document.Type,
                CreatedDateTime = document.CreatedDateTime.ToInstant(),
                Sender = new MarketParticipantDto
                {
                    Id = document.Sender.Id,
                    BusinessProcessRole = (Domain.MarketParticipants.MarketParticipantRole)document.Sender.BusinessProcessRole,
                },
                Recipient = new MarketParticipantDto
                {
                    Id = document.Recipient.Id,
                    BusinessProcessRole = (Domain.MarketParticipants.MarketParticipantRole)document.Recipient.BusinessProcessRole,
                },
                IndustryClassification = (Domain.MarketParticipants.IndustryClassification)document.IndustryClassification,
                BusinessReasonCode = (Domain.MarketParticipants.BusinessReasonCode)document.BusinessReasonCode,
            };
        }

        private static ChargeLinkDto ConvertChargeLink(ChargeLinkCommandReceived.ChargeLink chargeLink)
        {
            return new ChargeLinkDto
            {
                OperationId = chargeLink.OperationId,
                SenderProvidedChargeId = chargeLink.SenderProvidedChargeId,
                ChargeOwnerId = chargeLink.ChargeOwnerId,
                Factor = chargeLink.Factor,
                ChargeType = (GreenEnergyHub.Charges.Domain.Charges.ChargeType)chargeLink.ChargeType,
                StartDateTime = chargeLink.StartDateTime.ToInstant(),
                EndDateTime = chargeLink.EndDateTime?.ToInstant(),
            };
        }
    }
}
