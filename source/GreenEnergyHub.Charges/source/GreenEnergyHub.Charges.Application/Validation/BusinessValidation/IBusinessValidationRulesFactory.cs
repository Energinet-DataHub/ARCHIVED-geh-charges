using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application.Validation.BusinessValidation
{
    /// <summary>
    /// TODO
    /// </summary>
    public interface IBusinessValidationRulesFactory
    {
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="command"></param>
        /// <returns>TODO 2</returns>
        Task<IBusinessValidationRuleSet> CreateRulesForChargeCommandAsync(ChargeCommand command);
    }
}
