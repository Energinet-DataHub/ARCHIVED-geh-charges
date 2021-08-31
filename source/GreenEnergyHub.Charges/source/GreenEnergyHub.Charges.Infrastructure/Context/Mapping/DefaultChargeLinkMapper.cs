using System.Diagnostics.CodeAnalysis;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Infrastructure.Context.Model;
using NodaTime;

namespace GreenEnergyHub.Charges.Infrastructure.Context.Mapping
{
    public static class DefaultChargeLinkMapper
    {
        public static DefaultChargeLink Map([NotNull]DefaultChargeLinkSetting defaultChargeLinkSettings)
        {
            return new DefaultChargeLink
            {
                ApplicableDate = Instant.FromDateTimeUtc(defaultChargeLinkSettings.StartDateTime.ToUniversalTime()),
                EndDate = defaultChargeLinkSettings.EndDateTime != null ?
                    Instant.FromDateTimeUtc(defaultChargeLinkSettings.EndDateTime.Value.ToUniversalTime()) :
                    Instant.MinValue,
                ChargeRowId = defaultChargeLinkSettings.ChargeRowId,
            };
        }
    }
}
