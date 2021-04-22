using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application.Validation.BusinessValidation
{
    public interface IChargeCommandBusinessValidator
    {
        Task<ChargeCommandValidationResult> ValidateAsync(ChargeCommand command);
    }
}
