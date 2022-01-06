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
        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.StartDateValidation"/>
        /// using <see cref="CimValidationErrorTextToken.ChargeStartDateTime"/>
        /// </summary>
        public const string StartDateValidationErrorText =
            "Effectuation date {{ChargeStartDateTime}} incorrect: The information is not received within the correct time period (either too soon or too late)";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.ChangingTariffVatValueNotAllowed"/>
        /// using <see cref="CimValidationErrorTextToken.ChargeVatClass"/>
        /// and <see cref="CimValidationErrorTextToken.DocumentSenderProvidedChargeId"/>
        /// </summary>
        public const string ChangingTariffVatValueNotAllowedErrorText =
            "VAT class {{ChargeVatClass}} not allowed: charge {{DocumentSenderProvidedChargeId}} cannot be updated with another entitlement for VAT";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.ChangingTariffTaxValueNotAllowed"/>
        /// using <see cref="CimValidationErrorTextToken.ChargeTaxIndicator"/>
        /// and <see cref="CimValidationErrorTextToken.DocumentSenderProvidedChargeId"/>
        /// </summary>
        public const string ChangingTariffTaxValueNotAllowedErrorText =
            "Tax indicator {{ChargeTaxIndicator}} not allowed: charge {{DocumentSenderProvidedChargeId}} cannot be updated with another Tax indicator";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.SenderIsMandatoryTypeValidation"/>
        /// using <see cref="CimValidationErrorTextToken.DocumentId"/>
        /// </summary>
        public const string SenderIsMandatoryTypeValidationErrorText =
            "Sender is missing for message {{DocumentId}}";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.RecipientIsMandatoryTypeValidation"/>
        /// using <see cref="CimValidationErrorTextToken.DocumentId"/>
        /// </summary>
        public const string RecipientIsMandatoryTypeValidationErrorText =
            "Recipient is missing for message {{DocumentId}}";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.ChargeOperationIdRequired"/>
        /// using <see cref="CimValidationErrorTextToken.DocumentId"/>
        /// </summary>
        public const string ChargeOperationIdRequiredErrorText =
            "Identification is missing: transaction can not be processed for document {{DocumentId}}";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.ChargeIdLengthValidation"/>
        /// using <see cref="CimValidationErrorTextToken.DocumentSenderProvidedChargeId"/>
        /// </summary>
        public const string ChargeIdLengthValidationErrorText =
            "Charge ID {{DocumentSenderProvidedChargeId}} has a length that exceeds 10";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.ChargeIdRequiredValidation"/>
        /// </summary>
        public const string ChargeIdRequiredValidationErrorText =
            "Identification is missing, charge can not be processed";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.DocumentTypeMustBeRequestUpdateChargeInformation"/>
        /// using <see cref="CimValidationErrorTextToken.DocumentType"/>
        /// and <see cref="CimValidationErrorTextToken.DocumentBusinessReasonCode"/>
        /// </summary>
        public const string DocumentTypeMustBeRequestUpdateChargeInformationErrorText =
            "Document type {{DocumentType}} not allowed together with energy business process {{DocumentBusinessReasonCode}}";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.BusinessReasonCodeMustBeUpdateChargeInformation"/>
        /// using <see cref="CimValidationErrorTextToken.DocumentBusinessReasonCode"/>
        /// and <see cref="CimValidationErrorTextToken.DocumentType"/>
        /// </summary>
        public const string BusinessReasonCodeMustBeUpdateChargeInformationErrorText =
            "Energy business process {{DocumentBusinessReasonCode}} not allowed together with document type {{DocumentType}}";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.ChargeTypeIsKnownValidation"/>
        /// using <see cref="CimValidationErrorTextToken.ChargeType"/>
        /// and <see cref="CimValidationErrorTextToken.DocumentSenderProvidedChargeId"/>
        /// </summary>
        public const string ChargeTypeIsKnownValidationErrorText =
            "Charge type {{ChargeType}} for charge {{DocumentSenderProvidedChargeId}} has wrong value (outside domain)";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.VatClassificationValidation"/>
        /// using <see cref="CimValidationErrorTextToken.ChargeVatClass"/>
        /// and <see cref="CimValidationErrorTextToken.DocumentSenderProvidedChargeId"/>
        /// </summary>
        public const string VatClassificationValidationErrorText =
            "VAT class {{ChargeVatClass}} for charge {{DocumentSenderProvidedChargeId}} has wrong value (outside domain)";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.ResolutionTariffValidation"/>
        /// using <see cref="CimValidationErrorTextToken.ChargeResolution"/>
        /// <see cref="CimValidationErrorTextToken.DocumentSenderProvidedChargeId"/>
        /// and <see cref="CimValidationErrorTextToken.ChargeType"/>
        /// </summary>
        public const string ResolutionTariffValidationErrorText =
            "Period type {{ChargeResolution}} not allowed: The specified resolution for charge {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} must be Day or Hour";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.ResolutionFeeValidation"/>
        /// using <see cref="CimValidationErrorTextToken.ChargeResolution"/>,
        /// <see cref="CimValidationErrorTextToken.DocumentSenderProvidedChargeId"/>
        /// and <see cref="CimValidationErrorTextToken.ChargeType"/>
        /// </summary>
        public const string ResolutionFeeValidationErrorText =
            "Period type {{ChargeResolution}} not allowed: The specified resolution for charge {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} must be Day";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.ResolutionSubscriptionValidation"/>,
        /// using <see cref="CimValidationErrorTextToken.ChargeResolution"/>
        /// <see cref="CimValidationErrorTextToken.DocumentSenderProvidedChargeId"/>
        /// and <see cref="CimValidationErrorTextToken.ChargeType"/>
        /// </summary>
        public const string ResolutionSubscriptionValidationErrorText =
            "Period type {{ChargeResolution}} not allowed: The specified resolution for charge {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} must be Month";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.StartDateTimeRequiredValidation"/>
        /// using <see cref="CimValidationErrorTextToken.DocumentSenderProvidedChargeId"/>
        /// </summary>
        public const string StartDateTimeRequiredValidationErrorText =
            "Occurrence date is missing for charge {{DocumentSenderProvidedChargeId}}";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.ChargeOwnerIsRequiredValidation"/>
        /// using <see cref="CimValidationErrorTextToken.DocumentSenderProvidedChargeId"/>
        /// </summary>
        public const string ChargeOwnerIsRequiredValidationErrorText =
            "Owner is missing for charge {{DocumentSenderProvidedChargeId}}";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.ChargeNameHasMaximumLength"/>
        /// using <see cref="CimValidationErrorTextToken.ChargeName"/>
        /// using <see cref="CimValidationErrorTextToken.DocumentSenderProvidedChargeId"/>
        /// </summary>
        public const string ChargeNameHasMaximumLengthErrorText =
            "Name {{ChargeName}} for charge {{DocumentSenderProvidedChargeId}} has a length that exceeds 50";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.ChargeDescriptionHasMaximumLength"/>
        /// using <see cref="CimValidationErrorTextToken.ChargeDescription"/>
        /// and <see cref="CimValidationErrorTextToken.DocumentSenderProvidedChargeId"/>
        /// </summary>
        public const string ChargeDescriptionHasMaximumLengthErrorText =
            "Description {{ChargeDescription}} for charge {{DocumentSenderProvidedChargeId}} has a length that exceeds 2048";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.ChargeTypeTariffPriceCount"/>
        /// using <see cref="CimValidationErrorTextToken.ChargePointsCount"/>
        /// <see cref="CimValidationErrorTextToken.DocumentSenderProvidedChargeId"/>,
        /// and <see cref="CimValidationErrorTextToken.ChargeResolution"/>
        /// </summary>
        public const string ChargeTypeTariffPriceCountErrorText =
            "The number of prices {{ChargePointsCount}} for charge {{DocumentSenderProvidedChargeId}} doesn't match period type {{ChargeResolution}}";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.MaximumPrice"/>
        /// using <see cref="CimValidationErrorTextToken.ChargePointPrice"/>
        /// and <see cref="CimValidationErrorTextToken.ChargePointPosition"/>
        /// </summary>
        public const string MaximumPriceErrorText =
            "Price {{ChargePointPrice}} not allowed: The specified charge price for position {{ChargePointPosition}} is not plausible (too large)";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.ChargePriceMaximumDigitsAndDecimals"/>
        /// using <see cref="CimValidationErrorTextToken.ChargePointPrice"/>
        /// and <see cref="CimValidationErrorTextToken.DocumentSenderProvidedChargeId"/>
        /// </summary>
        public const string ChargePriceMaximumDigitsAndDecimalsErrorText =
            "Energy price {{ChargePointPrice}} for charge {{DocumentSenderProvidedChargeId}} contains a non-digit character, has a length that exceeds 15 or does not comply with format '99999999.999999'";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.FeeMustHaveSinglePrice"/>
        /// using <see cref="CimValidationErrorTextToken.ChargePointsCount"/>,
        /// <see cref="CimValidationErrorTextToken.DocumentSenderProvidedChargeId"/>
        /// and <see cref="CimValidationErrorTextToken.ChargeResolution"/>
        /// </summary>
        public const string FeeMustHaveSinglePriceErrorText =
            "The number of prices {{ChargePointsCount}} for charge {{DocumentSenderProvidedChargeId}} doesn't match period type {{ChargeResolution}}";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.SubscriptionMustHaveSinglePrice"/>
        /// using <see cref="CimValidationErrorTextToken.ChargePointsCount"/>,
        /// <see cref="CimValidationErrorTextToken.DocumentSenderProvidedChargeId"/>
        /// and <see cref="CimValidationErrorTextToken.ChargeResolution"/>
        /// </summary>
        public const string SubscriptionMustHaveSinglePriceErrorText =
            "The number of prices {{ChargePointsCount}} for charge {{DocumentSenderProvidedChargeId}} doesn't match period type {{ChargeResolution}}";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.CommandSenderMustBeAnExistingMarketParticipant"/>
        /// <see cref="CimValidationErrorTextToken.DocumentSenderId"/>
        /// and <see cref="CimValidationErrorTextToken.DocumentId"/>
        /// </summary>
        public const string CommandSenderMustBeAnExistingMarketParticipantErrorText =
            "Sender {{DocumentSenderId}} for message {{DocumentId}} is currently not an existing market party (company) or not active";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.UpdateNotYetSupported"/>
        /// using <see cref="CimValidationErrorTextToken.DocumentSenderProvidedChargeId"/>,
        /// <see cref="CimValidationErrorTextToken.ChargeOwner"/>,
        /// and <see cref="CimValidationErrorTextToken.ChargeType"/>
        /// </summary>
        public const string UpdateNotYetSupportedErrorText =
            "Charge ID {{DocumentSenderProvidedChargeId}} for owner {{ChargeOwner}} of type {{ChargeType}} cannot yet be updated or stopped. The functionality is not implemented yet.";
    }
}
