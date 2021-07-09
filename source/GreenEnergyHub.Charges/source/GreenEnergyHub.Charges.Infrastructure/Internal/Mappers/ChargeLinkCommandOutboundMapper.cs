﻿using System;
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
                        Name = document.Sender.Name,
                        MarketParticipantRole = document.Sender.BusinessProcessRole.ToString(),
                    },
                    Recipient = new MarketParticipantDomain
                    {
                        Id = document.Recipient.Id,
                        Name = document.Recipient.Name,
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
