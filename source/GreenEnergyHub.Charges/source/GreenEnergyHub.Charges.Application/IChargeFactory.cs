using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application
{
    /// <summary>
    /// TODO
    /// </summary>
    public interface IChargeFactory
    {
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="command"></param>
        /// <returns>TODO 2</returns>
        Task<Charge> CreateFromCommandAsync(ChargeCommand command);
    }
}
