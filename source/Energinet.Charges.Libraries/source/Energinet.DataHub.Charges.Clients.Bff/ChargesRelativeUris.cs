using System;

namespace Energinet.DataHub.Charges.Clients.Bff
{
    public static class ChargesRelativeUris
    {
        public static Uri GetChargeLinksByMeteringPointId(string meteringPointId)
        {
            return new Uri($"ChargeLinks/GetChargeLinksByMeteringPointIdAsync/?meteringPointId={meteringPointId}", UriKind.Relative);
        }
    }
}
