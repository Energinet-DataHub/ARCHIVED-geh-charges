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
    public class CimValidationErrorMessageProvider : ICimValidationErrorMessageProvider
    {
        public string GetCimValidationErrorMessage(ValidationRuleIdentifier validationRuleIdentifier)
        {
            return validationRuleIdentifier switch
            {
                // Please keep sorted by ValidationRuleIdentifier
                ValidationRuleIdentifier.BusinessReasonCodeMustBeUpdateChargeInformation =>
                    CimValidationErrorTemplateMessages.BusinessReasonCodeMustBeUpdateChargeInformationErrorMessage,
                ValidationRuleIdentifier.ChangingTariffTaxValueNotAllowed =>
                    CimValidationErrorTemplateMessages.ChangingTariffTaxValueNotAllowedErrorMessage,
                ValidationRuleIdentifier.ChangingTariffVatValueNotAllowed =>
                    CimValidationErrorTemplateMessages.ChangingTariffVatValueNotAllowedErrorMessage,
                ValidationRuleIdentifier.ChargeDescriptionHasMaximumLength =>
                    CimValidationErrorTemplateMessages.ChargeDescriptionHasMaximumLengthErrorMessage,
                ValidationRuleIdentifier.ChargeNameHasMaximumLength =>
                    CimValidationErrorTemplateMessages.ChargeNameHasMaximumLengthErrorMessage,
                ValidationRuleIdentifier.ChargeTypeTariffPriceCount =>
                    CimValidationErrorTemplateMessages.ChargeTypeTariffPriceCountErrorMessage,
                ValidationRuleIdentifier.ChargeIdLengthValidation =>
                    CimValidationErrorTemplateMessages.ChargeIdLengthValidationErrorMessage,
                ValidationRuleIdentifier.ChargeIdRequiredValidation =>
                    CimValidationErrorTemplateMessages.ChargeIdRequiredValidationErrorMessage,
                ValidationRuleIdentifier.ChargeOperationIdRequired =>
                    CimValidationErrorTemplateMessages.ChargeOperationIdRequiredErrorMessage,
                ValidationRuleIdentifier.ChargeOwnerIsRequiredValidation =>
                    CimValidationErrorTemplateMessages.ChargeOwnerIsRequiredValidationErrorMessage,
                ValidationRuleIdentifier.ChargePriceMaximumDigitsAndDecimals =>
                    CimValidationErrorTemplateMessages.ChargePriceMaximumDigitsAndDecimalsErrorMessage,
                ValidationRuleIdentifier.ChargeTypeIsKnownValidation =>
                    CimValidationErrorTemplateMessages.ChargeTypeIsKnownValidationErrorMessage,
                ValidationRuleIdentifier.CommandSenderMustBeAnExistingMarketParticipant =>
                    CimValidationErrorTemplateMessages.CommandSenderMustBeAnExistingMarketParticipantErrorMessage,
                ValidationRuleIdentifier.DocumentTypeMustBeRequestUpdateChargeInformation =>
                    CimValidationErrorTemplateMessages.DocumentTypeMustBeRequestUpdateChargeInformationErrorMessage,
                ValidationRuleIdentifier.FeeMustHaveSinglePrice =>
                    CimValidationErrorTemplateMessages.FeeMustHaveSinglePriceErrorMessage,
                ValidationRuleIdentifier.MaximumPrice =>
                    CimValidationErrorTemplateMessages.MaximumPriceErrorMessage,
                ValidationRuleIdentifier.RecipientIsMandatoryTypeValidation =>
                    CimValidationErrorTemplateMessages.RecipientIsMandatoryTypeValidationErrorMessage,
                ValidationRuleIdentifier.ResolutionFeeValidation =>
                    CimValidationErrorTemplateMessages.ResolutionFeeValidationErrorMessage,
                ValidationRuleIdentifier.ResolutionSubscriptionValidation =>
                    CimValidationErrorTemplateMessages.ResolutionSubscriptionValidationErrorMessage,
                ValidationRuleIdentifier.ResolutionTariffValidation =>
                    CimValidationErrorTemplateMessages.ResolutionTariffValidationErrorMessage,
                ValidationRuleIdentifier.SenderIsMandatoryTypeValidation =>
                    CimValidationErrorTemplateMessages.SenderIsMandatoryTypeValidationErrorMessage,
                ValidationRuleIdentifier.StartDateTimeRequiredValidation =>
                    CimValidationErrorTemplateMessages.StartDateTimeRequiredValidationErrorMessage,
                ValidationRuleIdentifier.StartDateValidation =>
                    CimValidationErrorTemplateMessages.StartDateValidationErrorMessage,
                ValidationRuleIdentifier.SubscriptionMustHaveSinglePrice =>
                    CimValidationErrorTemplateMessages.SubscriptionMustHaveSinglePriceErrorMessage,
                ValidationRuleIdentifier.UpdateNotYetSupported =>
                    CimValidationErrorTemplateMessages.UpdateNotYetSupportedErrorMessage,
                ValidationRuleIdentifier.VatClassificationValidation =>
                    CimValidationErrorTemplateMessages.VatClassificationValidationErrorMessage,
                _ => throw new NotImplementedException(),
            };
        }
    }
}
