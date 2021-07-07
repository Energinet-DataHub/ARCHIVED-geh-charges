using System;
using System.Globalization;
using Energinet.DataHub.ChargeLinks.InternalContracts;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Infrastructure.Transport.Protobuf;

namespace GreenEnergyHub.Charges.Infrastructure.Integration.Mappers
{
    public class ChargeLinkDomainOutboundMapper : ProtobufOutboundMapper<ChargeLink>
    {
        protected override Google.Protobuf.IMessage Convert(ChargeLink obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            return new ChargeLinkDomain
            {
                Id = obj.Id,
                MeteringPointId = obj.MeteringPointId,
                ChargeId = obj.ChargeId,
                ChargeOwner = obj.ChargeOwner,
                Factor = obj.Factor.ToString(CultureInfo.InvariantCulture),
                ChargeType = obj.ChargeType.ToString(),
                StartDateTime = obj.StartDateTime.ToString(),
                EndDateTime = obj.EndDateTime.ToString(),
            };
        }
    }
}
