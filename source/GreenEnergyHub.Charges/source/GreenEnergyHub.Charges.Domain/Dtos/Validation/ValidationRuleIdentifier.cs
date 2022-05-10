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
        StartDateValidation = 1, // VR209-1 / E17
        ChangingTariffTaxValueNotAllowed = 3, // VR903 / D14
        SenderIsMandatoryTypeValidation = 5, // VR150 / D02
        RecipientIsMandatoryTypeValidation = 6, // VR153 / D02
        ChargeOperationIdRequired = 7, // VR223 / E0H
        ChargeIdLengthValidation = 9, // VR441 / E86
        ChargeIdRequiredValidation = 10, // VR440 / E0H
        DocumentTypeMustBeRequestChangeOfPriceList = 11, // VR404 / D02
        BusinessReasonCodeMustBeUpdateChargeInformationOrChargePrices = 12, // VR424 / D02
        ChargeTypeIsKnownValidation = 13, // VR449 / E86
        VatClassificationValidation = 14, // VR488 / E86
        ResolutionTariffValidation = 15, // VR505-1 / D23
        ResolutionFeeValidation = 16, // VR505-2 / D23
        ResolutionSubscriptionValidation = 17, // VR505-3 / D23
        StartDateTimeRequiredValidation = 18, // VR531 / E0H
        ChargeOwnerIsRequiredValidation = 19, // VR532 / E0H
        ChargeNameHasMaximumLength = 20, // VR446 / E86
        ChargeDescriptionHasMaximumLength = 21, // VR447 / E86
        ChargeTypeTariffPriceCount = 22, // VR507-1 / E87
        MaximumPrice = 23, // VR509-1, VR509-2 / E90
        ChargePriceMaximumDigitsAndDecimals = 24, // VR457 / E86
        FeeMustHaveSinglePrice = 25, // VR507-2 / E87
        SubscriptionMustHaveSinglePrice = 26, // VR507-2 / E87
        CommandSenderMustBeAnExistingMarketParticipant = 27, // VR152 / D02
        MeteringPointDoesNotExist = 29, // VR200 / E10
        ChargeDoesNotExist = 30, // VR679 / E0I
        ChargeLinkUpdateNotYetSupported = 31, // VR902 / D13
        UpdateChargeMustHaveEffectiveDateBeforeOrOnStopDate = 32, // VR905 / D14
        SubsequentBundleOperationsFail = 33, // VR906 / D14
        TransparentInvoicingIsNotAllowedForFee = 34, // VR904 / D67
        ChargeResolutionCanNotBeUpdated = 35, // VR907 / D23
    }
}
