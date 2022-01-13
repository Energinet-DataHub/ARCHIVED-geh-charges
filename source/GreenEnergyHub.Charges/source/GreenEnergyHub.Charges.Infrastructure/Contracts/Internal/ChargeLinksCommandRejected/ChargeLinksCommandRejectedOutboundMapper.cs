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

using System.Linq;
using Energinet.DataHub.Core.Messaging.Protobuf;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksRejectionEvents;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinksCommandRejected;

namespace GreenEnergyHub.Charges.Infrastructure.Contracts.Internal.ChargeLinksCommandRejected
{
    public class ChargeLinksCommandRejectedOutboundMapper : ProtobufOutboundMapper<ChargeLinksRejectedEvent>
    {
        protected override Google.Protobuf.IMessage Convert(ChargeLinksRejectedEvent chargeLinksRejectedEvent)
        {
            var linksCommandRejected = new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinksCommandRejected.ChargeLinksCommandRejected
            {
                PublishedTime = chargeLinksRejectedEvent.PublishedTime.ToTimestamp(),
                ChargeLinksCommand = new Infrastructure.Internal.ChargeLinksCommandRejected.ChargeLinksCommand
                {
                    MeteringPointId = chargeLinksRejectedEvent.ChargeLinksCommand.MeteringPointId,
                    Document = ConvertDocument(chargeLinksRejectedEvent.ChargeLinksCommand.Document),
                },
            };

            foreach (var chargeLinkDto in chargeLinksRejectedEvent.ChargeLinksCommand.ChargeLinks)
            {
                linksCommandRejected.ChargeLinksCommand.ChargeLinks.Add(ConvertChargeLink(chargeLinkDto));
            }

            ConvertValidationErrors(linksCommandRejected, chargeLinksRejectedEvent);

            return linksCommandRejected;
        }

        private static void ConvertValidationErrors(
            GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinksCommandRejected.ChargeLinksCommandRejected chargeLinksCommandRejected,
            ChargeLinksRejectedEvent rejectionEvent)
        {
            chargeLinksCommandRejected.ValidationErrors.AddRange(
                rejectionEvent.ValidationErrors.Select(ve => new ValidationErrorContract
                {
                    ValidationRuleIdentifier = (ValidationRuleIdentifierContract)ve.ValidationRuleIdentifier,
                    ListElementWithValidationError = ve.ListElementWithValidationError,
                }));
        }

        private static Document ConvertDocument(DocumentDto documentDto)
        {
            return new Document
            {
                Id = documentDto.Id,
                RequestDate = documentDto.RequestDate.ToTimestamp(),
                Type = (DocumentType)documentDto.Type,
                CreatedDateTime = documentDto.CreatedDateTime.ToTimestamp(),
                Sender = new MarketParticipant
                {
                    Id = documentDto.Sender.Id,
                    BusinessProcessRole = (MarketParticipantRole)documentDto.Sender.BusinessProcessRole,
                },
                Recipient = new MarketParticipant
                {
                    Id = documentDto.Recipient.Id,
                    BusinessProcessRole = (MarketParticipantRole)documentDto.Recipient.BusinessProcessRole,
                },
                IndustryClassification = (IndustryClassification)documentDto.IndustryClassification,
                BusinessReasonCode = (BusinessReasonCode)documentDto.BusinessReasonCode,
            };
        }

        private static ChargeLink ConvertChargeLink(ChargeLinkDto chargeLink)
        {
            return new ChargeLink
            {
                OperationId = chargeLink.OperationId,
                SenderProvidedChargeId = chargeLink.SenderProvidedChargeId,
                ChargeOwnerId = chargeLink.ChargeOwnerId,
                Factor = chargeLink.Factor,
                ChargeType = (ChargeType)chargeLink.ChargeType,
                StartDateTime = chargeLink.StartDateTime.ToTimestamp(),
                EndDateTime = chargeLink.EndDateTime.TimeOrEndDefault().ToTimestamp(),
            };
        }
    }
}
