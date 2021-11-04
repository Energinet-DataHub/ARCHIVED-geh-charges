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
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommands;
using GreenEnergyHub.Charges.Domain.SharedDtos;
using GreenEnergyHub.Messaging.Protobuf;

namespace GreenEnergyHub.Charges.Infrastructure.Internal.Mappers
{
    public class LinkCommandAcceptedOutboundMapper : ProtobufOutboundMapper<ChargeLinkCommandAcceptedEvent>
    {
        protected override Google.Protobuf.IMessage Convert([NotNull]ChargeLinkCommandAcceptedEvent chargeLinkCommandAcceptedEvent)
        {
            var chargeLinkCommandAcceptedContract = new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.ChargeLinkCommandAccepted()
            {
                CorrelationId = chargeLinkCommandAcceptedEvent.CorrelationId,
                PublishedTime = chargeLinkCommandAcceptedEvent.PublishedTime.ToTimestamp(),
            };

            foreach (var chargeLinkCommand in chargeLinkCommandAcceptedEvent.ChargeLinkCommands)
            {
             chargeLinkCommandAcceptedContract.ChargeLinkCommands.Add(new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.ChargeLinkCommand
             {
                 Document = ConvertDocument(chargeLinkCommand.Document),
                 ChargeLink = ConvertChargeLink(chargeLinkCommand.ChargeLink),
                 CorrelationId = chargeLinkCommand.CorrelationId,
             });
            }

            return chargeLinkCommandAcceptedContract;
        }

        private static GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.Document ConvertDocument(DocumentDto documentDto)
        {
            return new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.Document
            {
                Id = documentDto.Id,
                RequestDate = documentDto.RequestDate.ToTimestamp(),
                Type = (GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.DocumentType)documentDto.Type,
                CreatedDateTime = documentDto.CreatedDateTime.ToTimestamp(),
                Sender = new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.MarketParticipant
                {
                    Id = documentDto.Sender.Id,
                    BusinessProcessRole = (GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.MarketParticipantRole)documentDto.Sender.BusinessProcessRole,
                },
                Recipient = new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.MarketParticipant
                {
                    Id = documentDto.Recipient.Id,
                    BusinessProcessRole = (GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.MarketParticipantRole)documentDto.Recipient.BusinessProcessRole,
                },
                IndustryClassification = (GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.IndustryClassification)documentDto.IndustryClassification,
                BusinessReasonCode = (GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.BusinessReasonCode)documentDto.BusinessReasonCode,
            };
        }

        private static GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.ChargeLink ConvertChargeLink(ChargeLinkDto chargeLink)
        {
            return new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.ChargeLink
            {
                OperationId = chargeLink.OperationId,
                MeteringPointId = chargeLink.MeteringPointId,
                SenderProvidedChargeId = chargeLink.SenderProvidedChargeId,
                ChargeOwner = chargeLink.ChargeOwner,
                Factor = chargeLink.Factor,
                ChargeType = (GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.ChargeType)chargeLink.ChargeType,
                StartDateTime = chargeLink.StartDateTime.ToTimestamp(),
                EndDateTime = chargeLink.EndDateTime.TimeOrEndDefault().ToTimestamp(),
            };
        }
    }
}
