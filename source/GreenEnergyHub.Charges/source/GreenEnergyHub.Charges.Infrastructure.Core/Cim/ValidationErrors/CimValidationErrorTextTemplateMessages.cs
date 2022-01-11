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
    /// <summary>
    /// This class contains templates for error messages texts for rejection messages to market actors.
    /// Each public property contains the template for an error message corresponding to a specific
    /// <see cref="ValidationRuleIdentifier"/> designated by the <see cref="ErrormessageForAttribute"/>.
    /// The template texts can contain placeholders for data from the Charges domain. All valid
    /// placeholders are defined in <see cref="CimValidationErrorTextToken"/>.
    /// </summary>
    public static class CimValidationErrorTextTemplateMessages
    {
        [ErrormessageFor(ValidationRuleIdentifier.StartDateValidation)]
        public const string StartDateValidationErrorText =
            "Effectuation date {{ChargeStartDateTime}} incorrect: The information is not received within the correct time period (either too soon or too late)";

        [ErrormessageFor(ValidationRuleIdentifier.ChangingTariffVatValueNotAllowed)]
        public const string ChangingTariffVatValueNotAllowedErrorText =
            "VAT class {{ChargeVatClass}} not allowed: charge {{DocumentSenderProvidedChargeId}} cannot be updated with another entitlement for VAT";

        [ErrormessageFor(ValidationRuleIdentifier.ChangingTariffTaxValueNotAllowed)]
        public const string ChangingTariffTaxValueNotAllowedErrorText =
            "Tax indicator {{ChargeTaxIndicator}} not allowed: charge {{DocumentSenderProvidedChargeId}} cannot be updated with another Tax indicator";

        [ErrormessageFor(ValidationRuleIdentifier.SenderIsMandatoryTypeValidation)]
        public const string SenderIsMandatoryTypeValidationErrorText =
            "Sender is missing for message {{DocumentId}}";

        [ErrormessageFor(ValidationRuleIdentifier.RecipientIsMandatoryTypeValidation)]
        public const string RecipientIsMandatoryTypeValidationErrorText =
            "Recipient is missing for message {{DocumentId}}";

        [ErrormessageFor(ValidationRuleIdentifier.ChargeOperationIdRequired)]
        public const string ChargeOperationIdRequiredErrorText =
            "Identification is missing: transaction can not be processed for document {{DocumentId}}";

        [ErrormessageFor(ValidationRuleIdentifier.ChargeIdLengthValidation)]
        public const string ChargeIdLengthValidationErrorText =
            "Charge ID {{DocumentSenderProvidedChargeId}} has a length that exceeds 10";

        [ErrormessageFor(ValidationRuleIdentifier.ChargeIdRequiredValidation)]
        public const string ChargeIdRequiredValidationErrorText =
            "Identification is missing, charge can not be processed";

        [ErrormessageFor(ValidationRuleIdentifier.DocumentTypeMustBeRequestUpdateChargeInformation)]
        public const string DocumentTypeMustBeRequestUpdateChargeInformationErrorText =
            "Document type {{DocumentType}} not allowed together with energy business process {{DocumentBusinessReasonCode}}";

        [ErrormessageFor(ValidationRuleIdentifier.BusinessReasonCodeMustBeUpdateChargeInformation)]
        public const string BusinessReasonCodeMustBeUpdateChargeInformationErrorText =
            "Energy business process {{DocumentBusinessReasonCode}} not allowed together with document type {{DocumentType}}";

        [ErrormessageFor(ValidationRuleIdentifier.ChargeTypeIsKnownValidation)]
        public const string ChargeTypeIsKnownValidationErrorText =
            "Charge type {{ChargeType}} for charge {{DocumentSenderProvidedChargeId}} has wrong value (outside domain)";

        [ErrormessageFor(ValidationRuleIdentifier.VatClassificationValidation)]
        public const string VatClassificationValidationErrorText =
            "VAT class {{ChargeVatClass}} for charge {{DocumentSenderProvidedChargeId}} has wrong value (outside domain)";

        [ErrormessageFor(ValidationRuleIdentifier.ResolutionTariffValidation)]
        public const string ResolutionTariffValidationErrorText =
            "Period type {{ChargeResolution}} not allowed: The specified resolution for charge {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} must be Day or Hour";

        [ErrormessageFor(ValidationRuleIdentifier.ResolutionFeeValidation)]
        public const string ResolutionFeeValidationErrorText =
            "Period type {{ChargeResolution}} not allowed: The specified resolution for charge {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} must be Day";

        [ErrormessageFor(ValidationRuleIdentifier.ResolutionSubscriptionValidation)]
        public const string ResolutionSubscriptionValidationErrorText =
            "Period type {{ChargeResolution}} not allowed: The specified resolution for charge {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} must be Month";

        [ErrormessageFor(ValidationRuleIdentifier.StartDateTimeRequiredValidation)]
        public const string StartDateTimeRequiredValidationErrorText =
            "Occurrence date is missing for charge {{DocumentSenderProvidedChargeId}}";

        [ErrormessageFor(ValidationRuleIdentifier.ChargeOwnerIsRequiredValidation)]
        public const string ChargeOwnerIsRequiredValidationErrorText =
            "Owner is missing for charge {{DocumentSenderProvidedChargeId}}";

        [ErrormessageFor(ValidationRuleIdentifier.ChargeNameHasMaximumLength)]
        public const string ChargeNameHasMaximumLengthErrorText =
            "Name {{ChargeName}} for charge {{DocumentSenderProvidedChargeId}} has a length that exceeds 50";

        [ErrormessageFor(ValidationRuleIdentifier.ChargeDescriptionHasMaximumLength)]
        public const string ChargeDescriptionHasMaximumLengthErrorText =
            "Description {{ChargeDescription}} for charge {{DocumentSenderProvidedChargeId}} has a length that exceeds 2048";

        [ErrormessageFor(ValidationRuleIdentifier.ChargeTypeTariffPriceCount)]
        public const string ChargeTypeTariffPriceCountErrorText =
            "The number of prices {{ChargePointsCount}} for charge {{DocumentSenderProvidedChargeId}} doesn't match period type {{ChargeResolution}}";

        [ErrormessageFor(ValidationRuleIdentifier.MaximumPrice)]
        public const string MaximumPriceErrorText =
            "Price {{ChargePointPrice}} not allowed: The specified charge price for position {{ChargePointPosition}} is not plausible (too large)";

        [ErrormessageFor(ValidationRuleIdentifier.ChargePriceMaximumDigitsAndDecimals)]
        public const string ChargePriceMaximumDigitsAndDecimalsErrorText =
            "Energy price {{ChargePointPrice}} for charge {{DocumentSenderProvidedChargeId}} contains a non-digit character, has a length that exceeds 15 or does not comply with format '99999999.999999'";

        [ErrormessageFor(ValidationRuleIdentifier.FeeMustHaveSinglePrice)]
        public const string FeeMustHaveSinglePriceErrorText =
            "The number of prices {{ChargePointsCount}} for charge {{DocumentSenderProvidedChargeId}} doesn't match period type {{ChargeResolution}}";

        [ErrormessageFor(ValidationRuleIdentifier.SubscriptionMustHaveSinglePrice)]
        public const string SubscriptionMustHaveSinglePriceErrorText =
            "The number of prices {{ChargePointsCount}} for charge {{DocumentSenderProvidedChargeId}} doesn't match period type {{ChargeResolution}}";

        [ErrormessageFor(ValidationRuleIdentifier.CommandSenderMustBeAnExistingMarketParticipant)]
        public const string CommandSenderMustBeAnExistingMarketParticipantErrorText =
            "Sender {{DocumentSenderId}} for message {{DocumentId}} is currently not an existing market party (company) or not active";

        [ErrormessageFor(ValidationRuleIdentifier.UpdateNotYetSupported)]
        public const string UpdateNotYetSupportedErrorText =
            "Charge ID {{DocumentSenderProvidedChargeId}} for owner {{ChargeOwner}} of type {{ChargeType}} cannot yet be updated or stopped. The functionality is not implemented yet.";
    }
}
