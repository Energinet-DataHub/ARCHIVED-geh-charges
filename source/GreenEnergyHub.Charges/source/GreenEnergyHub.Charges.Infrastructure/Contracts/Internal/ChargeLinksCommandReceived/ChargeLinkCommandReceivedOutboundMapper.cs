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

using Energinet.DataHub.Core.Messaging.Protobuf;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;

namespace GreenEnergyHub.Charges.Infrastructure.Contracts.Internal.ChargeLinksCommandReceived
{
    public class ChargeLinksCommandReceivedOutboundMapper : ProtobufOutboundMapper<ChargeLinksReceivedEvent>
    {
        protected override Google.Protobuf.IMessage Convert(ChargeLinksReceivedEvent chargeLinksReceivedEvent)
        {
            var chargeLinkCommandReceived = new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinksCommandReceived.ChargeLinksCommandReceived
            {
                PublishedTime = chargeLinksReceivedEvent.PublishedTime.ToTimestamp().TruncateToSeconds(),
                ChargeLinksCommand = new Infrastructure.Internal.ChargeLinksCommandReceived.ChargeLinksCommand
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

        private static Infrastructure.Internal.ChargeLinksCommandReceived.Document ConvertDocument(DocumentDto documentDto)
        {
            return new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinksCommandReceived.Document
            {
                Id = documentDto.Id,
                RequestDate = documentDto.RequestDate.ToTimestamp(),
                Type = (Infrastructure.Internal.ChargeLinksCommandReceived.DocumentType)documentDto.Type,
                CreatedDateTime = documentDto.CreatedDateTime.ToTimestamp(),
                Sender = new Infrastructure.Internal.ChargeLinksCommandReceived.MarketParticipant
                {
                    Id = documentDto.Sender.Id,
                    BusinessProcessRole = (Infrastructure.Internal.ChargeLinksCommandReceived.MarketParticipantRole)documentDto.Sender.BusinessProcessRole,
                },
                Recipient = new Infrastructure.Internal.ChargeLinksCommandReceived.MarketParticipant
                {
                    Id = documentDto.Recipient.Id,
                    BusinessProcessRole = (Infrastructure.Internal.ChargeLinksCommandReceived.MarketParticipantRole)documentDto.Recipient.BusinessProcessRole,
                },
                IndustryClassification = (Infrastructure.Internal.ChargeLinksCommandReceived.IndustryClassification)documentDto.IndustryClassification,
                BusinessReasonCode = (Infrastructure.Internal.ChargeLinksCommandReceived.BusinessReasonCode)documentDto.BusinessReasonCode,
            };
        }

        private static Infrastructure.Internal.ChargeLinksCommandReceived.ChargeLink ConvertChargeLink(ChargeLinkDto chargeLinkDto)
        {
            return new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinksCommandReceived.ChargeLink
            {
                OperationId = chargeLinkDto.OperationId,
                SenderProvidedChargeId = chargeLinkDto.SenderProvidedChargeId,
                ChargeOwnerId = chargeLinkDto.ChargeOwnerId,
                Factor = chargeLinkDto.Factor,
                ChargeType = (Infrastructure.Internal.ChargeLinksCommandReceived.ChargeType)chargeLinkDto.ChargeType,
                StartDateTime = chargeLinkDto.StartDateTime.ToTimestamp(),
                EndDateTime = chargeLinkDto.EndDateTime?.ToTimestamp(),
            };
        }
    }
}
