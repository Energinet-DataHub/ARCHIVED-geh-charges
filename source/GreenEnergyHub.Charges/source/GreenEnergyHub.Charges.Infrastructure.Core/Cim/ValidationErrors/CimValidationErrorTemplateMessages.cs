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
    public static class CimValidationErrorTemplateMessages
    {
        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.StartDateValidation"/>
        /// </summary>
        public const string StartDateValidationErrorMessage =
            "Effectuation date {{ChargeStartDateTime}} incorrect: The information is not received within the correct time period (either too soon or too late)";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.ChangingTariffVatValueNotAllowed"/>
        /// </summary>
        public const string ChangingTariffVatValueNotAllowedErrorMessage =
            "VAT class {{ChargeVatClass}} not allowed: charge {{DocumentSenderProvidedChargeId}} cannot be updated with another entitlement for VAT";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.ChangingTariffTaxValueNotAllowed"/>
        /// </summary>
        public const string ChangingTariffTaxValueNotAllowedErrorMessage =
            "Tax indicator {{ChargeTaxIndicator}} not allowed: charge {{DocumentSenderProvidedChargeId}} cannot be updated with another Tax indicator";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.SenderIsMandatoryTypeValidation"/>
        /// </summary>
        public const string SenderIsMandatoryTypeValidationErrorMessage =
            "Sender is missing for message {{DocumentId}}";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.RecipientIsMandatoryTypeValidation"/>
        /// </summary>
        public const string RecipientIsMandatoryTypeValidationErrorMessage =
            "Recipient is missing for message {{DocumentId}}";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.ChargeOperationIdRequired"/>
        /// </summary>
        public const string ChargeOperationIdRequiredErrorMessage =
            "Identification is missing: transaction can not be processed for document {{DocumentId}}";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.ChargeIdLengthValidation"/>
        /// </summary>
        public const string ChargeIdLengthValidationErrorMessage =
            "Charge ID {{DocumentSenderProvidedChargeId}} has a length that exceeds 10";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.ChargeIdRequiredValidation"/>
        /// </summary>
        public const string ChargeIdRequiredValidationErrorMessage =
            "Identification is missing, charge can not be processed";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.DocumentTypeMustBeRequestUpdateChargeInformation"/>
        /// </summary>
        public const string DocumentTypeMustBeRequestUpdateChargeInformationErrorMessage =
            "Document type {{DocumentType}} not allowed together with energy business process {{DocumentBusinessReasonCode}}";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.BusinessReasonCodeMustBeUpdateChargeInformation"/>
        /// </summary>
        public const string BusinessReasonCodeMustBeUpdateChargeInformationErrorMessage =
            "Energy business process {{DocumentBusinessReasonCode}} not allowed together with document type {{DocumentType}}";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.ChargeTypeIsKnownValidation"/>
        /// </summary>
        public const string ChargeTypeIsKnownValidationErrorMessage =
            "Charge type {{ChargeType}} for charge {{DocumentSenderProvidedChargeId}} has wrong value (outside domain)";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.VatClassificationValidation"/>
        /// </summary>
        public const string VatClassificationValidationErrorMessage =
            "VAT class {{ChargeVatClass}} for charge {{DocumentSenderProvidedChargeId}} has wrong value (outside domain)";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.ResolutionTariffValidation"/>
        /// </summary>
        public const string ResolutionTariffValidationErrorMessage =
            "Period type {{ChargeResolution}} not allowed: The specified resolution for charge {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} must be Day or Hour";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.ResolutionFeeValidation"/>
        /// </summary>
        public const string ResolutionFeeValidationErrorMessage =
            "Period type {{ChargeResolution}} not allowed: The specified resolution for charge {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} must be Day";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.ResolutionSubscriptionValidation"/>
        /// </summary>
        public const string ResolutionSubscriptionValidationErrorMessage =
            "Period type {{ChargeResolution}} not allowed: The specified resolution for charge {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} must be Month";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.StartDateTimeRequiredValidation"/>
        /// </summary>
        public const string StartDateTimeRequiredValidationErrorMessage =
            "Occurrence date is missing for charge {{DocumentSenderProvidedChargeId}}";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.ChargeOwnerIsRequiredValidation"/>
        /// </summary>
        public const string ChargeOwnerIsRequiredValidationErrorMessage =
            "Owner is missing for charge {{DocumentSenderProvidedChargeId}}";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.ChargeNameHasMaximumLength"/>
        /// </summary>
        public const string ChargeNameHasMaximumLengthErrorMessage =
            "Name {{ChargeName}} for charge {{DocumentSenderProvidedChargeId}} has a length that exceeds 50";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.ChargeDescriptionHasMaximumLength"/>
        /// </summary>
        public const string ChargeDescriptionHasMaximumLengthErrorMessage =
            "Description {{ChargeDescription}} for charge {{DocumentSenderProvidedChargeId}} has a length that exceeds 2048";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.ChargeTypeTariffPriceCount"/>
        /// </summary>
        public const string ChargeTypeTariffPriceCountErrorMessage =
            "The number of prices {{ChargePointsCount}} for charge {{DocumentSenderProvidedChargeId}} doesn't match period type {{ChargeResolution}}";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.MaximumPrice"/>
        /// </summary>
        public const string MaximumPriceErrorMessage =
            "Price {{ChargePointPrice}} not allowed: The specified charge price for position {{ChargePointPosition}} is not plausible (too large)";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.ChargePriceMaximumDigitsAndDecimals"/>
        /// </summary>
        public const string ChargePriceMaximumDigitsAndDecimalsErrorMessage =
            "Energy price {{ChargePointPrice}} for charge {{DocumentSenderProvidedChargeId}} contains a non-digit character, has a length that exceeds 15 or does not comply with format '99999999.999999'";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.FeeMustHaveSinglePrice"/>
        /// </summary>
        public const string FeeMustHaveSinglePriceErrorMessage =
            "The number of prices {{ChargePointsCount}} for charge {{DocumentSenderProvidedChargeId}} doesn't match period type {{ChargeResolution}}";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.SubscriptionMustHaveSinglePrice"/>
        /// </summary>
        public const string SubscriptionMustHaveSinglePriceErrorMessage =
            "The number of prices {{ChargePointsCount}} for charge {{DocumentSenderProvidedChargeId}} doesn't match period type {{ChargeResolution}}";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.CommandSenderMustBeAnExistingMarketParticipant"/>
        /// </summary>
        public const string CommandSenderMustBeAnExistingMarketParticipantErrorMessage =
            "Sender {{DocumentSenderId}} for message {{DocumentId}} is currently not an existing market party (company) or not active";

        /// <summary>
        /// Errormessage for <see cref="ValidationRuleIdentifier.UpdateNotYetSupported"/>
        /// </summary>
        public const string UpdateNotYetSupportedErrorMessage =
            "Charge ID {{DocumentSenderProvidedChargeId}} for owner {{ChargeOwner}} of type {{ChargeType}} cannot yet be updated or stopped. The functionality is not implemented yet.";
    }
}
