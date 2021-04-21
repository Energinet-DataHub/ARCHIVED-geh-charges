using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application.Validation.BusinessValidation
{
    public class ChargeCommandBusinessValidator : IChargeCommandBusinessValidator
    {
        private readonly IBusinessValidationRulesFactory _businessValidationRulesFactory;

        public ChargeCommandBusinessValidator(IBusinessValidationRulesFactory businessValidationRulesFactory)
        {
            _businessValidationRulesFactory = businessValidationRulesFactory;
        }

        public async Task<ChargeCommandValidationResult> ValidateAsync(ChargeCommand command)
        {
            var ruleSet = await _businessValidationRulesFactory.GetRulesForChargeCommandAsync(command).ConfigureAwait(false);
            return ruleSet.Validate();
        }
    }
}
