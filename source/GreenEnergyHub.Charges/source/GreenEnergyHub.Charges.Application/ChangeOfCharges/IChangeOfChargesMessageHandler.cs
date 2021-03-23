using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Message;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Result;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application.ChangeOfCharges
{
    /// <summary>
    /// Contract for handling a change of charges message.
    /// </summary>
    public interface IChangeOfChargesMessageHandler
    {
        /// <summary>
        /// Synchronously handle the message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns>Returns the result of the synchronous handling of the message.</returns>
        Task<ChangeOfChargesMessageResult> HandleAsync(ChangeOfChargesMessage message);
    }
}
