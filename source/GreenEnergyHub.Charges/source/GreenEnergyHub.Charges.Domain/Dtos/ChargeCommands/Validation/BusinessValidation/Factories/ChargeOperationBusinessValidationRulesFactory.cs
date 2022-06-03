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
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.Factories
{
    public class ChargeOperationBusinessValidationRulesFactory : IBusinessValidationRulesFactory<ChargeOperationDto>
    {
        private readonly IChargeRepository _chargeRepository;
        private readonly IChargeIdentifierFactory _chargeIdentifierFactory;

        public ChargeOperationBusinessValidationRulesFactory(
            IChargeRepository chargeRepository,
            IChargeIdentifierFactory chargeIdentifierFactory)
        {
            _chargeRepository = chargeRepository;
            _chargeIdentifierFactory = chargeIdentifierFactory;
        }

        public async Task<IValidationRuleSet> CreateRulesAsync(ChargeOperationDto operation)
        {
            ArgumentNullException.ThrowIfNull(operation);
            var rules = await GetRulesForOperationAsync(operation).ConfigureAwait(false);
            return ValidationRuleSet.FromRules(rules);
        }

        private async Task<List<IValidationRuleContainer>> GetRulesForOperationAsync(ChargeOperationDto chargeOperationDto)
        {
            var charge = await GetChargeOrNullAsync(chargeOperationDto).ConfigureAwait(false);
            var rules = new List<IValidationRuleContainer>();
            if (charge == null)
            {
                return rules;
            }

            var isChargePrices = chargeOperationDto.Points.Any();
            if (chargeOperationDto.Type == ChargeType.Tariff && isChargePrices is false)
            {
                rules.AddRange(AddTariffOnlyRules(chargeOperationDto, charge));
            }

            AddUpdateRules(rules, chargeOperationDto, charge);
            return rules;
        }

        private static IEnumerable<IValidationRuleContainer> AddTariffOnlyRules(
            ChargeOperationDto chargeOperationDto, Charge charge)
        {
            return new List<IValidationRuleContainer>
            {
                new OperationValidationRuleContainer(
                    new ChangingTariffTaxValueNotAllowedRule(chargeOperationDto, charge), chargeOperationDto.Id),
            };
        }

        private static void AddUpdateRules(
            List<IValidationRuleContainer> rules,
            ChargeOperationDto chargeOperationDto,
            Charge existingCharge)
        {
            var updateRules = new List<IValidationRuleContainer>
            {
                new OperationValidationRuleContainer(
                    new UpdateChargeMustHaveEffectiveDateBeforeOrOnStopDateRule(existingCharge, chargeOperationDto),
                    chargeOperationDto.Id),
                new OperationValidationRuleContainer(
                    new ChargeResolutionCanNotBeUpdatedRule(existingCharge, chargeOperationDto),
                    chargeOperationDto.Id),
            };

            rules.AddRange(updateRules);
        }

        private async Task<Charge?> GetChargeOrNullAsync(ChargeOperationDto chargeOperationDto)
        {
            var chargeIdentifier = await _chargeIdentifierFactory
                .CreateAsync(chargeOperationDto.ChargeId, chargeOperationDto.Type, chargeOperationDto.ChargeOwner)
                .ConfigureAwait(false);

            return await _chargeRepository.SingleOrNullAsync(chargeIdentifier).ConfigureAwait(false);
        }
    }
}
