using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application
{
    public interface IChargeFactory
    {
        Task<Charge> CreateFromCommandAsync(ChargeCommand command);
    }
}
