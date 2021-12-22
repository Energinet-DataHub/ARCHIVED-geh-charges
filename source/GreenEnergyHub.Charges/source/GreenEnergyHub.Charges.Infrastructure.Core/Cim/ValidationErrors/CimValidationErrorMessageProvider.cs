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

using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;

namespace GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors
{
    public static class CimValidationErrorMessageProvider
    {
        public static string GetCimValidationErrorMessage(ValidationRuleIdentifier validationRuleIdentifier)
        {
            return validationRuleIdentifier switch
            {
                // Please keep sorted by ValidationRuleIdentifier
                ValidationRuleIdentifier.BusinessReasonCodeMustBeUpdateChargeInformation =>
                    CimValidationErrorMessages.BusinessReasonCodeMustBeUpdateChargeInformationErrorMessage,
                ValidationRuleIdentifier.ChangingTariffTaxValueNotAllowed =>
                    CimValidationErrorMessages.ChangingTariffTaxValueNotAllowedErrorMessage,
                ValidationRuleIdentifier.ChangingTariffVatValueNotAllowed =>
                    CimValidationErrorMessages.ChangingTariffVatValueNotAllowedErrorMessage,
                ValidationRuleIdentifier.ChargeDescriptionHasMaximumLength =>
                    CimValidationErrorMessages.ChargeDescriptionHasMaximumLengthErrorMessage,
                ValidationRuleIdentifier.ChargeNameHasMaximumLength =>
                    CimValidationErrorMessages.ChargeNameHasMaximumLengthErrorMessage,
                ValidationRuleIdentifier.ChargeTypeTariffPriceCount =>
                    CimValidationErrorMessages.ChargeTypeTariffPriceCountErrorMessage,
                ValidationRuleIdentifier.ChargeIdLengthValidation =>
                    CimValidationErrorMessages.ChargeIdLengthValidationErrorMessage,
                ValidationRuleIdentifier.ChargeIdRequiredValidation =>
                    CimValidationErrorMessages.ChargeIdRequiredValidationErrorMessage,
                ValidationRuleIdentifier.ChargeOperationIdRequired =>
                    CimValidationErrorMessages.ChargeOperationIdRequiredErrorMessage,
                ValidationRuleIdentifier.ChargeOwnerIsRequiredValidation =>
                    CimValidationErrorMessages.ChargeOwnerIsRequiredValidationErrorMessage,
                ValidationRuleIdentifier.ChargePriceMaximumDigitsAndDecimals =>
                    CimValidationErrorMessages.ChargePriceMaximumDigitsAndDecimalsErrorMessage,
                ValidationRuleIdentifier.ChargeTypeIsKnownValidation =>
                    CimValidationErrorMessages.ChargeTypeIsKnownValidationErrorMessage,
                ValidationRuleIdentifier.CommandSenderMustBeAnExistingMarketParticipant =>
                    CimValidationErrorMessages.CommandSenderMustBeAnExistingMarketParticipantErrorMessage,
                ValidationRuleIdentifier.DocumentTypeMustBeRequestUpdateChargeInformation =>
                    CimValidationErrorMessages.DocumentTypeMustBeRequestUpdateChargeInformationErrorMessage,
                ValidationRuleIdentifier.FeeMustHaveSinglePrice =>
                    CimValidationErrorMessages.FeeMustHaveSinglePriceErrorMessage,
                ValidationRuleIdentifier.MaximumPrice =>
                    CimValidationErrorMessages.MaximumPriceErrorMessage,
                ValidationRuleIdentifier.OperationTypeValidation =>
                    CimValidationErrorMessages.OperationTypeValidationErrorMessage,
                ValidationRuleIdentifier.ProcessTypeIsKnownValidation =>
                    CimValidationErrorMessages.ProcessTypeIsKnownValidationErrorMessage,
                ValidationRuleIdentifier.RecipientIsMandatoryTypeValidation =>
                    CimValidationErrorMessages.RecipientIsMandatoryTypeValidationErrorMessage,
                ValidationRuleIdentifier.ResolutionFeeValidation =>
                    CimValidationErrorMessages.ResolutionFeeValidationErrorMessage,
                ValidationRuleIdentifier.ResolutionSubscriptionValidation =>
                    CimValidationErrorMessages.ResolutionSubscriptionValidationErrorMessage,
                ValidationRuleIdentifier.ResolutionTariffValidation =>
                    CimValidationErrorMessages.ResolutionTariffValidationErrorMessage,
                ValidationRuleIdentifier.SenderIsMandatoryTypeValidation =>
                    CimValidationErrorMessages.SenderIsMandatoryTypeValidationErrorMessage,
                ValidationRuleIdentifier.StartDateTimeRequiredValidation =>
                    CimValidationErrorMessages.StartDateTimeRequiredValidationErrorMessage,
                ValidationRuleIdentifier.StartDateValidation =>
                    CimValidationErrorMessages.StartDateValidationErrorMessage,
                ValidationRuleIdentifier.SubscriptionMustHaveSinglePrice =>
                    CimValidationErrorMessages.SubscriptionMustHaveSinglePriceErrorMessage,
                ValidationRuleIdentifier.Unknown => CimValidationErrorMessages.UnknownError,
                ValidationRuleIdentifier.UpdateNotYetSupported =>
                    CimValidationErrorMessages.UpdateNotYetSupportedErrorMessage,
                ValidationRuleIdentifier.VatClassificationValidation =>
                    CimValidationErrorMessages.VatClassificationValidationErrorMessage,
                _ => CimValidationErrorMessages.UnknownError,
            };
        }
    }
}
