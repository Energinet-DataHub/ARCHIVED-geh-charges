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
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim;

namespace GreenEnergyHub.Charges.MessageHub.Models.Shared
{
    public class CimValidationErrorCodeFactory : ICimValidationErrorCodeFactory
    {
        public ReasonCode Create(ValidationRuleIdentifier validationRuleIdentifier)
        {
            return validationRuleIdentifier switch
            {
                ValidationRuleIdentifier.StartDateValidation => ReasonCode.E17,
                ValidationRuleIdentifier.ChangingTariffTaxValueNotAllowed => ReasonCode.D14,
                ValidationRuleIdentifier.SenderIsMandatoryTypeValidation => ReasonCode.D02,
                ValidationRuleIdentifier.RecipientIsMandatoryTypeValidation => ReasonCode.D02,
                ValidationRuleIdentifier.ChargeOperationIdRequired => ReasonCode.E0H,
                ValidationRuleIdentifier.ChargeIdLengthValidation => ReasonCode.E86,
                ValidationRuleIdentifier.ChargeIdRequiredValidation => ReasonCode.E0H,
                ValidationRuleIdentifier.DocumentTypeMustBeRequestUpdateChargeInformation => ReasonCode.D02,
                ValidationRuleIdentifier.BusinessReasonCodeMustBeUpdateChargeInformation => ReasonCode.D02,
                ValidationRuleIdentifier.ChargeTypeIsKnownValidation => ReasonCode.E86,
                ValidationRuleIdentifier.VatClassificationValidation => ReasonCode.E86,
                ValidationRuleIdentifier.ResolutionTariffValidation => ReasonCode.D23,
                ValidationRuleIdentifier.ResolutionFeeValidation => ReasonCode.D23,
                ValidationRuleIdentifier.ResolutionSubscriptionValidation => ReasonCode.D23,
                ValidationRuleIdentifier.StartDateTimeRequiredValidation => ReasonCode.E0H,
                ValidationRuleIdentifier.ChargeOwnerIsRequiredValidation => ReasonCode.E0H,
                ValidationRuleIdentifier.ChargeNameHasMaximumLength => ReasonCode.E86,
                ValidationRuleIdentifier.ChargeDescriptionHasMaximumLength => ReasonCode.E86,
                ValidationRuleIdentifier.ChargeTypeTariffPriceCount => ReasonCode.E87,
                ValidationRuleIdentifier.MaximumPrice => ReasonCode.E90,
                ValidationRuleIdentifier.ChargePriceMaximumDigitsAndDecimals => ReasonCode.E86,
                ValidationRuleIdentifier.FeeMustHaveSinglePrice => ReasonCode.E87,
                ValidationRuleIdentifier.SubscriptionMustHaveSinglePrice => ReasonCode.E87,
                ValidationRuleIdentifier.CommandSenderMustBeAnExistingMarketParticipant => ReasonCode.D02,
                ValidationRuleIdentifier.MeteringPointDoesNotExist => ReasonCode.E10,
                ValidationRuleIdentifier.ChargeDoesNotExist => ReasonCode.E0I,
                ValidationRuleIdentifier.ChargeLinkUpdateNotYetSupported => ReasonCode.D13,
                ValidationRuleIdentifier.UpdateChargeMustHaveEffectiveDateBeforeOrOnStopDate => ReasonCode.D14,
                ValidationRuleIdentifier.SubsequentBundleOperationsFail => ReasonCode.D14,
                ValidationRuleIdentifier.TransparentInvoicingIsNotAllowedForFee => ReasonCode.D67,
                ValidationRuleIdentifier.ChargeResolutionCanNotBeUpdated => ReasonCode.D23,
                _ => throw new NotImplementedException(),
            };
        }
    }
}
