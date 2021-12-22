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
                ValidationRuleIdentifier.Unknown => CimValidationErrorMessages.UnknownError,
                ValidationRuleIdentifier.StartDateValidation =>
                    CimValidationErrorMessages.StartDateValidationErrorMessage,
                ValidationRuleIdentifier.ChangingTariffVatValueNotAllowed =>
                    CimValidationErrorMessages.ChangingTariffVatValueNotAllowedErrorMessage,
                ValidationRuleIdentifier.ChangingTariffTaxValueNotAllowed =>
                    CimValidationErrorMessages.ChangingTariffTaxValueNotAllowedErrorMessage,
                ValidationRuleIdentifier.ProcessTypeIsKnownValidation =>
                    CimValidationErrorMessages.ProcessTypeIsKnownValidationErrorMessage,
                ValidationRuleIdentifier.SenderIsMandatoryTypeValidation =>
                    CimValidationErrorMessages.SenderIsMandatoryTypeValidationErrorMessage,
                ValidationRuleIdentifier.RecipientIsMandatoryTypeValidation =>
                    CimValidationErrorMessages.RecipientIsMandatoryTypeValidationErrorMessage,
                ValidationRuleIdentifier.ChargeOperationIdRequired =>
                    CimValidationErrorMessages.ChargeOperationIdRequiredErrorMessage,
                ValidationRuleIdentifier.OperationTypeValidation =>
                    CimValidationErrorMessages.OperationTypeValidationErrorMessage,
                ValidationRuleIdentifier.ChargeIdLengthValidation =>
                    CimValidationErrorMessages.ChargeIdLengthValidationErrorMessage,
                ValidationRuleIdentifier.ChargeIdRequiredValidation =>
                    CimValidationErrorMessages.ChargeIdRequiredValidationErrorMessage,
                ValidationRuleIdentifier.DocumentTypeMustBeRequestUpdateChargeInformation =>
                    CimValidationErrorMessages.DocumentTypeMustBeRequestUpdateChargeInformationErrorMessage,
                ValidationRuleIdentifier.BusinessReasonCodeMustBeUpdateChargeInformation =>
                    CimValidationErrorMessages.BusinessReasonCodeMustBeUpdateChargeInformationErrorMessage,
                ValidationRuleIdentifier.ChargeTypeIsKnownValidation =>
                    CimValidationErrorMessages.ChargeTypeIsKnownValidationErrorMessage,
                ValidationRuleIdentifier.VatClassificationValidation =>
                    CimValidationErrorMessages.VatClassificationValidationErrorMessage,
                ValidationRuleIdentifier.ResolutionTariffValidation =>
                    CimValidationErrorMessages.ResolutionTariffValidationErrorMessage,
                ValidationRuleIdentifier.ResolutionFeeValidation =>
                    CimValidationErrorMessages.ResolutionFeeValidationErrorMessage,
                ValidationRuleIdentifier.ResolutionSubscriptionValidation =>
                    CimValidationErrorMessages.ResolutionSubscriptionValidationErrorMessage,
                ValidationRuleIdentifier.StartDateTimeRequiredValidation =>
                    CimValidationErrorMessages.StartDateTimeRequiredValidationErrorMessage,
                ValidationRuleIdentifier.ChargeOwnerIsRequiredValidation =>
                    CimValidationErrorMessages.ChargeOwnerIsRequiredValidationErrorMessage,
                ValidationRuleIdentifier.ChargeNameHasMaximumLength =>
                    CimValidationErrorMessages.ChargeNameHasMaximumLengthErrorMessage,
                ValidationRuleIdentifier.ChargeDescriptionHasMaximumLength =>
                    CimValidationErrorMessages.ChargeDescriptionHasMaximumLengthErrorMessage,
                ValidationRuleIdentifier.ChargeTypeTariffPriceCount =>
                    CimValidationErrorMessages.ChargeTypeTariffPriceCountErrorMessage,
                ValidationRuleIdentifier.MaximumPrice =>
                    CimValidationErrorMessages.MaximumPriceErrorMessage,
                ValidationRuleIdentifier.ChargePriceMaximumDigitsAndDecimals =>
                    CimValidationErrorMessages.ChargePriceMaximumDigitsAndDecimalsErrorMessage,
                ValidationRuleIdentifier.FeeMustHaveSinglePrice =>
                    CimValidationErrorMessages.FeeMustHaveSinglePriceErrorMessage,
                ValidationRuleIdentifier.SubscriptionMustHaveSinglePrice =>
                    CimValidationErrorMessages.SubscriptionMustHaveSinglePriceErrorMessage,
                ValidationRuleIdentifier.CommandSenderMustBeAnExistingMarketParticipant =>
                    CimValidationErrorMessages.CommandSenderMustBeAnExistingMarketParticipantErrorMessage,
                ValidationRuleIdentifier.UpdateNotYetSupported =>
                    CimValidationErrorMessages.UpdateNotYetSupportedErrorMessage,
                _ => CimValidationErrorMessages.UnknownError,
            };
        }
    }
}
