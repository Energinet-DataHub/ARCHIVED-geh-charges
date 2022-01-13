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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;

namespace GreenEnergyHub.Charges.Infrastructure.Contracts.Internal.ChargeLinkCommandAccepted
{
    public class ChargeLinksCommandAcceptedOutboundMapper : ProtobufOutboundMapper<ChargeLinksAcceptedEvent>
    {
        protected override Google.Protobuf.IMessage Convert(ChargeLinksAcceptedEvent chargeLinksAcceptedEvent)
        {
            var chargeLinkCommandAcceptedContract = new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinksCommandAccepted.ChargeLinksCommandAccepted
            {
                PublishedTime = chargeLinksAcceptedEvent.PublishedTime.ToTimestamp(),
                ChargeLinksCommand = new Infrastructure.Internal.ChargeLinksCommandAccepted.ChargeLinksCommand
                {
                    MeteringPointId = chargeLinksAcceptedEvent.ChargeLinksCommand.MeteringPointId,
                    Document = ConvertDocument(chargeLinksAcceptedEvent.ChargeLinksCommand.Document),
                },
            };

            foreach (var chargeLinkDto in chargeLinksAcceptedEvent.ChargeLinksCommand.ChargeLinks)
            {
             chargeLinkCommandAcceptedContract.ChargeLinksCommand.ChargeLinks.Add(ConvertChargeLink(chargeLinkDto));
            }

            return chargeLinkCommandAcceptedContract;
        }

        private static Infrastructure.Internal.ChargeLinksCommandAccepted.Document ConvertDocument(DocumentDto documentDto)
        {
            return new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinksCommandAccepted.Document
            {
                Id = documentDto.Id,
                RequestDate = documentDto.RequestDate.ToTimestamp(),
                Type = (Infrastructure.Internal.ChargeLinksCommandAccepted.DocumentType)documentDto.Type,
                CreatedDateTime = documentDto.CreatedDateTime.ToTimestamp(),
                Sender = new Infrastructure.Internal.ChargeLinksCommandAccepted.MarketParticipant
                {
                    Id = documentDto.Sender.Id,
                    BusinessProcessRole = (Infrastructure.Internal.ChargeLinksCommandAccepted.MarketParticipantRole)documentDto.Sender.BusinessProcessRole,
                },
                Recipient = new Infrastructure.Internal.ChargeLinksCommandAccepted.MarketParticipant
                {
                    Id = documentDto.Recipient.Id,
                    BusinessProcessRole = (Infrastructure.Internal.ChargeLinksCommandAccepted.MarketParticipantRole)documentDto.Recipient.BusinessProcessRole,
                },
                IndustryClassification = (Infrastructure.Internal.ChargeLinksCommandAccepted.IndustryClassification)documentDto.IndustryClassification,
                BusinessReasonCode = (Infrastructure.Internal.ChargeLinksCommandAccepted.BusinessReasonCode)documentDto.BusinessReasonCode,
            };
        }

        private static Infrastructure.Internal.ChargeLinksCommandAccepted.ChargeLink ConvertChargeLink(ChargeLinkDto chargeLink)
        {
            return new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinksCommandAccepted.ChargeLink
            {
                OperationId = chargeLink.OperationId,
                SenderProvidedChargeId = chargeLink.SenderProvidedChargeId,
                ChargeOwnerId = chargeLink.ChargeOwnerId,
                Factor = chargeLink.Factor,
                ChargeType = (Infrastructure.Internal.ChargeLinksCommandAccepted.ChargeType)chargeLink.ChargeType,
                StartDateTime = chargeLink.StartDateTime.ToTimestamp(),
                EndDateTime = chargeLink.EndDateTime.TimeOrEndDefault().ToTimestamp(),
            };
        }
    }
}
