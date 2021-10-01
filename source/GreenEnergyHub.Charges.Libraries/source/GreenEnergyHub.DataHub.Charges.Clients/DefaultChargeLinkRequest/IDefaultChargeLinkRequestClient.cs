using System.Threading.Tasks;

namespace GreenEnergyHub.DataHub.Charges.Libraries.DefaultChargeLinkRequest
{
    public interface IDefaultChargeLinkRequestClient
    {
        /// <summary>
        /// Calling this method with required parameters will request the Charges domain to create
        /// Default charge links based on the supplied meteringPointId's MeteringPointType.
        /// </summary>
        /// <param name="meteringPointId"></param>
        /// <param name="correlationId"></param>
        Task CreateDefaultChargeLinksRequestAsync(
            string meteringPointId,
            string correlationId);

        ValueTask DisposeAsync();
    }
}
