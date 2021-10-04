﻿// Copyright 2020 Energinet DataHub A/S
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
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted;
using GreenEnergyHub.Messaging.Protobuf;

namespace GreenEnergyHub.Charges.Infrastructure.Internal.Mappers
{
    public class LinkCommandAcceptedOutboundMapper : ProtobufOutboundMapper<ChargeLinkCommandAcceptedEvent>
    {
        protected override Google.Protobuf.IMessage Convert([NotNull]ChargeLinkCommandAcceptedEvent chargeLinkCommandAcceptedEvent)
        {
            var document = chargeLinkCommandAcceptedEvent.Document;
            var chargeLink = chargeLinkCommandAcceptedEvent.ChargeLink;

            return new ChargeLinkCommandAcceptedContract
            {
                Document = ConvertDocument(document),
                ChargeLink = ConvertChargeLink(chargeLinkCommandAcceptedEvent, chargeLink),
                CorrelationId = chargeLinkCommandAcceptedEvent.CorrelationId,
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

        private static ChargeLinkContract ConvertChargeLink(ChargeLinkCommandAcceptedEvent chargeLinkCommandAcceptedEvent, ChargeLinkDto chargeLink)
        {
            return new ChargeLinkContract
            {
                OperationId = chargeLinkCommandAcceptedEvent.ChargeLink.OperationId,
                MeteringPointId = chargeLink.MeteringPointId,
                ChargeId = chargeLink.SenderProvidedChargeId,
                ChargeOwner = chargeLink.ChargeOwner,
                Factor = chargeLink.Factor,
                ChargeType = (ChargeTypeContract)chargeLink.ChargeType,
                StartDateTime = chargeLink.StartDateTime.ToTimestamp(),
                EndDateTime = chargeLink.EndDateTime.TimeOrEndDefault().ToTimestamp(),
            };
        }
    }
}
