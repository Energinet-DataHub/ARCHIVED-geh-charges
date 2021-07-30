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
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.MarketDocument;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived;
using GreenEnergyHub.Messaging.Protobuf;
using GreenEnergyHub.Messaging.Transport;
using NodaTime;

namespace GreenEnergyHub.Charges.Infrastructure.Internal.Mappers
{
    public class LinkCommandReceivedInboundMapper : ProtobufInboundMapper<ChargeLinkCommandReceivedContract>
    {
        protected override IInboundMessage Convert([NotNull]ChargeLinkCommandReceivedContract chargeLinkCommandReceivedContract)
        {
            var document = chargeLinkCommandReceivedContract.Document;
            var chargeLink = chargeLinkCommandReceivedContract.ChargeLink;

            return new ChargeLinkCommandReceivedEvent(chargeLinkCommandReceivedContract.CorrelationId)
            {
                Document = ConvertDocument(document),
                ChargeLink = ConvertChargeLink(chargeLinkCommandReceivedContract, chargeLink),
            };
        }

        private static Document ConvertDocument(DocumentContract document)
        {
            return new Document
            {
                Id = document.Id,
                RequestDate = Instant.FromUnixTimeSeconds(document.RequestDate.Seconds),
                Type = (DocumentType)document.Type,
                CreatedDateTime = Instant.FromUnixTimeSeconds(document.CreatedDateTime.Seconds),
                Sender = new MarketParticipant
                {
                    Id = document.Sender.Id,
                    BusinessProcessRole = (MarketParticipantRole)document.Sender.BusinessProcessRole,
                },
                Recipient = new MarketParticipant
                {
                    Id = document.Recipient.Id,
                    BusinessProcessRole = (MarketParticipantRole)document.Recipient.BusinessProcessRole,
                },
                IndustryClassification = (IndustryClassification)document.IndustryClassification,
                BusinessReasonCode = (BusinessReasonCode)document.BusinessReasonCode,
            };
        }

        private static ChargeLink ConvertChargeLink(ChargeLinkCommandReceivedContract chargeLinkCommandReceivedContract, ChargeLinkContract chargeLink)
        {
            return new ChargeLink
            {
                Id = chargeLinkCommandReceivedContract.ChargeLink.Id,
                MeteringPointId = chargeLink.MeteringPointId,
                ChargeId = chargeLink.ChargeId,
                ChargeOwner = chargeLink.ChargeOwner,
                Factor = chargeLink.Factor,
                ChargeType = (ChargeType)chargeLink.ChargeType,
                StartDateTime = Instant.FromUnixTimeSeconds(chargeLink.StartDateTime.Seconds),
                EndDateTime = Instant.FromUnixTimeSeconds(chargeLink.EndDateTime.Seconds),
            };
        }
    }
}
