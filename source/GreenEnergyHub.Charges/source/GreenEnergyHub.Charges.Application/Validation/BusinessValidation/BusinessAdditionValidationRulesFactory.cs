using System.Collections.Generic;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application.Validation.BusinessValidation
{
    public class BusinessAdditionValidationRulesFactory : IBusinessAdditionValidationRulesFactory
    {
        public Task<IBusinessValidationRuleSet> CreateRulesForAdditionCommandAsync(ChargeCommand command)
        {
            return Task.FromResult(BusinessValidationRuleSet.FromRules(new List<IBusinessValidationRule>()));
        }
    }
}
