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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.Dtos.Validation.InputValidation;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands.Validation.InputValidation.Factories
{
    public class ChargeOperationInputValidationRulesFactory : IInputValidationRulesFactory<ChargeInformationOperationDto>
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

        public IValidationRuleSet CreateRules(ChargeInformationOperationDto informationOperation, DocumentDto document)
        {
            ArgumentNullException.ThrowIfNull(informationOperation);
            var rules = GetRulesForOperation(informationOperation, document);
            return ValidationRuleSet.FromRules(rules.ToList());
        }

        private IEnumerable<IValidationRuleContainer> GetRulesForOperation(ChargeInformationOperationDto chargeInformationOperationDto, DocumentDto document)
        {
            var rules = new List<IValidationRuleContainer>
            {
                CreateRuleContainer(new ChargeIdLengthValidationRule(chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
                CreateRuleContainer(new ChargeIdRequiredValidationRule(chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
                CreateRuleContainer(new ChargeOperationIdRequiredRule(chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
                CreateRuleContainer(new ChargeOperationIdLengthValidationRule(chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
                CreateRuleContainer(new ChargeOwnerIsRequiredValidationRule(chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
                CreateRuleContainer(new ChargeTypeIsKnownValidationRule(chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
                CreateRuleContainer(new StartDateTimeRequiredValidationRule(chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
                CreateRuleContainer(new ChargeOwnerTextLengthRule(chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
                CreateRuleContainer(new ChargeOwnerMustMatchSenderRule(document.Sender.MarketParticipantId, chargeInformationOperationDto.ChargeOwner), chargeInformationOperationDto.OperationId),
            };

            switch (document.BusinessReasonCode)
            {
                case BusinessReasonCode.UpdateChargeInformation:
                    rules.AddRange(CreateRulesForChargeInformation(chargeInformationOperationDto));
                    break;
                case BusinessReasonCode.UpdateChargePrices:
                    rules.AddRange(CreateRulesForChargePrice(chargeInformationOperationDto));
                    break;
                case BusinessReasonCode.Unknown:
                case BusinessReasonCode.UpdateMasterDataSettlement:
                default:
                    throw new ArgumentOutOfRangeException($"Could not create input validation rules for business reason code: {document.BusinessReasonCode}");
            }

            return rules;
        }

        private List<IValidationRuleContainer> CreateRulesForChargePrice(ChargeInformationOperationDto chargeInformationOperationDto)
        {
            return new List<IValidationRuleContainer>
            {
                CreateRuleContainer(new ChargePriceMaximumDigitsAndDecimalsRule(chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
                CreateRuleContainer(new ChargeTypeTariffPriceCountRule(chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
                CreateRuleContainer(new MaximumPriceRule(chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
                CreateRuleContainer(new NumberOfPointsMatchTimeIntervalAndResolutionRule(chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
                CreateRuleContainer(new PriceListMustStartAndStopAtMidnightValidationRule(_zonedDateTimeService, chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
            };
        }

        private List<IValidationRuleContainer> CreateRulesForChargeInformation(ChargeInformationOperationDto chargeInformationOperationDto)
        {
            return new List<IValidationRuleContainer>
            {
                CreateRuleContainer(new ResolutionFeeValidationRule(chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
                CreateRuleContainer(new ResolutionSubscriptionValidationRule(chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
                CreateRuleContainer(new ResolutionTariffValidationRule(chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
                CreateRuleContainer(new ChargeNameHasMaximumLengthRule(chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
                CreateRuleContainer(new ChargeDescriptionHasMaximumLengthRule(chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
                CreateRuleContainer(new VatClassificationValidationRule(chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
                CreateRuleContainer(new TransparentInvoicingIsNotAllowedForFeeValidationRule(chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
                CreateRuleContainer(new ChargePriceMaximumDigitsAndDecimalsRule(chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
                CreateRuleContainer(new ChargeTypeTariffPriceCountRule(chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
                CreateRuleContainer(new MaximumPriceRule(chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
                CreateRuleContainer(
                    new StartDateValidationRule(
                        chargeInformationOperationDto.StartDate,
                        _zonedDateTimeService,
                        _clock),
                    chargeInformationOperationDto.OperationId),
                CreateRuleContainer(new ChargeNameRequiredRule(chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
                CreateRuleContainer(new ChargeDescriptionRequiredRule(chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
                CreateRuleContainer(new ResolutionIsRequiredRule(chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
                CreateRuleContainer(new TransparentInvoicingIsRequiredValidationRule(chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
                CreateRuleContainer(new TaxIndicatorIsRequiredValidationRule(chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
                CreateRuleContainer(new TerminationDateMustMatchEffectiveDateValidationRule(chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
                CreateRuleContainer(new TaxIndicatorMustBeFalseForFeeValidationRule(chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
                CreateRuleContainer(new TaxIndicatorMustBeFalseForSubscriptionValidationRule(chargeInformationOperationDto), chargeInformationOperationDto.OperationId),
            };
        }

        private static IValidationRuleContainer CreateRuleContainer(
            IValidationRule validationRule, string operationId)
        {
            return new OperationValidationRuleContainer(validationRule, operationId);
        }
    }
}
