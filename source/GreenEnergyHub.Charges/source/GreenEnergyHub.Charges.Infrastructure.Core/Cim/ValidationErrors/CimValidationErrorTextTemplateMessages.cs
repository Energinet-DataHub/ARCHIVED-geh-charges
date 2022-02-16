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

using GreenEnergyHub.Charges.Domain.Dtos.Validation;

namespace GreenEnergyHub.Charges.Infrastructure.Core.Cim.ValidationErrors
{
    /// <summary>
    /// This class contains templates for error messages texts for rejection messages to market actors.
    /// Each public property contains the template for an error message corresponding to a specific
    /// <see cref="ValidationRuleIdentifier"/> designated by the <see cref="ErrorMessageForAttribute"/>.
    /// The template texts can contain placeholders for data from the Charges domain. All valid
    /// placeholders are defined in <see cref="CimValidationErrorTextToken"/>.
    /// </summary>
    public static class CimValidationErrorTextTemplateMessages
    {
        [ErrorMessageFor(ValidationRuleIdentifier.StartDateValidation)]
        public const string StartDateValidationErrorText =
            "Effective date {{ChargeStartDateTime}} incorrect: The information is not received within the correct time period (either too soon or too late)";

        [ErrorMessageFor(ValidationRuleIdentifier.ChangingTariffVatValueNotAllowed)]
        public const string ChangingTariffVatValueNotAllowedErrorText =
            "VAT class {{ChargeVatClass}} not allowed: charge {{DocumentSenderProvidedChargeId}} cannot be updated with another entitlement for VAT";

        [ErrorMessageFor(ValidationRuleIdentifier.ChangingTariffTaxValueNotAllowed)]
        public const string ChangingTariffTaxValueNotAllowedErrorText =
            "It is not allowed to change the tax indicator to {{ChargeTaxIndicator}} for charge {{DocumentSenderProvidedChargeId}}";

        [ErrorMessageFor(ValidationRuleIdentifier.SenderIsMandatoryTypeValidation)]
        public const string SenderIsMandatoryTypeValidationErrorText =
            "Sender is missing for message {{DocumentId}}";

        [ErrorMessageFor(ValidationRuleIdentifier.RecipientIsMandatoryTypeValidation)]
        public const string RecipientIsMandatoryTypeValidationErrorText =
            "Recipient is missing for message {{DocumentId}}";

        [ErrorMessageFor(ValidationRuleIdentifier.ChargeOperationIdRequired)]
        public const string ChargeOperationIdRequiredErrorText =
            "Identification is missing: transaction can not be processed for document {{DocumentId}}";

        [ErrorMessageFor(ValidationRuleIdentifier.ChargeIdLengthValidation)]
        public const string ChargeIdLengthValidationErrorText =
            "Charge ID {{DocumentSenderProvidedChargeId}} has a length that exceeds 10";

        [ErrorMessageFor(ValidationRuleIdentifier.ChargeIdRequiredValidation)]
        public const string ChargeIdRequiredValidationErrorText =
            "Identification is missing, charge can not be processed";

        [ErrorMessageFor(ValidationRuleIdentifier.DocumentTypeMustBeRequestUpdateChargeInformation)]
        public const string DocumentTypeMustBeRequestUpdateChargeInformationErrorText =
            "Document type {{DocumentType}} not allowed together with energy business process {{DocumentBusinessReasonCode}}";

        [ErrorMessageFor(ValidationRuleIdentifier.BusinessReasonCodeMustBeUpdateChargeInformation)]
        public const string BusinessReasonCodeMustBeUpdateChargeInformationErrorText =
            "Energy business process {{DocumentBusinessReasonCode}} not allowed together with document type {{DocumentType}}";

        [ErrorMessageFor(ValidationRuleIdentifier.ChargeTypeIsKnownValidation)]
        public const string ChargeTypeIsKnownValidationErrorText =
            "Charge type {{ChargeType}} for charge {{DocumentSenderProvidedChargeId}} has wrong value (outside domain)";

        [ErrorMessageFor(ValidationRuleIdentifier.VatClassificationValidation)]
        public const string VatClassificationValidationErrorText =
            "VAT class {{ChargeVatClass}} for charge {{DocumentSenderProvidedChargeId}} has wrong value (outside domain)";

        [ErrorMessageFor(ValidationRuleIdentifier.ResolutionTariffValidation)]
        public const string ResolutionTariffValidationErrorText =
            "Period type {{ChargeResolution}} not allowed: The specified resolution for charge {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} must be Day or Hour";

        [ErrorMessageFor(ValidationRuleIdentifier.ResolutionFeeValidation)]
        public const string ResolutionFeeValidationErrorText =
            "Period type {{ChargeResolution}} not allowed: The specified resolution for charge {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} must be Day";

        [ErrorMessageFor(ValidationRuleIdentifier.ResolutionSubscriptionValidation)]
        public const string ResolutionSubscriptionValidationErrorText =
            "Period type {{ChargeResolution}} not allowed: The specified resolution for charge {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} must be Month";

