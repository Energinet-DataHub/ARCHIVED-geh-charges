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

namespace GreenEnergyHub.Charges.Domain.Dtos.Validation
{
    public enum ValidationRuleIdentifier
    {
        Unknown = 0,
        StartDateValidation = 1, // VR209
        ChangingTariffVatValueNotAllowed = 2, // VR630
        ChangingTariffTaxValueNotAllowed = 3, // VR903
        ProcessTypeIsKnownValidation = 4, // VR009
        SenderIsMandatoryTypeValidation = 5, // VR150
        RecipientIsMandatoryTypeValidation = 6, // VR153
        ChargeOperationIdRequired = 7, // VR223
        OperationTypeValidation = 8, // VR445
        ChargeIdLengthValidation = 9, // VR441
        ChargeIdRequiredValidation = 10, // VR440
        DocumentTypeMustBeRequestUpdateChargeInformation = 11, // VR404
        BusinessReasonCodeMustBeUpdateChargeInformation = 12, // VR424
        ChargeTypeIsKnownValidation = 13, // VR449
        VatClassificationValidation = 14, // VR488
        ResolutionTariffValidation = 15, // VR505-1
        ResolutionFeeValidation = 16, // VR505-2
        ResolutionSubscriptionValidation = 17, // VR505-3
        StartDateTimeRequiredValidation = 18, // VR531
        ChargeOwnerIsRequiredValidation = 19, // VR532
        ChargeNameHasMaximumLength = 20, // VR446
        ChargeDescriptionHasMaximumLength = 21, // VR447
        ChargeTypeTariffPriceCount = 22, // VR507-1
        MaximumPrice = 23, // VR509
        ChargePriceMaximumDigitsAndDecimals = 24, // VR457
        FeeMustHaveSinglePrice = 25, // VR507-2
        SubscriptionMustHaveSinglePrice = 26, // VR507-2
        CommandSenderMustBeAnExistingMarketParticipant = 27, // VR152
        UpdateNotYetSupported = 28, // VR902
    }
}
