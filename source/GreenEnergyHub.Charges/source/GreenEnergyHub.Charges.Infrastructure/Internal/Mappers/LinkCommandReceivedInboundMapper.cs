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
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.ChargeLinks.Command;
using GreenEnergyHub.Charges.Domain.ChargeLinks.Events.Local;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MarketDocument;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived;
using GreenEnergyHub.Messaging.MessageTypes.Common;
using GreenEnergyHub.Messaging.Protobuf;
using GreenEnergyHub.Messaging.Transport;
using NodaTime;
using MarketParticipant = GreenEnergyHub.Charges.Domain.MarketDocument.MarketParticipant;

namespace GreenEnergyHub.Charges.Infrastructure.Internal.Mappers
{
    public class LinkCommandReceivedInboundMapper : ProtobufInboundMapper<ChargeLinkCommandReceivedContract>
    {
        private readonly IClock _clock;

        public LinkCommandReceivedInboundMapper(IClock clock)
        {
            _clock = clock;
        }

        protected override IInboundMessage Convert([NotNull]ChargeLinkCommandReceivedContract chargeLinkCommandReceivedContract)
        {
            return new ChargeLinkCommandReceivedEvent(
                _clock.GetCurrentInstant(),
                chargeLinkCommandReceivedContract.CorrelationId,
                new ChargeLinkCommand(chargeLinkCommandReceivedContract.CorrelationId)
                {
                    Document = ConvertDocument(chargeLinkCommandReceivedContract.Document),
                    ChargeLink = ConvertChargeLink(chargeLinkCommandReceivedContract.ChargeLink),
                    Transaction = Transaction.NewTransaction(),
                });
        }

        private static Document ConvertDocument(DocumentContract document)
        {
            return new Document
            {
                Id = document.Id,
                RequestDate = document.RequestDate.ToInstant(),
                Type = (DocumentType)document.Type,
                CreatedDateTime = document.CreatedDateTime.ToInstant(),
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

        private static ChargeLink ConvertChargeLink(ChargeLinkContract chargeLink)
        {
            return new ChargeLink
            {
                Id = chargeLink.Id,
                MeteringPointId = chargeLink.MeteringPointId,
                ChargeId = chargeLink.ChargeId,
                ChargeOwner = chargeLink.ChargeOwner,
                Factor = chargeLink.Factor,
                ChargeType = (ChargeType)chargeLink.ChargeType,
                StartDateTime = chargeLink.StartDateTime.ToInstant(),
                EndDateTime = chargeLink.EndDateTime.ToInstant(),
            };
        }
    }
}
