using System;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application.Validation.BusinessValidation
{
    /// <summary>
    /// TODO: Implement the following business rules
    /// System validation: Netvirksomhed er legitim aktør
    /// VR-209 StartDate must be in (configurable?) date interval
    /// VR-??? TAX indicator must not change in an update
    /// VR-630 VAT payer must not change in an update
    /// </summary>
    public class BusinessValidationRulesFactory : IBusinessValidationRulesFactory
    {
        private readonly IBusinessUpdateValidationRulesFactory _businessUpdateValidationRulesFactory;

        public BusinessValidationRulesFactory(IBusinessUpdateValidationRulesFactory businessUpdateValidationRulesFactory)
        {
            _businessUpdateValidationRulesFactory = businessUpdateValidationRulesFactory;
        }

        public async Task<IBusinessValidationRuleSet> CreateRulesForChargeCommandAsync(ChargeCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            return command.MktActivityRecord!.Status switch
            {
                MktActivityRecordStatus.Change => await _businessUpdateValidationRulesFactory.CreateRulesForUpdateCommandAsync(command).ConfigureAwait(false),
                _ => throw new NotImplementedException("Unknown operation"),
            };
        }
    }
}
