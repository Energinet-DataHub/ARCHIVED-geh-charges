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
                Document = new DocumentContract
                {
                        Id = document.Id,
                        RequestDate = Timestamp.FromDateTime(document.RequestDate.ToDateTimeUtc()),
                        Type = (DocumentTypeContract)document.Type,
                        CreatedDateTime = Timestamp.FromDateTime(document.CreatedDateTime.ToDateTimeUtc()),
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
                },
                ChargeLink = new ChargeLinkContract
                {
                    Id = chargeLinkCommandAcceptedEvent.ChargeLink.Id,
                    MeteringPointId = chargeLink.MeteringPointId,
                    ChargeId = chargeLink.ChargeId,
                    ChargeOwner = chargeLink.ChargeOwner,
                    Factor = chargeLink.Factor,
                    ChargeType = (ChargeTypeContract)chargeLink.ChargeType,
                    StartDateTime = Timestamp.FromDateTime(chargeLink.StartDateTime.ToDateTimeUtc()),
                    EndDateTime = Timestamp.FromDateTime(chargeLink.EndDateTime.TimeOrEndDefault().ToDateTimeUtc()),
                },
                CorrelationId = chargeLinkCommandAcceptedEvent.CorrelationId,
            };
        }
    }
}
