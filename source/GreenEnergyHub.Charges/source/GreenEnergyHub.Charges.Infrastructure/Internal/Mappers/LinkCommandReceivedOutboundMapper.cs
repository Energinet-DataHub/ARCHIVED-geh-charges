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

using System;
using System.Diagnostics.CodeAnalysis;
using Google.Protobuf.WellKnownTypes;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.ChargeLinks.Events.Local;
using GreenEnergyHub.Charges.Domain.MarketDocument;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived;
using GreenEnergyHub.Messaging.Protobuf;
using NodaTime;

namespace GreenEnergyHub.Charges.Infrastructure.Internal.Mappers
{
    public class LinkCommandReceivedOutboundMapper : ProtobufOutboundMapper<ChargeLinkCommandReceivedEvent>
    {
        protected override Google.Protobuf.IMessage Convert([NotNull]ChargeLinkCommandReceivedEvent chargeLinkCommandReceivedEvent)
        {
            var document = chargeLinkCommandReceivedEvent.ChargeLinkCommand.Document;

            return new ChargeLinkCommandReceivedContract
            {
                PublishedTime = chargeLinkCommandReceivedEvent.PublishedTime.ToTimestamp().TruncateToSeconds(),
                ChargeLinkCommand = new ChargeLinkCommandContract
                {
                    Document = ConvertDocument(document),
                    ChargeLink = ConvertChargeLink(chargeLinkCommandReceivedEvent),
                    CorrelationId = chargeLinkCommandReceivedEvent.ChargeLinkCommand.CorrelationId,
                },
                CorrelationId = chargeLinkCommandReceivedEvent.CorrelationId,
            };
        }

        private static DocumentContract ConvertDocument(Document document)
        {
            return new DocumentContract
            {
                Id = document.Id,
                RequestDate = document.RequestDate.ToTimestamp(),
                Type = (DocumentTypeContract)document.Type,
                CreatedDateTime = document.CreatedDateTime.ToTimestamp(),
                Sender = new MarketParticipantContract
                {
                    Id = document.Sender.Id,
                    BusinessProcessRole = (MarketParticipantRoleContract)document.Sender.BusinessProcessRole,
                },
                Recipient = new MarketParticipantContract
                {
                    Id = document.Recipient.Id,
                    BusinessProcessRole = (MarketParticipantRoleContract)document.Recipient.BusinessProcessRole,
                },
                IndustryClassification = (IndustryClassificationContract)document.IndustryClassification,
                BusinessReasonCode = (BusinessReasonCodeContract)document.BusinessReasonCode,
            };
        }

        private static ChargeLinkContract ConvertChargeLink(ChargeLinkCommandReceivedEvent chargeLinkCommandReceivedEvent)
        {
            var chargeLink = chargeLinkCommandReceivedEvent.ChargeLinkCommand.ChargeLink;
            return new ChargeLinkContract
            {
                OperationId = chargeLink.OperationId,
                MeteringPointId = chargeLink.MeteringPointId,
                ChargeId = chargeLink.ChargeId,
                ChargeOwner = chargeLink.ChargeOwner,
                Factor = chargeLink.Factor,
                ChargeType = (ChargeTypeContract)chargeLink.ChargeType,
                StartDateTime = chargeLink.StartDateTime.ToTimestamp(),
                EndDateTime = chargeLink.EndDateTime.TimeOrEndDefault().ToTimestamp(),
            };
        }
    }
}
