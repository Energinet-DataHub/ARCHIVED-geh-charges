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
    public class CimValidationErrorDescriptionProvider : ICimValidationErrorDescriptionProvider
    {
        public string GetCimValidationErrorDescription(ValidationRuleIdentifier validationRuleIdentifier)
        {
            return validationRuleIdentifier switch
            {
                // Please keep sorted by ValidationRuleIdentifier
                ValidationRuleIdentifier.BusinessReasonCodeMustBeUpdateChargeInformation =>
                    CimValidationErrorDescriptionTemplateMessages.BusinessReasonCodeMustBeUpdateChargeInformationErrorDescription,
                ValidationRuleIdentifier.ChangingTariffTaxValueNotAllowed =>
                    CimValidationErrorDescriptionTemplateMessages.ChangingTariffTaxValueNotAllowedErrorDescription,
                ValidationRuleIdentifier.ChangingTariffVatValueNotAllowed =>
                    CimValidationErrorDescriptionTemplateMessages.ChangingTariffVatValueNotAllowedErrorDescription,
                ValidationRuleIdentifier.ChargeDescriptionHasMaximumLength =>
                    CimValidationErrorDescriptionTemplateMessages.ChargeDescriptionHasMaximumLengthErrorDescription,
                ValidationRuleIdentifier.ChargeNameHasMaximumLength =>
                    CimValidationErrorDescriptionTemplateMessages.ChargeNameHasMaximumLengthErrorDescription,
                ValidationRuleIdentifier.ChargeTypeTariffPriceCount =>
                    CimValidationErrorDescriptionTemplateMessages.ChargeTypeTariffPriceCountErrorDescription,
                ValidationRuleIdentifier.ChargeIdLengthValidation =>
                    CimValidationErrorDescriptionTemplateMessages.ChargeIdLengthValidationErrorDescription,
                ValidationRuleIdentifier.ChargeIdRequiredValidation =>
                    CimValidationErrorDescriptionTemplateMessages.ChargeIdRequiredValidationErrorDescription,
                ValidationRuleIdentifier.ChargeOperationIdRequired =>
                    CimValidationErrorDescriptionTemplateMessages.ChargeOperationIdRequiredErrorDescription,
                ValidationRuleIdentifier.ChargeOwnerIsRequiredValidation =>
                    CimValidationErrorDescriptionTemplateMessages.ChargeOwnerIsRequiredValidationErrorDescription,
                ValidationRuleIdentifier.ChargePriceMaximumDigitsAndDecimals =>
                    CimValidationErrorDescriptionTemplateMessages.ChargePriceMaximumDigitsAndDecimalsErrorDescription,
                ValidationRuleIdentifier.ChargeTypeIsKnownValidation =>
                    CimValidationErrorDescriptionTemplateMessages.ChargeTypeIsKnownValidationErrorDescription,
                ValidationRuleIdentifier.CommandSenderMustBeAnExistingMarketParticipant =>
                    CimValidationErrorDescriptionTemplateMessages.CommandSenderMustBeAnExistingMarketParticipantErrorDescription,
                ValidationRuleIdentifier.DocumentTypeMustBeRequestUpdateChargeInformation =>
                    CimValidationErrorDescriptionTemplateMessages.DocumentTypeMustBeRequestUpdateChargeInformationErrorDescription,
                ValidationRuleIdentifier.FeeMustHaveSinglePrice =>
                    CimValidationErrorDescriptionTemplateMessages.FeeMustHaveSinglePriceErrorDescription,
                ValidationRuleIdentifier.MaximumPrice =>
                    CimValidationErrorDescriptionTemplateMessages.MaximumPriceErrorDescription,
                ValidationRuleIdentifier.RecipientIsMandatoryTypeValidation =>
                    CimValidationErrorDescriptionTemplateMessages.RecipientIsMandatoryTypeValidationErrorDescription,
                ValidationRuleIdentifier.ResolutionFeeValidation =>
                    CimValidationErrorDescriptionTemplateMessages.ResolutionFeeValidationErrorDescription,
                ValidationRuleIdentifier.ResolutionSubscriptionValidation =>
                    CimValidationErrorDescriptionTemplateMessages.ResolutionSubscriptionValidationErrorDescription,
                ValidationRuleIdentifier.ResolutionTariffValidation =>
                    CimValidationErrorDescriptionTemplateMessages.ResolutionTariffValidationErrorDescription,
                ValidationRuleIdentifier.SenderIsMandatoryTypeValidation =>
                    CimValidationErrorDescriptionTemplateMessages.SenderIsMandatoryTypeValidationErrorDescription,
                ValidationRuleIdentifier.StartDateTimeRequiredValidation =>
                    CimValidationErrorDescriptionTemplateMessages.StartDateTimeRequiredValidationErrorDescription,
                ValidationRuleIdentifier.StartDateValidation =>
                    CimValidationErrorDescriptionTemplateMessages.StartDateValidationErrorDescription,
                ValidationRuleIdentifier.SubscriptionMustHaveSinglePrice =>
                    CimValidationErrorDescriptionTemplateMessages.SubscriptionMustHaveSinglePriceErrorDescription,
                ValidationRuleIdentifier.UpdateNotYetSupported =>
                    CimValidationErrorDescriptionTemplateMessages.UpdateNotYetSupportedErrorDescription,
                ValidationRuleIdentifier.VatClassificationValidation =>
                    CimValidationErrorDescriptionTemplateMessages.VatClassificationValidationErrorDescription,
                _ => throw new NotImplementedException(),
            };
        }
    }
}
