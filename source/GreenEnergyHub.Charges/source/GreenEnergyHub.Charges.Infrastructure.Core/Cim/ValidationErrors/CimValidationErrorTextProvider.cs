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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;

namespace GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors
{
    public class CimValidationErrorTextProvider : ICimValidationErrorTextProvider
    {
        public string GetCimValidationErrorText(ValidationRuleIdentifier validationRuleIdentifier)
        {
            return validationRuleIdentifier switch
            {
                // Please keep sorted by ValidationRuleIdentifier
                ValidationRuleIdentifier.BusinessReasonCodeMustBeUpdateChargeInformation =>
                    CimValidationErrorTextTemplateMessages.BusinessReasonCodeMustBeUpdateChargeInformationErrorText,
                ValidationRuleIdentifier.ChangingTariffTaxValueNotAllowed =>
                    CimValidationErrorTextTemplateMessages.ChangingTariffTaxValueNotAllowedErrorText,
                ValidationRuleIdentifier.ChangingTariffVatValueNotAllowed =>
                    CimValidationErrorTextTemplateMessages.ChangingTariffVatValueNotAllowedErrorText,
                ValidationRuleIdentifier.ChargeDescriptionHasMaximumLength =>
                    CimValidationErrorTextTemplateMessages.ChargeDescriptionHasMaximumLengthErrorText,
                ValidationRuleIdentifier.ChargeNameHasMaximumLength =>
                    CimValidationErrorTextTemplateMessages.ChargeNameHasMaximumLengthErrorText,
                ValidationRuleIdentifier.ChargeTypeTariffPriceCount =>
                    CimValidationErrorTextTemplateMessages.ChargeTypeTariffPriceCountErrorText,
                ValidationRuleIdentifier.ChargeIdLengthValidation =>
                    CimValidationErrorTextTemplateMessages.ChargeIdLengthValidationErrorText,
                ValidationRuleIdentifier.ChargeIdRequiredValidation =>
                    CimValidationErrorTextTemplateMessages.ChargeIdRequiredValidationErrorText,
                ValidationRuleIdentifier.ChargeOperationIdRequired =>
                    CimValidationErrorTextTemplateMessages.ChargeOperationIdRequiredErrorText,
                ValidationRuleIdentifier.ChargeOwnerIsRequiredValidation =>
                    CimValidationErrorTextTemplateMessages.ChargeOwnerIsRequiredValidationErrorText,
                ValidationRuleIdentifier.ChargePriceMaximumDigitsAndDecimals =>
                    CimValidationErrorTextTemplateMessages.ChargePriceMaximumDigitsAndDecimalsErrorText,
                ValidationRuleIdentifier.ChargeTypeIsKnownValidation =>
                    CimValidationErrorTextTemplateMessages.ChargeTypeIsKnownValidationErrorText,
                ValidationRuleIdentifier.CommandSenderMustBeAnExistingMarketParticipant =>
                    CimValidationErrorTextTemplateMessages.CommandSenderMustBeAnExistingMarketParticipantErrorText,
                ValidationRuleIdentifier.DocumentTypeMustBeRequestUpdateChargeInformation =>
                    CimValidationErrorTextTemplateMessages.DocumentTypeMustBeRequestUpdateChargeInformationErrorText,
                ValidationRuleIdentifier.FeeMustHaveSinglePrice =>
                    CimValidationErrorTextTemplateMessages.FeeMustHaveSinglePriceErrorText,
                ValidationRuleIdentifier.MaximumPrice =>
                    CimValidationErrorTextTemplateMessages.MaximumPriceErrorText,
                ValidationRuleIdentifier.RecipientIsMandatoryTypeValidation =>
                    CimValidationErrorTextTemplateMessages.RecipientIsMandatoryTypeValidationErrorText,
                ValidationRuleIdentifier.ResolutionFeeValidation =>
                    CimValidationErrorTextTemplateMessages.ResolutionFeeValidationErrorText,
                ValidationRuleIdentifier.ResolutionSubscriptionValidation =>
                    CimValidationErrorTextTemplateMessages.ResolutionSubscriptionValidationErrorText,
                ValidationRuleIdentifier.ResolutionTariffValidation =>
                    CimValidationErrorTextTemplateMessages.ResolutionTariffValidationErrorText,
                ValidationRuleIdentifier.SenderIsMandatoryTypeValidation =>
                    CimValidationErrorTextTemplateMessages.SenderIsMandatoryTypeValidationErrorText,
                ValidationRuleIdentifier.StartDateTimeRequiredValidation =>
                    CimValidationErrorTextTemplateMessages.StartDateTimeRequiredValidationErrorText,
                ValidationRuleIdentifier.StartDateValidation =>
                    CimValidationErrorTextTemplateMessages.StartDateValidationErrorText,
                ValidationRuleIdentifier.SubscriptionMustHaveSinglePrice =>
                    CimValidationErrorTextTemplateMessages.SubscriptionMustHaveSinglePriceErrorText,
                ValidationRuleIdentifier.UpdateNotYetSupported =>
                    CimValidationErrorTextTemplateMessages.UpdateNotYetSupportedErrorText,
                ValidationRuleIdentifier.VatClassificationValidation =>
                    CimValidationErrorTextTemplateMessages.VatClassificationValidationErrorText,
                _ => throw new NotImplementedException(),
            };
        }
    }
}
