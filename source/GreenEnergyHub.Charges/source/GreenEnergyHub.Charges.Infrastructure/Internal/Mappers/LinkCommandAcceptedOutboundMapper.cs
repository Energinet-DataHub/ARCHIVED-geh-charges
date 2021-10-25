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
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommands;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted;
using GreenEnergyHub.Messaging.Protobuf;
using Document = GreenEnergyHub.Charges.Domain.MarketParticipants.Document;

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

        private static GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.Document ConvertDocument(Document document)
        {
            return new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.Document
            {
                Id = document.Id,
                RequestDate = document.RequestDate.ToTimestamp(),
                Type = (GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.DocumentType)document.Type,
                CreatedDateTime = document.CreatedDateTime.ToTimestamp(),
                Sender = new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.MarketParticipant
                {
                    Id = document.Sender.Id,
                    BusinessProcessRole = (GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.MarketParticipantRole)document.Sender.BusinessProcessRole,
                },
                Recipient = new GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.MarketParticipant
                {
                    Id = document.Recipient.Id,
                    BusinessProcessRole = (GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.MarketParticipantRole)document.Recipient.BusinessProcessRole,
                },
                IndustryClassification = (GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.IndustryClassification)document.IndustryClassification,
                BusinessReasonCode = (GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted.BusinessReasonCode)document.BusinessReasonCode,
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
