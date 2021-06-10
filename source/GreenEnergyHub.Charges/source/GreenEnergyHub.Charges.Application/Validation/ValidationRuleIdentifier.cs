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

namespace GreenEnergyHub.Charges.Application.Validation
{
    public enum ValidationRuleIdentifier
    {
        StartDateValidation, // VR209
        ChangingTariffVatValueNotAllowed, // VR630
        ChangingTariffTaxValueNotAllowed, // VRXYZ
        ProcessTypeIsKnownValidation, // VR009
        SenderIsMandatoryTypeValidation, // VR150
        RecipientIsMandatoryTypeValidation, // VR153
        ChargeOperationIdRequired, // VR223
        OperationTypeValidation, // VR445
        ChargeIdLengthValidation, // VR441
        ChargeIdRequiredValidation, // VR440
        DocumentTypeMustBeRequestUpdateChargeInformation, // VR404
        BusinessReasonCodeMustBeUpdateChargeInformation, // VR424
        ChargeTypeIsKnownValidation, // VR449
        VatClassificationValidation, // VR488
        ResolutionTariffValidation, // VR505-1
        ResolutionFeeValidation, // VR505-2
        ResolutionSubscriptionValidation, // VR505-3
        StartDateTimeRequiredValidation, // VR531
        ChargeOwnerIsRequiredValidation, // VR532
        ChargeNameHasMaximumLength, // VR446
        ChargeDescriptionHasMaximumLength, // VR447
        ChargeTypeTariffPriceCount, // VR507-1
        MaximumPrice, // VR509
        ChargePriceMaximumDigitsAndDecimals, // VR457
        FeeMustHaveSinglePrice, // VR507-2
        SubscriptionMustHaveSinglePrice, // VR507-2
        CommandSenderMustBeAnExistingMarketParticipant, // VR152
    }
}