        [ErrorMessageFor(ValidationRuleIdentifier.StartDateTimeRequiredValidation)]
        public const string StartDateTimeRequiredValidationErrorText =
            "Occurrence date is missing for charge {{DocumentSenderProvidedChargeId}}";

        [ErrorMessageFor(ValidationRuleIdentifier.ChargeOwnerIsRequiredValidation)]
        public const string ChargeOwnerIsRequiredValidationErrorText =
            "Owner is missing for charge {{DocumentSenderProvidedChargeId}}";

        [ErrorMessageFor(ValidationRuleIdentifier.ChargeNameHasMaximumLength)]
        public const string ChargeNameHasMaximumLengthErrorText =
            "Name {{ChargeName}} for charge {{DocumentSenderProvidedChargeId}} has a length that exceeds 50";

        [ErrorMessageFor(ValidationRuleIdentifier.ChargeDescriptionHasMaximumLength)]
        public const string ChargeDescriptionHasMaximumLengthErrorText =
            "Description {{ChargeDescription}} for charge {{DocumentSenderProvidedChargeId}} has a length that exceeds 2048";

        [ErrorMessageFor(ValidationRuleIdentifier.ChargeTypeTariffPriceCount)]
        public const string ChargeTypeTariffPriceCountErrorText =
            "The number of prices {{ChargePointsCount}} for charge {{DocumentSenderProvidedChargeId}} doesn't match period type {{ChargeResolution}}";

        [ErrorMessageFor(ValidationRuleIdentifier.MaximumPrice)]
        public const string MaximumPriceErrorText =
            "Price {{ChargePointPrice}} not allowed: The specified charge price for position {{ChargePointPosition}} is not plausible (too large)";

        [ErrorMessageFor(ValidationRuleIdentifier.ChargePriceMaximumDigitsAndDecimals)]
        public const string ChargePriceMaximumDigitsAndDecimalsErrorText =
            "Energy price {{ChargePointPrice}} for charge {{DocumentSenderProvidedChargeId}} contains a non-digit character, has a length that exceeds 15 or does not comply with format '99999999.999999'";

        [ErrorMessageFor(ValidationRuleIdentifier.FeeMustHaveSinglePrice)]
        public const string FeeMustHaveSinglePriceErrorText =
            "The number of prices {{ChargePointsCount}} for charge {{DocumentSenderProvidedChargeId}} doesn't match period type {{ChargeResolution}}";

        [ErrorMessageFor(ValidationRuleIdentifier.SubscriptionMustHaveSinglePrice)]
        public const string SubscriptionMustHaveSinglePriceErrorText =
            "The number of prices {{ChargePointsCount}} for charge {{DocumentSenderProvidedChargeId}} doesn't match period type {{ChargeResolution}}";

        [ErrorMessageFor(ValidationRuleIdentifier.CommandSenderMustBeAnExistingMarketParticipant)]
        public const string CommandSenderMustBeAnExistingMarketParticipantErrorText =
            "Sender {{DocumentSenderId}} for message {{DocumentId}} is currently not an existing market party (company) or not active";

        [ErrorMessageFor(ValidationRuleIdentifier.ChargeUpdateNotYetSupported)]
        public const string ChargeUpdateNotYetSupportedErrorText =
            "Charge ID {{DocumentSenderProvidedChargeId}} for owner {{ChargeOwner}} of type {{ChargeType}} cannot yet be updated or stopped. The functionality is not implemented yet.";

        [ErrorMessageFor(ValidationRuleIdentifier.MeteringPointDoesNotExist)]
        public const string MeteringPointDoesNotExistValidationErrorText =
            "GSRN-code {{MeteringPointId}} is unknown: The specified metering point has not been registered in the system on the charge link start date";

        [ErrorMessageFor(ValidationRuleIdentifier.ChargeDoesNotExist)]
        public const string ChargeDoesNotExistValidationErrorText =
            "Charge {{DocumentSenderProvidedChargeId}} not allowed: The charge is not an existing charge on date {{ChargeLinkStartDate}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.ChargeLinkUpdateNotYetSupported)]
        public const string ChargeLinksUpdateNotYetSupportedErrorText =
            "Charge link for metering point ID {{MeteringPointId}} and Charge with ID {{DocumentSenderProvidedChargeId}} for owner {{ChargeOwner}} of type {{ChargeType}} cannot yet be updated or stopped. The functionality is not implemented yet.";

        [ErrorMessageFor(ValidationRuleIdentifier.StopChargeNotYetSupported)]
        public const string StopChargeNotYetSupportedErrorText =
            "Charge ID {{DocumentSenderProvidedChargeId}} for owner {{ChargeOwner}} of type {{ChargeType}} cannot stopped. The functionality is not yet implemented.";

        public const string Unknown = "unknown";
    }
}
