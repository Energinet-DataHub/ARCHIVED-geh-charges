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
    public static class CimValidationErrorTextTemplateMessages
    {
        [ErrormessageFor(ValidationRuleIdentifier.StartDateValidation)]
        [Tokens(CimValidationErrorTextToken.ChargeStartDateTime)]
        public const string StartDateValidationErrorText =
            "Effectuation date {{ChargeStartDateTime}} incorrect: The information is not received within the correct time period (either too soon or too late)";

        [ErrormessageFor(ValidationRuleIdentifier.ChangingTariffVatValueNotAllowed)]
        [Tokens(CimValidationErrorTextToken.ChargeVatClass, CimValidationErrorTextToken.DocumentSenderProvidedChargeId)]
        public const string ChangingTariffVatValueNotAllowedErrorText =
            "VAT class {{ChargeVatClass}} not allowed: charge {{DocumentSenderProvidedChargeId}} cannot be updated with another entitlement for VAT";

        [ErrormessageFor(ValidationRuleIdentifier.ChangingTariffTaxValueNotAllowed)]
        [Tokens(
            CimValidationErrorTextToken.ChargeTaxIndicator,
            CimValidationErrorTextToken.DocumentSenderProvidedChargeId)]
        public const string ChangingTariffTaxValueNotAllowedErrorText =
            "Tax indicator {{ChargeTaxIndicator}} not allowed: charge {{DocumentSenderProvidedChargeId}} cannot be updated with another Tax indicator";

        [ErrormessageFor(ValidationRuleIdentifier.SenderIsMandatoryTypeValidation)]
        [Tokens(CimValidationErrorTextToken.DocumentId)]
        public const string SenderIsMandatoryTypeValidationErrorText =
            "Sender is missing for message {{DocumentId}}";

        [ErrormessageFor(ValidationRuleIdentifier.RecipientIsMandatoryTypeValidation)]
        [Tokens(CimValidationErrorTextToken.DocumentId)]
        public const string RecipientIsMandatoryTypeValidationErrorText =
            "Recipient is missing for message {{DocumentId}}";

        [ErrormessageFor(ValidationRuleIdentifier.ChargeOperationIdRequired)]
        [Tokens(CimValidationErrorTextToken.DocumentId)]
        public const string ChargeOperationIdRequiredErrorText =
            "Identification is missing: transaction can not be processed for document {{DocumentId}}";

        [ErrormessageFor(ValidationRuleIdentifier.ChargeIdLengthValidation)]
        [Tokens(CimValidationErrorTextToken.DocumentSenderProvidedChargeId)]
        public const string ChargeIdLengthValidationErrorText =
            "Charge ID {{DocumentSenderProvidedChargeId}} has a length that exceeds 10";

        [ErrormessageFor(ValidationRuleIdentifier.ChargeIdRequiredValidation)]
        public const string ChargeIdRequiredValidationErrorText =
            "Identification is missing, charge can not be processed";

        [ErrormessageFor(ValidationRuleIdentifier.DocumentTypeMustBeRequestUpdateChargeInformation)]
        [Tokens(CimValidationErrorTextToken.DocumentType, CimValidationErrorTextToken.DocumentBusinessReasonCode)]
        public const string DocumentTypeMustBeRequestUpdateChargeInformationErrorText =
            "Document type {{DocumentType}} not allowed together with energy business process {{DocumentBusinessReasonCode}}";

        [ErrormessageFor(ValidationRuleIdentifier.BusinessReasonCodeMustBeUpdateChargeInformation)]
        [Tokens(CimValidationErrorTextToken.DocumentBusinessReasonCode, CimValidationErrorTextToken.DocumentType)]
        public const string BusinessReasonCodeMustBeUpdateChargeInformationErrorText =
            "Energy business process {{DocumentBusinessReasonCode}} not allowed together with document type {{DocumentType}}";

        [ErrormessageFor(ValidationRuleIdentifier.ChargeTypeIsKnownValidation)]
        [Tokens(CimValidationErrorTextToken.ChargeType, CimValidationErrorTextToken.DocumentSenderProvidedChargeId)]
        public const string ChargeTypeIsKnownValidationErrorText =
            "Charge type {{ChargeType}} for charge {{DocumentSenderProvidedChargeId}} has wrong value (outside domain)";

        [ErrormessageFor(ValidationRuleIdentifier.VatClassificationValidation)]
        [Tokens(
            CimValidationErrorTextToken.ChargeVatClass,
            CimValidationErrorTextToken.DocumentSenderProvidedChargeId)]
        public const string VatClassificationValidationErrorText =
            "VAT class {{ChargeVatClass}} for charge {{DocumentSenderProvidedChargeId}} has wrong value (outside domain)";

        [ErrormessageFor(ValidationRuleIdentifier.ResolutionTariffValidation)]
        [Tokens(
            CimValidationErrorTextToken.ChargeResolution,
            CimValidationErrorTextToken.DocumentSenderProvidedChargeId,
            CimValidationErrorTextToken.ChargeType)]
        public const string ResolutionTariffValidationErrorText =
            "Period type {{ChargeResolution}} not allowed: The specified resolution for charge {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} must be Day or Hour";

        [ErrormessageFor(ValidationRuleIdentifier.ResolutionFeeValidation)]
        [Tokens(
            CimValidationErrorTextToken.ChargeResolution,
            CimValidationErrorTextToken.DocumentSenderProvidedChargeId,
            CimValidationErrorTextToken.ChargeType)]
        public const string ResolutionFeeValidationErrorText =
            "Period type {{ChargeResolution}} not allowed: The specified resolution for charge {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} must be Day";

        [ErrormessageFor(ValidationRuleIdentifier.ResolutionSubscriptionValidation)]
        [Tokens(
            CimValidationErrorTextToken.ChargeResolution,
            CimValidationErrorTextToken.DocumentSenderProvidedChargeId,
            CimValidationErrorTextToken.ChargeType)]
        public const string ResolutionSubscriptionValidationErrorText =
            "Period type {{ChargeResolution}} not allowed: The specified resolution for charge {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} must be Month";

        [ErrormessageFor(ValidationRuleIdentifier.StartDateTimeRequiredValidation)]
        [Tokens(CimValidationErrorTextToken.DocumentSenderProvidedChargeId)]
        public const string StartDateTimeRequiredValidationErrorText =
            "Occurrence date is missing for charge {{DocumentSenderProvidedChargeId}}";

        [ErrormessageFor(ValidationRuleIdentifier.ChargeOwnerIsRequiredValidation)]
        [Tokens(CimValidationErrorTextToken.DocumentSenderProvidedChargeId)]
        public const string ChargeOwnerIsRequiredValidationErrorText =
            "Owner is missing for charge {{DocumentSenderProvidedChargeId}}";

        [ErrormessageFor(ValidationRuleIdentifier.ChargeNameHasMaximumLength)]
        [Tokens(CimValidationErrorTextToken.ChargeName, CimValidationErrorTextToken.DocumentSenderProvidedChargeId)]
        public const string ChargeNameHasMaximumLengthErrorText =
            "Name {{ChargeName}} for charge {{DocumentSenderProvidedChargeId}} has a length that exceeds 50";

        [ErrormessageFor(ValidationRuleIdentifier.ChargeDescriptionHasMaximumLength)]
        [Tokens(
            CimValidationErrorTextToken.ChargeDescription,
            CimValidationErrorTextToken.DocumentSenderProvidedChargeId)]
        public const string ChargeDescriptionHasMaximumLengthErrorText =
            "Description {{ChargeDescription}} for charge {{DocumentSenderProvidedChargeId}} has a length that exceeds 2048";

        [ErrormessageFor(ValidationRuleIdentifier.ChargeTypeTariffPriceCount)]
        [Tokens(
            CimValidationErrorTextToken.ChargePointsCount,
            CimValidationErrorTextToken.DocumentSenderProvidedChargeId,
            CimValidationErrorTextToken.ChargeResolution)]
        public const string ChargeTypeTariffPriceCountErrorText =
            "The number of prices {{ChargePointsCount}} for charge {{DocumentSenderProvidedChargeId}} doesn't match period type {{ChargeResolution}}";

        [ErrormessageFor(ValidationRuleIdentifier.MaximumPrice)]
        [Tokens(CimValidationErrorTextToken.ChargePointPrice, CimValidationErrorTextToken.ChargePointPosition)]
        public const string MaximumPriceErrorText =
            "Price {{ChargePointPrice}} not allowed: The specified charge price for position {{ChargePointPosition}} is not plausible (too large)";

        [ErrormessageFor(ValidationRuleIdentifier.ChargePriceMaximumDigitsAndDecimals)]
        [Tokens(CimValidationErrorTextToken.ChargePointPrice, CimValidationErrorTextToken.DocumentSenderProvidedChargeId)]
        public const string ChargePriceMaximumDigitsAndDecimalsErrorText =
            "Energy price {{ChargePointPrice}} for charge {{DocumentSenderProvidedChargeId}} contains a non-digit character, has a length that exceeds 15 or does not comply with format '99999999.999999'";

        [ErrormessageFor(ValidationRuleIdentifier.FeeMustHaveSinglePrice)]
        [Tokens(
            CimValidationErrorTextToken.ChargePointsCount,
            CimValidationErrorTextToken.DocumentSenderProvidedChargeId,
            CimValidationErrorTextToken.ChargeResolution)]
        public const string FeeMustHaveSinglePriceErrorText =
            "The number of prices {{ChargePointsCount}} for charge {{DocumentSenderProvidedChargeId}} doesn't match period type {{ChargeResolution}}";

        [ErrormessageFor(ValidationRuleIdentifier.SubscriptionMustHaveSinglePrice)]
        [Tokens(
            CimValidationErrorTextToken.ChargePointsCount,
            CimValidationErrorTextToken.DocumentSenderProvidedChargeId,
            CimValidationErrorTextToken.ChargeResolution)]
        public const string SubscriptionMustHaveSinglePriceErrorText =
            "The number of prices {{ChargePointsCount}} for charge {{DocumentSenderProvidedChargeId}} doesn't match period type {{ChargeResolution}}";

        [ErrormessageFor(ValidationRuleIdentifier.CommandSenderMustBeAnExistingMarketParticipant)]
        [Tokens(CimValidationErrorTextToken.DocumentSenderId, CimValidationErrorTextToken.DocumentId)]
        public const string CommandSenderMustBeAnExistingMarketParticipantErrorText =
            "Sender {{DocumentSenderId}} for message {{DocumentId}} is currently not an existing market party (company) or not active";

        [ErrormessageFor(ValidationRuleIdentifier.UpdateNotYetSupported)]
        [Tokens(
            CimValidationErrorTextToken.DocumentSenderProvidedChargeId,
            CimValidationErrorTextToken.ChargeOwner,
            CimValidationErrorTextToken.ChargeType)]
        public const string UpdateNotYetSupportedErrorText =
            "Charge ID {{DocumentSenderProvidedChargeId}} for owner {{ChargeOwner}} of type {{ChargeType}} cannot yet be updated or stopped. The functionality is not implemented yet.";
    }
}
