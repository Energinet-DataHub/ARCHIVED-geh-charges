using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.Validation;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

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
        /// <param name="command"></param>
        /// <returns>TODO 2</returns>
        Task AcceptAsync(ChargeCommand command);
    }
}
