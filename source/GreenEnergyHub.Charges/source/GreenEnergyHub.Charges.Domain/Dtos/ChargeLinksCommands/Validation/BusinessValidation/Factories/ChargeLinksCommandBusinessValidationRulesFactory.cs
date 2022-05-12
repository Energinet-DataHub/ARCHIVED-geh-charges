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
using GreenEnergyHub.Charges.Domain.ChargeInformations;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MeteringPoints;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands.Validation.BusinessValidation.Factories
{
    public class ChargeLinksCommandBusinessValidationRulesFactory : IBusinessValidationRulesFactory<ChargeLinkDto>
    {
        private readonly IChargeInformationRepository _chargeInformationRepository;
        private readonly IMeteringPointRepository _meteringPointRepository;
        private readonly IChargeLinksRepository _chargeLinksRepository;

        public ChargeLinksCommandBusinessValidationRulesFactory(
            IChargeInformationRepository chargeInformationRepository,
            IMeteringPointRepository meteringPointRepository,
            IChargeLinksRepository chargeLinksRepository)
        {
            _chargeInformationRepository = chargeInformationRepository;
            _meteringPointRepository = meteringPointRepository;
            _chargeLinksRepository = chargeLinksRepository;
        }

        public async Task<IValidationRuleSet> CreateRulesAsync(ChargeLinkDto operation)
        {
            ArgumentNullException.ThrowIfNull(operation);

            var meteringPoint = await _meteringPointRepository
                .GetOrNullAsync(operation.MeteringPointId)
                .ConfigureAwait(false);

            var rules = GetMandatoryRulesForCommand(meteringPoint);
            if (meteringPoint == null)
                return ValidationRuleSet.FromRules(rules);

            rules.AddRange(await GetRulesForChargeLinkDtoAsync(operation, meteringPoint).ConfigureAwait(false));

            return ValidationRuleSet.FromRules(rules);
        }

        private List<IValidationRule> GetMandatoryRulesForCommand(MeteringPoint? meteringPoint)
        {
            return new List<IValidationRule>
            {
                new MeteringPointMustExistRule(meteringPoint),
            };
        }

        private async Task<IEnumerable<IValidationRule>> GetRulesForChargeLinkDtoAsync(
            ChargeLinkDto chargeLinkDto,
            MeteringPoint meteringPoint)
        {
            var rules = new List<IValidationRule>();

            var charge = await _chargeInformationRepository
                .GetOrNullAsync(new ChargeInformationIdentifier(chargeLinkDto.SenderProvidedChargeId, chargeLinkDto.ChargeOwner, chargeLinkDto.ChargeType))
                .ConfigureAwait(false);

            rules.Add(new ChargeMustExistRule(charge, chargeLinkDto));

            if (charge == null)
                return rules;

            var existingChargeLinks = await _chargeLinksRepository
                .GetAsync(charge.Id, meteringPoint.Id)
                .ConfigureAwait(false);

            rules.Add(new ChargeLinksUpdateNotYetSupportedRule(chargeLinkDto, existingChargeLinks));

            return rules;
        }
    }
}
