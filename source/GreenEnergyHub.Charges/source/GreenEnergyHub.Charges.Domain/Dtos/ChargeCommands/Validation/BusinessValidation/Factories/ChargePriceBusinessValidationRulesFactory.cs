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
    public class ChargePriceBusinessValidationRulesFactory : IBusinessValidationRulesFactory<ChargePriceDto>
    {
        private readonly IChargeRepository _chargeRepository;
        private readonly IChargeIdentifierFactory _chargeIdentifierFactory;
        private readonly IClock _clock;
        private readonly IRulesConfigurationRepository _rulesConfigurationRepository;
        private readonly IZonedDateTimeService _zonedDateTimeService;

        public ChargePriceBusinessValidationRulesFactory(
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

        public async Task<IValidationRuleSet> CreateRulesAsync(ChargePriceDto chargeOperation)
        {
            ArgumentNullException.ThrowIfNull(chargeOperation);
            var rules = await GetRulesForOperationAsync(chargeOperation).ConfigureAwait(false);
            return ValidationRuleSet.FromRules(rules);
        }

        private async Task<List<IValidationRuleContainer>> GetRulesForOperationAsync(ChargePriceDto chargeOperation)
        {
            var configuration = await _rulesConfigurationRepository.GetConfigurationAsync().ConfigureAwait(false);
            var charge = await GetChargeOrNullAsync(chargeOperation).ConfigureAwait(false);
            var rules = GetMandatoryRulesForOperation(chargeOperation, configuration);

            return rules;
        }

        private List<IValidationRuleContainer> GetMandatoryRulesForOperation(
            IChargeOperation chargeOperation,
            RulesConfiguration configuration)
        {
            var rules = new List<IValidationRuleContainer>
            {
                new OperationValidationRuleContainer(
                    new StartDateValidationRule(
                        chargeOperation,
                        configuration.StartDateValidationRuleConfiguration,
                        _zonedDateTimeService,
                        _clock),
                    chargeOperation.Id),
            };

            return rules;
        }

        private async Task<Charge?> GetChargeOrNullAsync(IChargeOperation chargeOperation)
        {
            var chargeIdentifier = await _chargeIdentifierFactory
                .CreateAsync(chargeOperation.ChargeId, chargeOperation.Type, chargeOperation.ChargeOwner)
                .ConfigureAwait(false);

            return await _chargeRepository.SingleOrNullAsync(chargeIdentifier).ConfigureAwait(false);
        }
    }
}
