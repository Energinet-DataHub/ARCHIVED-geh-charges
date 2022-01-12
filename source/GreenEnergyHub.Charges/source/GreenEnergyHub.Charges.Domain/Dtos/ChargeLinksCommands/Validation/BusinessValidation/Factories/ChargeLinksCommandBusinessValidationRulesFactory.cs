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
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MeteringPoints;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands.Validation.BusinessValidation.Factories
{
    public class ChargeLinksCommandBusinessValidationRulesFactory : IBusinessValidationRulesFactory<ChargeLinksCommand>
    {
        private readonly IChargeRepository _chargeRepository;
        private readonly IMeteringPointRepository _meteringPointRepository;
        private readonly IChargeLinkRepository _chargeLinkRepository;

        public ChargeLinksCommandBusinessValidationRulesFactory(
            IChargeRepository chargeRepository,
            IMeteringPointRepository meteringPointRepository,
            IChargeLinkRepository chargeLinkRepository)
        {
            _chargeRepository = chargeRepository;
            _meteringPointRepository = meteringPointRepository;
            _chargeLinkRepository = chargeLinkRepository;
        }

        public async Task<IValidationRuleSet> CreateRulesAsync(ChargeLinksCommand chargeLinksCommand)
        {
            if (chargeLinksCommand == null) throw new ArgumentNullException(nameof(chargeLinksCommand));
            var meteringPoint = await _meteringPointRepository.GetOrNullAsync(chargeLinksCommand.MeteringPointId);

            var rules = GetMandatoryRulesForCommand(meteringPoint);
            if (meteringPoint == null)
                return ValidationRuleSet.FromRules(rules);

            foreach (var link in chargeLinksCommand.ChargeLinks)
            {
                var charge = await _chargeRepository
                    .GetOrNullAsync(new ChargeIdentifier(link.SenderProvidedChargeId, link.ChargeOwnerId, link.ChargeType))
                    .ConfigureAwait(false);

                rules.Add(new ChargeMustExistRule(charge));
                if (charge == null)
                    continue;

                var existingChargeLinks = await _chargeLinkRepository.GetAsync(charge.Id, meteringPoint.Id);
                GetMandatoryRulesForSingleLinks(rules, chargeLinksCommand, existingChargeLinks);
            }

            return ValidationRuleSet.FromRules(rules);
        }

        private List<IValidationRule> GetMandatoryRulesForCommand(MeteringPoint? meteringPoint)
        {
            return new List<IValidationRule>
            {
                new MeteringPointMustExistRule(meteringPoint),
            };
        }

        private void GetMandatoryRulesForSingleLinks(
            List<IValidationRule> rules,
            ChargeLinksCommand chargeLinksCommand,
            IReadOnlyCollection<ChargeLink> existingChargeLinks)
        {
            rules.Add(new ChargeLinksUpdateNotYetSupportedRule(chargeLinksCommand, existingChargeLinks));
        }
    }
}
