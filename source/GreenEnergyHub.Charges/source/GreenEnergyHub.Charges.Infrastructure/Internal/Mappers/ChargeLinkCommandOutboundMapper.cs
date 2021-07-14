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
using System.Globalization;
using Energinet.DataHub.ChargeLinks.InternalContracts;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Messaging.Protobuf;

namespace GreenEnergyHub.Charges.Infrastructure.Internal.Mappers
{
    public class ChargeLinkDomainOutboundMapper : ProtobufOutboundMapper<ChargeLinkCommand>
    {
        protected override Google.Protobuf.IMessage Convert(ChargeLinkCommand obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var document = obj.Document;
            var chargeLink = obj.ChargeLink;

            return new ChargeLinkCommandDomain
            {
                Document = new DocumentDomain
                {
                    Id = document.Id,
                    RequestDate = document.RequestDate.ToString(),
                    Type = document.Type.ToString(),
                    CreatedDateTime = document.CreatedDateTime.ToString(),
                    Sender = new MarketParticipantDomain
                    {
                        Id = document.Sender.Id,
                        MarketParticipantRole = document.Sender.BusinessProcessRole.ToString(),
                    },
                    Recipient = new MarketParticipantDomain
                    {
                        Id = document.Recipient.Id,
                        MarketParticipantRole = document.Recipient.BusinessProcessRole.ToString(),
                    },
                    IndustryClassification = document.IndustryClassification.ToString(),
                    BusinessReasonCode = document.BusinessReasonCode.ToString(),
                },
                ChargeLink = new ChargeLinkDomain
                {
                    Id = obj.ChargeLink.Id,
                    MeteringPointId = chargeLink.MeteringPointId,
                    ChargeId = chargeLink.ChargeId,
                    ChargeOwner = chargeLink.ChargeOwner,
                    Factor = chargeLink.Factor.ToString(CultureInfo.InvariantCulture),
                    ChargeType = chargeLink.ChargeType.ToString(),
                    StartDateTime = chargeLink.StartDateTime.ToString(),
                    EndDateTime = chargeLink.EndDateTime.ToString(),
                },
                CorrelationId = obj.CorrelationId,
            };
        }
    }
}
