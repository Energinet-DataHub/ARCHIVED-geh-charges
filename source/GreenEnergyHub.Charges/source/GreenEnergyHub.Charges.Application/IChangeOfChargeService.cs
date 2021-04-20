using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application
{
    /// <summary>
    /// TODO
    /// </summary>
    public interface IChangeOfChargeService
    {
        /// <summary>
        ///  TODO
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns>TODO return</returns>
        Task HandleAsync(ChangeOfChargesTransaction transaction);
    }
}
