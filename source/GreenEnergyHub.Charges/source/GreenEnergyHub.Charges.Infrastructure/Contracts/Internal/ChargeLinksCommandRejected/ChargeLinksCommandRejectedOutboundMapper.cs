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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksRejectionEvents;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;

namespace GreenEnergyHub.Charges.Infrastructure.Contracts.Internal.ChargeLinksCommandRejected
{
    public class ChargeLinksCommandRejectedOutboundMapper : ProtobufOutboundMapper<ChargeLinksRejectedEvent>
    {
        protected override Google.Protobuf.IMessage Convert(ChargeLinksRejectedEvent chargeLinksRejectedEvent)
        {
            var chargeLinkCommandAccepted = new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinksCommandRejected.ChargeLinksCommandRejected
            {
                PublishedTime = chargeLinksRejectedEvent.PublishedTime.ToTimestamp(),
                Command = new Infrastructure.Internal.ChargeLinksCommandRejected.ChargeLinksCommand
                {
                    MeteringPointId = chargeLinksRejectedEvent.ChargeLinksCommand.MeteringPointId,
                    Document = ConvertDocument(chargeLinksRejectedEvent.ChargeLinksCommand.Document),
                },
            };

            foreach (var chargeLinkDto in chargeLinksRejectedEvent.ChargeLinksCommand.ChargeLinks)
            {
             chargeLinkCommandAccepted.Command.ChargeLinks.Add(ConvertChargeLink(chargeLinkDto));
            }

            AddRejectedReasons(chargeLinkCommandAccepted, chargeLinksRejectedEvent);

            return chargeLinkCommandAccepted;
        }

        private static void AddRejectedReasons(Infrastructure.Internal.ChargeLinksCommandRejected.ChargeLinksCommandRejected chargeCommandRejectedContract, ChargeLinksRejectedEvent rejectionEvent)
        {
            foreach (var reason in rejectionEvent.RejectReasons)
            {
                chargeCommandRejectedContract.RejectReasons.Add(reason);
            }
        }

        private static Infrastructure.Internal.ChargeLinksCommandRejected.Document ConvertDocument(DocumentDto documentDto)
        {
            return new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinksCommandRejected.Document
            {
                Id = documentDto.Id,
                RequestDate = documentDto.RequestDate.ToTimestamp(),
                Type = (Infrastructure.Internal.ChargeLinksCommandRejected.DocumentType)documentDto.Type,
                CreatedDateTime = documentDto.CreatedDateTime.ToTimestamp(),
                Sender = new Infrastructure.Internal.ChargeLinksCommandRejected.MarketParticipant
                {
                    Id = documentDto.Sender.Id,
                    BusinessProcessRole = (Infrastructure.Internal.ChargeLinksCommandRejected.MarketParticipantRole)documentDto.Sender.BusinessProcessRole,
                },
                Recipient = new Infrastructure.Internal.ChargeLinksCommandRejected.MarketParticipant
                {
                    Id = documentDto.Recipient.Id,
                    BusinessProcessRole = (Infrastructure.Internal.ChargeLinksCommandRejected.MarketParticipantRole)documentDto.Recipient.BusinessProcessRole,
                },
                IndustryClassification = (Infrastructure.Internal.ChargeLinksCommandRejected.IndustryClassification)documentDto.IndustryClassification,
                BusinessReasonCode = (Infrastructure.Internal.ChargeLinksCommandRejected.BusinessReasonCode)documentDto.BusinessReasonCode,
            };
        }

        private static Infrastructure.Internal.ChargeLinksCommandRejected.ChargeLink ConvertChargeLink(ChargeLinkDto chargeLink)
        {
            return new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinksCommandRejected.ChargeLink
            {
                OperationId = chargeLink.OperationId,
                SenderProvidedChargeId = chargeLink.SenderProvidedChargeId,
                ChargeOwnerId = chargeLink.ChargeOwnerId,
                Factor = chargeLink.Factor,
                ChargeType = (Infrastructure.Internal.ChargeLinksCommandRejected.ChargeType)chargeLink.ChargeType,
                StartDateTime = chargeLink.StartDateTime.ToTimestamp(),
                EndDateTime = chargeLink.EndDateTime.TimeOrEndDefault().ToTimestamp(),
            };
        }
    }
}
