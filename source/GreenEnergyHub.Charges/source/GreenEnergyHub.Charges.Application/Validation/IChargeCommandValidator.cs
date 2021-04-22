using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application.Validation
{
    public interface IChargeCommandValidator
    {
        Task<ChargeCommandValidationResult> ValidateAsync(ChargeCommand command);
    }
}
