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
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.Factories
{
    public class ChargeOperationInputValidationRulesFactory : IInputValidationRulesFactory<ChargeOperationDto>
    {
        private readonly IClock _clock;
        private readonly IZonedDateTimeService _zonedDateTimeService;

        public ChargeOperationInputValidationRulesFactory(
            IZonedDateTimeService zonedDateTimeService,
            IClock clock)
        {
            _zonedDateTimeService = zonedDateTimeService;
            _clock = clock;
        }

        public IValidationRuleSet CreateRules(ChargeOperationDto operation)
        {
            ArgumentNullException.ThrowIfNull(operation);
            var rules = GetRulesForOperation(operation);
            return ValidationRuleSet.FromRules(rules.ToList());
        }

        private IEnumerable<IValidationRuleContainer> GetRulesForOperation(ChargeOperationDto chargeOperationDto)
        {
            var rules = new List<IValidationRuleContainer>
            {
                CreateRuleContainer(new ChargeIdLengthValidationRule(chargeOperationDto), chargeOperationDto.Id),
                CreateRuleContainer(new ChargeIdRequiredValidationRule(chargeOperationDto), chargeOperationDto.Id),
                CreateRuleContainer(new ChargeOperationIdRequiredRule(chargeOperationDto), chargeOperationDto.Id),
                CreateRuleContainer(new ChargeOwnerIsRequiredValidationRule(chargeOperationDto), chargeOperationDto.Id),
                CreateRuleContainer(new ChargeTypeIsKnownValidationRule(chargeOperationDto), chargeOperationDto.Id),
                CreateRuleContainer(new StartDateTimeRequiredValidationRule(chargeOperationDto), chargeOperationDto.Id),
                CreateRuleContainer(new ChargeOwnerTextLengthRule(chargeOperationDto), chargeOperationDto.Id),
            };

            rules.AddRange(chargeOperationDto.Points.Any()
                ? CreateRulesForChargePrice(chargeOperationDto, _zonedDateTimeService)
                : CreateRulesForChargeInformation(chargeOperationDto));

            return rules;
        }

        private static List<IValidationRuleContainer> CreateRulesForChargePrice(ChargeOperationDto chargeOperationDto, IZonedDateTimeService zonedDateTimeService)
        {
            return new List<IValidationRuleContainer>
            {
                CreateRuleContainer(new ChargePriceMaximumDigitsAndDecimalsRule(chargeOperationDto), chargeOperationDto.Id),
                CreateRuleContainer(new ChargeTypeTariffPriceCountRule(chargeOperationDto), chargeOperationDto.Id),
                CreateRuleContainer(new MaximumPriceRule(chargeOperationDto), chargeOperationDto.Id),
                CreateRuleContainer(new NumberOfPointsMatchTimeIntervalAndResolutionRule(chargeOperationDto), chargeOperationDto.Id),
                CreateRuleContainer(new PriceListMustStartAndStopAtMidnightValidationRule(zonedDateTimeService, chargeOperationDto), chargeOperationDto.Id),
            };
        }

        private List<IValidationRuleContainer> CreateRulesForChargeInformation(ChargeOperationDto chargeOperationDto)
        {
            return new List<IValidationRuleContainer>
            {
                CreateRuleContainer(new ResolutionFeeValidationRule(chargeOperationDto), chargeOperationDto.Id),
                CreateRuleContainer(new ResolutionSubscriptionValidationRule(chargeOperationDto), chargeOperationDto.Id),
                CreateRuleContainer(new ResolutionTariffValidationRule(chargeOperationDto), chargeOperationDto.Id),
                CreateRuleContainer(new ChargeNameHasMaximumLengthRule(chargeOperationDto), chargeOperationDto.Id),
                CreateRuleContainer(new ChargeDescriptionHasMaximumLengthRule(chargeOperationDto), chargeOperationDto.Id),
                CreateRuleContainer(new VatClassificationValidationRule(chargeOperationDto), chargeOperationDto.Id),
                CreateRuleContainer(new TransparentInvoicingIsNotAllowedForFeeValidationRule(chargeOperationDto), chargeOperationDto.Id),
                CreateRuleContainer(new ChargePriceMaximumDigitsAndDecimalsRule(chargeOperationDto), chargeOperationDto.Id),
                CreateRuleContainer(new ChargeTypeTariffPriceCountRule(chargeOperationDto), chargeOperationDto.Id),
                CreateRuleContainer(new MaximumPriceRule(chargeOperationDto), chargeOperationDto.Id),
                CreateRuleContainer(
                    new StartDateValidationRule(
                        chargeOperationDto.StartDateTime,
                        _zonedDateTimeService,
                        _clock),
                    chargeOperationDto.Id),
                CreateRuleContainer(new ChargeNameRequiredRule(chargeOperationDto), chargeOperationDto.Id),
                CreateRuleContainer(new ChargeDescriptionRequiredRule(chargeOperationDto), chargeOperationDto.Id),
                CreateRuleContainer(new ResolutionIsRequiredRule(chargeOperationDto), chargeOperationDto.Id),
                CreateRuleContainer(new TransparentInvoicingIsRequiredValidationRule(chargeOperationDto), chargeOperationDto.Id),
                CreateRuleContainer(new TaxIndicatorIsRequiredValidationRule(chargeOperationDto), chargeOperationDto.Id),
                CreateRuleContainer(new TerminationDateMustMatchEffectiveDateValidationRule(chargeOperationDto), chargeOperationDto.Id),
            };
        }

        private static IValidationRuleContainer CreateRuleContainer(
            IValidationRule validationRule, string operationId)
        {
            return new OperationValidationRuleContainer(validationRule, operationId);
        }
    }
}
