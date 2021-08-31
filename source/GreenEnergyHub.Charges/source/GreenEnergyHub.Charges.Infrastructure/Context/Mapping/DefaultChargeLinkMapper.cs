using System.Diagnostics.CodeAnalysis;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Infrastructure.Context.Model;
using NodaTime;

namespace GreenEnergyHub.Charges.Infrastructure.Context.Mapping
{
    public static class DefaultChargeLinkMapper
    {
        public static DefaultChargeLink Map(
            Instant meteringPointCreatedDateTime,
            [NotNull]DefaultChargeLinkSetting defaultChargeLinkSettings)
        {
            var endDateTime = defaultChargeLinkSettings.EndDateTime != null ?
                Instant.FromDateTimeUtc(defaultChargeLinkSettings.EndDateTime.Value.ToUniversalTime()) :
                (Instant?)null;

            return new DefaultChargeLink(
                meteringPointCreatedDateTime,
                Instant.FromDateTimeUtc(defaultChargeLinkSettings.StartDateTime.ToUniversalTime()),
                endDateTime,
                defaultChargeLinkSettings.ChargeRowId);
        }
    }
}
