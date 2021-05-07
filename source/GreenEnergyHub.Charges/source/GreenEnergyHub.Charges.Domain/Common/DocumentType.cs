namespace GreenEnergyHub.Charges.Domain.Common
{
    /// <summary>
    /// The document type indicates the intended business context of this business message.
    /// </summary>
    public enum DocumentType
    {
        Unknown = 0,
        RequestUpdateChargeInformation = 10, // This will be received as D10 in ebiX.
    }
}
