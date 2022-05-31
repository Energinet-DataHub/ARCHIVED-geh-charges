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
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.Factories
{
    public class ChargeOperationBusinessValidationRulesFactory : IBusinessValidationRulesFactory<ChargeInformationDto>
    {
        private readonly IChargeRepository _chargeRepository;
        private readonly IChargeIdentifierFactory _chargeIdentifierFactory;
        private readonly IClock _clock;
        private readonly IRulesConfigurationRepository _rulesConfigurationRepository;
        private readonly IZonedDateTimeService _zonedDateTimeService;

        public ChargeOperationBusinessValidationRulesFactory(
            IRulesConfigurationRepository rulesConfigurationRepository,
            IChargeRepository chargeRepository,
            IChargeIdentifierFactory chargeIdentifierFactory,
            IZonedDateTimeService zonedDateTimeService,
            IClock clock)
        {
            _rulesConfigurationRepository = rulesConfigurationRepository;
            _chargeRepository = chargeRepository;
            _chargeIdentifierFactory = chargeIdentifierFactory;
            _zonedDateTimeService = zonedDateTimeService;
            _clock = clock;
        }

        public async Task<IValidationRuleSet> CreateRulesAsync(ChargeInformationDto informationDto)
        {
            ArgumentNullException.ThrowIfNull(informationDto);
            var rules = await GetRulesForOperationAsync(informationDto).ConfigureAwait(false);
            return ValidationRuleSet.FromRules(rules);
        }

        private async Task<List<IValidationRuleContainer>> GetRulesForOperationAsync(ChargeInformationDto chargeInformationDto)
        {
            var configuration = await _rulesConfigurationRepository.GetConfigurationAsync().ConfigureAwait(false);
            var charge = await GetChargeOrNullAsync(chargeInformationDto).ConfigureAwait(false);
            var rules = GetMandatoryRulesForOperation(chargeInformationDto, configuration);
            if (charge == null)
            {
                return rules;
            }

            if (chargeInformationDto.Type == ChargeType.Tariff)
            {
                rules.AddRange(AddTariffOnlyRules(chargeInformationDto, charge));
            }

            AddUpdateRules(rules, chargeInformationDto, charge);
            return rules;
        }

        private static IEnumerable<IValidationRuleContainer> AddTariffOnlyRules(
            ChargeInformationDto chargeInformationDto, Charge charge)
        {
            return new List<IValidationRuleContainer>
            {
                new OperationValidationRuleContainer(
                    new ChangingTariffTaxValueNotAllowedRule(chargeInformationDto, charge), chargeInformationDto.Id),
            };
        }

        private static void AddUpdateRules(
            List<IValidationRuleContainer> rules,
            ChargeInformationDto chargeInformationDto,
            Charge existingCharge)
        {
            var updateRules = new List<IValidationRuleContainer>
            {
                new OperationValidationRuleContainer(
                    new UpdateChargeMustHaveEffectiveDateBeforeOrOnStopDateRule(existingCharge, chargeInformationDto),
                    chargeInformationDto.Id),
                new OperationValidationRuleContainer(
                    new ChargeResolutionCanNotBeUpdatedRule(existingCharge, chargeInformationDto),
                    chargeInformationDto.Id),
            };

            rules.AddRange(updateRules);
        }

        private List<IValidationRuleContainer> GetMandatoryRulesForOperation(
            ChargeInformationDto chargeInformationDto,
            RulesConfiguration configuration)
        {
            var rules = new List<IValidationRuleContainer>
            {
                new OperationValidationRuleContainer(
                    new StartDateValidationRule(
                        chargeInformationDto,
                        configuration.StartDateValidationRuleConfiguration,
                        _zonedDateTimeService,
                        _clock),
                    chargeInformationDto.Id),
            };

            return rules;
        }

        private async Task<Charge?> GetChargeOrNullAsync(ChargeInformationDto chargeInformationDto)
        {
            var chargeIdentifier = await _chargeIdentifierFactory
                .CreateAsync(chargeInformationDto.ChargeId, chargeInformationDto.Type, chargeInformationDto.ChargeOwner)
                .ConfigureAwait(false);

            return await _chargeRepository.SingleOrNullAsync(chargeIdentifier).ConfigureAwait(false);
        }
    }
}
