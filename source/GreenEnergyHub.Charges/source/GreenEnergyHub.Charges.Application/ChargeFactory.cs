using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application
{
    public class ChargeFactory : IChargeFactory
    {
        public Task<Charge> CreateFromCommandAsync(ChargeCommand command)
        {
            // Quick and dirty as long as Charge is a command
            return Task.FromResult((Charge)command);
        }
    }
}
