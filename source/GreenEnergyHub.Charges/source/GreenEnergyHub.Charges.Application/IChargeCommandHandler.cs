using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application
{
    /// <summary>
    /// TODO
    /// </summary>
    public interface IChargeCommandHandler
    {
        /// <summary>
        ///  TODO
        /// </summary>
        /// <param name="command"></param>
        /// <returns>TODO return</returns>
        Task HandleAsync(ChargeCommand command);
    }
}
