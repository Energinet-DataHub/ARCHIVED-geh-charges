using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application
{
    public interface IChargeCommandHandler
    {
        Task HandleAsync(ChargeCommand command);
    }
}
