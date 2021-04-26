using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.ChangeOfCharges.Repositories;
using GreenEnergyHub.Charges.Application.Validation.BusinessValidation.Rules;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;

namespace GreenEnergyHub.Charges.Application.Validation.BusinessValidation
{
    public class BusinessAdditionValidationRulesFactory : IBusinessAdditionValidationRulesFactory
    {
        private readonly IUpdateRulesConfigurationRepository _updateRulesConfigurationRepository;
        private readonly IChargeRepository _chargeRepository;

        public BusinessAdditionValidationRulesFactory(
            IUpdateRulesConfigurationRepository updateRulesConfigurationRepository,
            IChargeRepository chargeRepository)
        {
            _updateRulesConfigurationRepository = updateRulesConfigurationRepository;
            _chargeRepository = chargeRepository;
        }

        public async Task<IBusinessValidationRuleSet> CreateRulesForAdditionCommandAsync([NotNull] ChargeCommand command)
        {
            await CheckIfChargeExistAsync(command).ConfigureAwait(false);
            var configuration = await _updateRulesConfigurationRepository.GetConfigurationAsync().ConfigureAwait(false);

            var rules = GetRules(command, configuration);

            return BusinessValidationRuleSet.FromRules(rules);
        }

        private static List<IBusinessValidationRule> GetRules(ChargeCommand command, UpdateRulesConfiguration configuration)
        {
            var rules = new List<IBusinessValidationRule>
            {
                new StartDateVr209ValidationRule(
                    command,
                    configuration.StartDateVr209ValidationRuleConfiguration),
            };

            return rules;
        }

        private async Task CheckIfChargeExistAsync(ChargeCommand command)
        {
            var commandMRid = command.MRid!;
            var commandChargeTypeOwnerMRid = command.ChargeTypeOwnerMRid!;

            var charge = await _chargeRepository.GetChargeAsync(commandMRid, commandChargeTypeOwnerMRid).ConfigureAwait(false);
            if (charge == null) return;

            throw new Exception($"Charge found on MRid: {commandMRid}, ChargeTypeOwnerMRid: {commandChargeTypeOwnerMRid}");
        }
    }
}
