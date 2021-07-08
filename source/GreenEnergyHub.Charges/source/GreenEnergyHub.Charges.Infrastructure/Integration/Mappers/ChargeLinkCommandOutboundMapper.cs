using System;
using System.Globalization;
using Energinet.DataHub.ChargeLinks.InternalContracts;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Infrastructure.Transport.Protobuf;

namespace GreenEnergyHub.Charges.Infrastructure.Integration.Mappers
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
                    Sender = document.Sender.ToString(),
                    Recipient = document.Recipient.ToString(),
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
            };
        }
    }
}
