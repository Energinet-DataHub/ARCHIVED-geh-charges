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
            "Effective date {{ChargeStartDateTime}} incorrect: The information is not received within the correct time period (either too soon or too late).";

        [ErrorMessageFor(ValidationRuleIdentifier.ChangingTariffTaxValueNotAllowed)]
        public const string ChangingTariffTaxValueNotAllowedErrorText =
            "It is not allowed to change the tax indicator to {{ChargeTaxIndicator}} for charge ID {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} owned by {{ChargeOwner}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.SenderIsMandatoryTypeValidation)]
        public const string SenderIsMandatoryTypeValidationErrorText =
            "Sender is missing for message {{DocumentId}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.ChargeOwnerMustMatchSender)]
        public const string ChargeOwnerMustMatchSenderErrorText =
            "The specified charge owner {{ChargeOwner}} do not match sender {{DocumentSenderId}} and is therefore not authorized to change charge id {{DocumentSenderProvidedChargeId}} of type {{ChargeType}}";

        [ErrorMessageFor(ValidationRuleIdentifier.RecipientIsMandatoryTypeValidation)]
        public const string RecipientIsMandatoryTypeValidationErrorText =
            "Recipient is missing for message {{DocumentId}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.ChargeOperationIdRequired)]
        public const string ChargeOperationIdRequiredErrorText =
            "Transaction ID is missing: transaction can not be processed for document {{DocumentId}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.ChargeIdLengthValidation)]
        public const string ChargeIdLengthValidationErrorText =
            "Charge ID {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} owned by {{ChargeOwner}} has a length that exceeds 10.";

        [ErrorMessageFor(ValidationRuleIdentifier.ChargeIdRequiredValidation)]
        public const string ChargeIdRequiredValidationErrorText =
            "No charge ID provided in transaction with ID {{ChargeOperationId}}. Charge ID is required.";

        [ErrorMessageFor(ValidationRuleIdentifier.DocumentTypeMustBeRequestChangeOfPriceList)]
        public const string DocumentTypeMustBeRequestChangeOfPriceListErrorText =
            "Document type {{DocumentType}} not allowed together with business reason code {{DocumentBusinessReasonCode}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.BusinessReasonCodeMustBeUpdateChargeInformationOrChargePrices)]
        public const string BusinessReasonCodeMustBeUpdateChargeInformationErrorText =
            "Business reason code {{DocumentBusinessReasonCode}} not allowed together with document type {{DocumentType}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.ChargeTypeIsKnownValidation)]
        public const string ChargeTypeIsKnownValidationErrorText =
            "Charge type is missing for charge with ID {{DocumentSenderProvidedChargeId}} owned by {{ChargeOwner}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.VatClassificationValidation)]
        public const string VatClassificationValidationErrorText =
            "VAT class is missing for charge with ID {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} owned by {{ChargeOwner}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.ResolutionTariffValidation)]
        public const string ResolutionTariffValidationErrorText =
            "Period type {{ChargeResolution}} not allowed: The resolution must be day or hour for charge ID {{DocumentSenderProvidedChargeId}} owned by {{ChargeOwner}} as it is of type {{ChargeType}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.ResolutionFeeValidation)]
        public const string ResolutionFeeValidationErrorText =
            "Period type {{ChargeResolution}} not allowed: The resolution must be month for charge ID {{DocumentSenderProvidedChargeId}} owned by {{ChargeOwner}} as it is of type {{ChargeType}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.ResolutionSubscriptionValidation)]
        public const string ResolutionSubscriptionValidationErrorText =
            "Period type {{ChargeResolution}} not allowed: The resolution must be month for charge ID {{DocumentSenderProvidedChargeId}} owned by {{ChargeOwner}} as it is of type {{ChargeType}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.StartDateTimeRequiredValidation)]
        public const string StartDateTimeRequiredValidationErrorText =
            "Effective date is missing for charge with ID {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} owned by {{ChargeOwner}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.ChargeOwnerIsRequiredValidation)]
        public const string ChargeOwnerIsRequiredValidationErrorText =
            "Owner is missing for charge with ID {{DocumentSenderProvidedChargeId}} of type {{ChargeType}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.ChargeNameHasMaximumLength)]
        public const string ChargeNameHasMaximumLengthErrorText =
            "Name {{ChargeName}} has a length that exceeds 132 for charge ID {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} owned by {{ChargeOwner}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.ChargeDescriptionHasMaximumLength)]
        public const string ChargeDescriptionHasMaximumLengthErrorText =
            "Description {{ChargeDescription}} has a length that exceeds 2048 for charge ID {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} owned by {{ChargeOwner}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.ChargeTypeTariffPriceCount)]
        public const string ChargeTypeTariffPriceCountErrorText =
            "The number of prices {{ChargePointsCount}} doesn't match period type {{ChargeResolution}} for charge ID {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} owned by {{ChargeOwner}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.ChargeTypeTariffTaxIndicatorOnlyAllowedBySystemOperator)]
        public const string ChargeTypeTariffTaxIndicatorOnlyAllowedBySystemOperatorErrorText =
            "The sender role used is not allowed to set tax indicator to {{ChargeTaxIndicator}} for charge ID {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} owned by {{ChargeOwner}}";

        [ErrorMessageFor(ValidationRuleIdentifier.UpdateTaxTariffOnlyAllowedBySystemOperator)]
        public const string UpdateTaxTariffOnlyAllowedBySystemOperatorErrorText =
            "The sender role used is not allowed to submit a price series for charge ID {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} owned by {{ChargeOwner}} as it is marked as a tax";

        [ErrorMessageFor(ValidationRuleIdentifier.MaximumPrice)]
        public const string MaximumPriceErrorText =
            "Price {{ChargePointPrice}} not allowed: The specified charge price for position {{ChargePointPosition}} is not plausible (too large) for charge with ID {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} owned by {{ChargeOwner}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.ChargePriceMaximumDigitsAndDecimals)]
        public const string ChargePriceMaximumDigitsAndDecimalsErrorText =
            "Energy price {{ChargePointPrice}} contains a non-digit character, has a length that exceeds 15 or does not comply with format '99999999.999999' for charge with ID {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} owned by {{ChargeOwner}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.CommandSenderMustBeAnExistingMarketParticipant)]
        public const string CommandSenderMustBeAnExistingMarketParticipantErrorText =
            "Sender {{DocumentSenderId}} for message {{DocumentId}} is currently not an existing market party (company) or not active.";

        [ErrorMessageFor(ValidationRuleIdentifier.MeteringPointDoesNotExist)]
        public const string MeteringPointDoesNotExistValidationErrorText =
            "GSRN-code {{MeteringPointId}} is unknown: The specified metering point has not been registered in the system on the charge link start date.";

        [ErrorMessageFor(ValidationRuleIdentifier.ChargeDoesNotExist)]
        public const string ChargeDoesNotExistValidationErrorText =
            "Charge ID {{DocumentSenderProvidedChargeId}} not allowed: The charge is not an existing charge on date {{ChargeLinkStartDate}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.ChargeLinkUpdateNotYetSupported)]
        public const string ChargeLinksUpdateNotYetSupportedErrorText =
            "Charge link for metering point ID {{MeteringPointId}} and charge ID {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} owned by {{ChargeOwner}} cannot yet be updated or stopped. The functionality is not implemented yet.";

        [ErrorMessageFor(ValidationRuleIdentifier.UpdateChargeMustHaveEffectiveDateBeforeOrOnStopDate)]
        public const string UpdateChargeMustHaveEffectiveDateBeforeOrOnStopDateErrorText =
            "Charge ID {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} owned by {{ChargeOwner}} has been stopped and thus cannot be updated as per {{ChargeStartDateTime}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.SubsequentBundleOperationsFail)]
        public const string ValidationOfPriorOperationInBundleFailedErrorText =
            "Transaction for Charge ID {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} owned by {{ChargeOwner}} is not completed: The request received contained multiple transactions for the same charge, and one of the previous transactions with ID {{TriggeredByOperationId}} failed validation why this transaction with ID {{ChargeOperationId}} is also rejected.";

        [ErrorMessageFor(ValidationRuleIdentifier.TransparentInvoicingIsNotAllowedForFee)]
        public const string TransparentInvoicingIsNotAllowedForFeeErrorText =
            "Transparent invoicing cannot be set to true for Charge ID {{DocumentSenderProvidedChargeId}} owned by {{ChargeOwner}} as it is of type {{ChargeType}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.ChargeResolutionCanNotBeUpdated)]
        public const string ChargeResolutionCanNotBeUpdatedErrorText =
            "Changing period type {{ChargeResolution}} not allowed: The resolution may not be changed for charge with ID {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} owned by {{ChargeOwner}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.RecipientRoleMustBeDdz)]
        public const string RecipientRoleMustBeDdzErrorText =
            "Recipient role {{DocumentRecipientBusinessProcessRole}} not allowed: the role used with business reason code {{DocumentBusinessReasonCode}} must be metering point administrator (DDZ).";

        [ErrorMessageFor(ValidationRuleIdentifier.NumberOfPointsMatchTimeIntervalAndResolution)]
        public const string NumberOfPointsMatchTimeIntervalAndResolutionText =
            "The number of prices received does not match the expected number of prices given the time interval and resolution provided for charge with ID {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} owned by {{ChargeOwner}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.ChargeNameIsRequired)]
        public const string ChargeNameRequiredErrorText =
            "Charge name is missing for charge with ID {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} owned by {{ChargeOwner}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.ChargeDescriptionIsRequired)]
        public const string ChargeDescriptionRequiredErrorText =
            "Charge description is missing for charge with ID {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} owned by {{ChargeOwner}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.ResolutionIsRequired)]
        public const string ResolutionRequiredErrorText =
            "Resolution is missing for charge with ID {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} owned by {{ChargeOwner}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.TransparentInvoicingIsRequired)]
        public const string TransparentInvoicingIsRequiredErrorText =
            "Transparent invoicing must be set when calling with BusinessReasonCode D18 for charge type {{ChargeType}} with charge ID {{DocumentSenderProvidedChargeId}} owned by {{ChargeOwner}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.TaxIndicatorIsRequired)]
        public const string TaxIndicatorIsRequiredErrorText =
            "Tax indicator must be set when calling with BusinessReasonCode D18 for charge type {{ChargeType}} with charge ID {{DocumentSenderProvidedChargeId}} owned by {{ChargeOwner}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.TerminationDateMustMatchEffectiveDate)]
        public const string TerminationDateMustMatchEffectiveDateErrorText =
            "Termination date must match effective date when requesting a stop of charge with ID {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} owned by {{ChargeOwner}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.CreateChargeIsNotAllowedATerminationDate)]
        public const string CreateChargeIsNotAllowedATerminationDateErrorText =
            "Charge ID {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} owned by {{ChargeOwner}} cannot be stopped as it has never existed.";

        [ErrorMessageFor(ValidationRuleIdentifier.PriceListMustStartAndStopAtMidnightValidationRule)]
        public const string PriceListMustStartAndStopAtMidnightErrorText =
            "The time interval (start and end) of the price series must equal midnight local time, expressed in UTC+0 for charge with ID {{DocumentSenderProvidedChargeId}} of type {{ChargeType}} owned by {{ChargeOwner}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.TaxIndicatorMustBeFalseForFee)]
        public const string TaxIndicatorMustBeFalseForFeeErrorText =
            "Tax indicator cannot be true for charge ID {{DocumentSenderProvidedChargeId}} owned by {{ChargeOwner}} as it is of charge type {{ChargeType}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.TaxIndicatorMustBeFalseForSubscription)]
        public const string TaxIndicatorMustBeFalseForSubscriptionErrorText =
            "Tax indicator cannot be true for charge ID {{DocumentSenderProvidedChargeId}} owned by {{ChargeOwner}} as it is of charge type {{ChargeType}}.";

        [ErrorMessageFor(ValidationRuleIdentifier.ChargeOperationIdLengthValidation)]
        public const string OperationIdLengthValidationErrorText =
            "The document with ID {{DocumentId}} contains a transaction ID {{ChargeOperationId}} that is longer than the allowed length of 36 characters.";

        public const string Unknown = "unknown";
    }
}
