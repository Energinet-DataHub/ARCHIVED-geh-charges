using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.ChangeOfCharges.Repositories;
using GreenEnergyHub.Charges.Application.Validation;

namespace GreenEnergyHub.Charges.Application
{
    /// <summary>
    /// TODO: Doc
    /// </summary>
    public interface IChargeCommandAcknowledgementService
    {
        /// <summary>
        /// Reject the change of charge command.
        /// </summary>
        /// <param name="validationResult"></param>
        /// <returns>TODO 2</returns>
        Task RejectAsync(ChargeCommandValidationResult validationResult);

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="validationResult"></param>
        /// <returns>TODO 2</returns>
        Task RejectAsync(ChargeStorageStatus validationResult);
    }
}
