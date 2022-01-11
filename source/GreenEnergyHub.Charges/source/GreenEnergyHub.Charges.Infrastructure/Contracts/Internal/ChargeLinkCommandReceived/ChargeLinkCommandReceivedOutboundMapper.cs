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
using Energinet.DataHub.Core.Messaging.Protobuf;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;

namespace GreenEnergyHub.Charges.Infrastructure.Contracts.Internal.ChargeLinkCommandReceived
{
    public class ChargeLinkCommandReceivedOutboundMapper : ProtobufOutboundMapper<ChargeLinksReceivedEvent>
    {
        protected override Google.Protobuf.IMessage Convert([NotNull]ChargeLinksReceivedEvent chargeLinksReceivedEvent)
        {
            var chargeLinkCommandReceived = new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.ChargeLinkCommandReceived
            {
                PublishedTime = chargeLinksReceivedEvent.PublishedTime.ToTimestamp().TruncateToSeconds(),
                ChargeLinksCommand = new Infrastructure.Internal.ChargeLinkCommandReceived.ChargeLinkCommand
                {
                    MeteringPointId = chargeLinksReceivedEvent.ChargeLinksCommand.MeteringPointId,
                    Document = ConvertDocument(chargeLinksReceivedEvent.ChargeLinksCommand.Document),
                },
            };

            foreach (var chargeLinkDto in chargeLinksReceivedEvent.ChargeLinksCommand.ChargeLinks)
            {
                chargeLinkCommandReceived.ChargeLinksCommand.ChargeLinks.Add(ConvertChargeLink(chargeLinkDto));
            }

            return chargeLinkCommandReceived;
        }

        private static Infrastructure.Internal.ChargeLinkCommandReceived.Document ConvertDocument(DocumentDto documentDto)
        {
            return new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.Document
            {
                Id = documentDto.Id,
                RequestDate = documentDto.RequestDate.ToTimestamp(),
                Type = (Infrastructure.Internal.ChargeLinkCommandReceived.DocumentType)documentDto.Type,
                CreatedDateTime = documentDto.CreatedDateTime.ToTimestamp(),
                Sender = new Infrastructure.Internal.ChargeLinkCommandReceived.MarketParticipant
                {
                    Id = documentDto.Sender.Id,
                    BusinessProcessRole = (Infrastructure.Internal.ChargeLinkCommandReceived.MarketParticipantRole)documentDto.Sender.BusinessProcessRole,
                },
                Recipient = new Infrastructure.Internal.ChargeLinkCommandReceived.MarketParticipant
                {
                    Id = documentDto.Recipient.Id,
                    BusinessProcessRole = (Infrastructure.Internal.ChargeLinkCommandReceived.MarketParticipantRole)documentDto.Recipient.BusinessProcessRole,
                },
                IndustryClassification = (Infrastructure.Internal.ChargeLinkCommandReceived.IndustryClassification)documentDto.IndustryClassification,
                BusinessReasonCode = (Infrastructure.Internal.ChargeLinkCommandReceived.BusinessReasonCode)documentDto.BusinessReasonCode,
            };
        }

        private static Infrastructure.Internal.ChargeLinkCommandReceived.ChargeLink ConvertChargeLink(ChargeLinkDto chargeLinkDto)
        {
            return new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived.ChargeLink
            {
                OperationId = chargeLinkDto.OperationId,
                SenderProvidedChargeId = chargeLinkDto.SenderProvidedChargeId,
                ChargeOwnerId = chargeLinkDto.ChargeOwnerId,
                Factor = chargeLinkDto.Factor,
                ChargeType = (Infrastructure.Internal.ChargeLinkCommandReceived.ChargeType)chargeLinkDto.ChargeType,
                StartDateTime = chargeLinkDto.StartDateTime.ToTimestamp(),
                EndDateTime = chargeLinkDto.EndDateTime?.ToTimestamp(),
            };
        }
    }
}
