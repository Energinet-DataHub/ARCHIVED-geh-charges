namespace GreenEnergyHub.DataHub.Charges.Libraries.Models
{
    public sealed record CreateDefaultChargeLinksDto(
        string meteringPointId,
        string correlationId);
}
