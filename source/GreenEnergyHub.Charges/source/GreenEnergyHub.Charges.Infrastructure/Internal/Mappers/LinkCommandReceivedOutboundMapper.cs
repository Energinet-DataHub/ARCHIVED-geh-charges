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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinkCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinkCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Messaging.Protobuf;

namespace GreenEnergyHub.Charges.Infrastructure.Internal.Mappers
{
    public class LinkCommandReceivedOutboundMapper : ProtobufOutboundMapper<ChargeLinkCommandReceivedEvent>
    {
        protected override Google.Protobuf.IMessage Convert([NotNull]ChargeLinkCommandReceivedEvent chargeLinkCommandReceivedEvent)
        {
            var chargeLinkCommandReceived = new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.ChargeLinkCommandReceived
            {
                PublishedTime = chargeLinkCommandReceivedEvent.PublishedTime.ToTimestamp().TruncateToSeconds(),
                ChargeLinksCommand = new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.ChargeLinkCommand
                {
                    MeteringPointId = chargeLinkCommandReceivedEvent.ChargeLinksCommand.MeteringPointId,
                    Document = ConvertDocument(chargeLinkCommandReceivedEvent.ChargeLinksCommand.Document),
                },
            };

            foreach (var chargeLinkDto in chargeLinkCommandReceivedEvent.ChargeLinksCommand.ChargeLinks)
            {
                chargeLinkCommandReceived.ChargeLinksCommand.ChargeLinks.Add(ConvertChargeLink(chargeLinkDto));
            }

            return chargeLinkCommandReceived;
        }

        private static GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.Document ConvertDocument(DocumentDto documentDto)
        {
            return new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.Document
            {
                Id = documentDto.Id,
                RequestDate = documentDto.RequestDate.ToTimestamp(),
                Type = (GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.DocumentType)documentDto.Type,
                CreatedDateTime = documentDto.CreatedDateTime.ToTimestamp(),
                Sender = new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.MarketParticipant
                {
                    Id = documentDto.Sender.Id,
                    BusinessProcessRole = (GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.MarketParticipantRole)documentDto.Sender.BusinessProcessRole,
                },
                Recipient = new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.MarketParticipant
                {
                    Id = documentDto.Recipient.Id,
                    BusinessProcessRole = (GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.MarketParticipantRole)documentDto.Recipient.BusinessProcessRole,
                },
                IndustryClassification = (GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.IndustryClassification)documentDto.IndustryClassification,
                BusinessReasonCode = (GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.BusinessReasonCode)documentDto.BusinessReasonCode,
            };
        }

        private static GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.ChargeLink ConvertChargeLink(ChargeLinkDto chargeLinkDto)
        {
            return new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.ChargeLink
            {
                OperationId = chargeLinkDto.OperationId,
                SenderProvidedChargeId = chargeLinkDto.SenderProvidedChargeId,
                ChargeOwner = chargeLinkDto.ChargeOwner,
                Factor = chargeLinkDto.Factor,
                ChargeType = (GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.ChargeType)chargeLinkDto.ChargeType,
                StartDateTime = chargeLinkDto.StartDateTime.ToTimestamp(),
                EndDateTime = chargeLinkDto.EndDateTime?.ToTimestamp(),
            };
        }
    }
}
