using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application.Validation
{
    /// <summary>
    /// TODO
    /// </summary>
    public interface IChargeCommandValidator
    {
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="command"></param>
        /// <returns>TODO 2</returns>
        Task<ChargeCommandValidationResult> ValidateAsync(ChargeCommand command);
    }
}
