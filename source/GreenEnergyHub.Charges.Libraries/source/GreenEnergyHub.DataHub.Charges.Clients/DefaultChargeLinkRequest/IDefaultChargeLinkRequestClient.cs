using System.Threading.Tasks;

namespace GreenEnergyHub.DataHub.Charges.Libraries.DefaultChargeLinkRequest
{
    public interface IDefaultChargeLinkRequestClient
    {
        Task CreateDefaultChargeLinksRequestAsync(
            string meteringPointId,
            string correlationId);

        ValueTask DisposeAsync();
    }
}
