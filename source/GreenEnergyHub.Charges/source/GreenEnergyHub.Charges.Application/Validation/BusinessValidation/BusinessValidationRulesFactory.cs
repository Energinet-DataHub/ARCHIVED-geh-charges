using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.ChangeOfCharges.Repositories;
using GreenEnergyHub.Charges.Application.Validation.BusinessValidation.Rules;
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
        private readonly IChargeRepository _chargeRepository;

        public BusinessValidationRulesFactory(IChargeRepository chargeRepository)
        {
            _chargeRepository = chargeRepository;
        }

        public async Task<IBusinessValidationRuleSet> GetRulesForChargeCommandAsync(ChargeCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            return command.MktActivityRecord!.Status switch
            {
                MktActivityRecordStatus.Change => await GetRulesForUpdateCommandAsync(command).ConfigureAwait(false),
                _ => throw new NotImplementedException(),
            };
        }

        private async Task<IBusinessValidationRuleSet> GetRulesForUpdateCommandAsync(ChargeCommand command)
        {
            var charge = await _chargeRepository.GetChargeAsync().ConfigureAwait(false);
            var rules = new List<IBusinessValidationRule>();

            // TODO magic string
            if (command.Type == "D03") rules.Add(new VatPayerMustNotChangeInUpdateRule(command, charge));

            return BusinessValidationRuleSet.FromRules(rules);
        }
    }
}
