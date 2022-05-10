// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.ChargeInformation;
using GreenEnergyHub.Charges.Domain.Common;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.Factories
{
    public class ChargeCommandBusinessValidationRulesFactory : IBusinessValidationRulesFactory<ChargeCommand>
    {
        private readonly IChargeRepository _chargeRepository;
        private readonly IClock _clock;
        private readonly IRulesConfigurationRepository _rulesConfigurationRepository;
        private readonly IZonedDateTimeService _zonedDateTimeService;

        public ChargeCommandBusinessValidationRulesFactory(
            IRulesConfigurationRepository rulesConfigurationRepository,
            IChargeRepository chargeRepository,
            IZonedDateTimeService zonedDateTimeService,
            IClock clock)
        {
            _rulesConfigurationRepository = rulesConfigurationRepository;
            _chargeRepository = chargeRepository;
            _zonedDateTimeService = zonedDateTimeService;
            _clock = clock;
        }

        public async Task<IValidationRuleSet> CreateRulesAsync(ChargeCommand chargeCommand)
        {
            ArgumentNullException.ThrowIfNull(chargeCommand);
            var chargeOperation = chargeCommand.ChargeOperations.SingleOrDefault();
            ArgumentNullException.ThrowIfNull(chargeOperation);

            var rules = await GetRulesForOperationAsync(chargeOperation).ConfigureAwait(false);
            return ValidationRuleSet.FromRules(rules);
        }

        private async Task<List<IValidationRule>> GetRulesForOperationAsync(ChargeOperationDto chargeOperationDto)
        {
            var configuration = await _rulesConfigurationRepository.GetConfigurationAsync().ConfigureAwait(false);
            var charge = await GetChargeOrNullAsync(chargeOperationDto).ConfigureAwait(false);
            var rules = GetMandatoryRulesForOperation(chargeOperationDto, configuration);
            if (charge == null)
            {
                return rules;
            }

            if (chargeOperationDto.Type == ChargeType.Tariff)
            {
                rules.AddRange(AddTariffOnlyRules(chargeOperationDto, charge));
            }

            AddUpdateRules(rules, chargeOperationDto, charge);
            return rules;
        }

        private static IEnumerable<IValidationRule> AddTariffOnlyRules(
            ChargeOperationDto chargeOperationDto, Charge charge)
        {
            return new List<IValidationRule> { new ChangingTariffTaxValueNotAllowedRule(chargeOperationDto, charge) };
        }

        private void AddUpdateRules(
            List<IValidationRule> rules,
            ChargeOperationDto chargeOperationDto,
            Charge existingCharge)
        {
            var updateRules = new List<IValidationRule>
            {
                new UpdateChargeMustHaveEffectiveDateBeforeOrOnStopDateRule(existingCharge, chargeOperationDto),
                new ChargeResolutionCanNotBeUpdatedRule(existingCharge, chargeOperationDto),
            };

            rules.AddRange(updateRules);
        }

        private List<IValidationRule> GetMandatoryRulesForOperation(
            ChargeOperationDto chargeOperationDto,
            RulesConfiguration configuration)
        {
            var rules = new List<IValidationRule>
            {
                new StartDateValidationRule(
                    chargeOperationDto,
                    configuration.StartDateValidationRuleConfiguration,
                    _zonedDateTimeService,
                    _clock),
            };

            return rules;
        }

        private Task<Charge?> GetChargeOrNullAsync(ChargeOperationDto chargeOperationDto)
        {
            var chargeIdentifier = new ChargeIdentifier(
                chargeOperationDto.ChargeId,
                chargeOperationDto.ChargeOwner,
                chargeOperationDto.Type);

            return _chargeRepository.GetOrNullAsync(chargeIdentifier);
        }
    }
}
