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
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim;
using GreenEnergyHub.Charges.MessageHub.Models.Shared;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Models.Shared
{
    public class CimValidationErrorCodeFactoryTests
    {
        [Theory]
        [InlineAutoMoqData(ValidationRuleIdentifier.MaximumPrice, ReasonCode.E90)]
        [InlineAutoMoqData(ValidationRuleIdentifier.ResolutionFeeValidation, ReasonCode.D23)]
        [InlineAutoMoqData(ValidationRuleIdentifier.ResolutionSubscriptionValidation, ReasonCode.D23)]
        [InlineAutoMoqData(ValidationRuleIdentifier.ResolutionTariffValidation, ReasonCode.D23)]
        [InlineAutoMoqData(ValidationRuleIdentifier.StartDateValidation, ReasonCode.E17)]
        [InlineAutoMoqData(ValidationRuleIdentifier.VatClassificationValidation, ReasonCode.E86)]
        [InlineAutoMoqData(ValidationRuleIdentifier.ChargeIdLengthValidation, ReasonCode.E86)]
        [InlineAutoMoqData(ValidationRuleIdentifier.ChargeIdRequiredValidation, ReasonCode.E0H)]
        [InlineAutoMoqData(ValidationRuleIdentifier.ChargeOperationIdRequired, ReasonCode.E0H)]
        [InlineAutoMoqData(ValidationRuleIdentifier.ChargeDescriptionHasMaximumLength, ReasonCode.E86)]
        [InlineAutoMoqData(ValidationRuleIdentifier.ChargeNameHasMaximumLength, ReasonCode.E86)]
        [InlineAutoMoqData(ValidationRuleIdentifier.ChargeOwnerIsRequiredValidation, ReasonCode.E0H)]
        [InlineAutoMoqData(ValidationRuleIdentifier.ChargeTypeIsKnownValidation, ReasonCode.E86)]
        [InlineAutoMoqData(ValidationRuleIdentifier.ChargeTypeTariffPriceCount, ReasonCode.E87)]
        [InlineAutoMoqData(ValidationRuleIdentifier.FeeMustHaveSinglePrice, ReasonCode.E87)]
        [InlineAutoMoqData(ValidationRuleIdentifier.RecipientIsMandatoryTypeValidation, ReasonCode.D02)]
        [InlineAutoMoqData(ValidationRuleIdentifier.SenderIsMandatoryTypeValidation, ReasonCode.D02)]
        [InlineAutoMoqData(ValidationRuleIdentifier.StartDateTimeRequiredValidation, ReasonCode.E0H)]
        [InlineAutoMoqData(ValidationRuleIdentifier.SubscriptionMustHaveSinglePrice, ReasonCode.E87)]
        [InlineAutoMoqData(ValidationRuleIdentifier.ChangingTariffTaxValueNotAllowed, ReasonCode.D14)]
        [InlineAutoMoqData(ValidationRuleIdentifier.ChargePriceMaximumDigitsAndDecimals, ReasonCode.E86)]
        [InlineAutoMoqData(ValidationRuleIdentifier.BusinessReasonCodeMustBeUpdateChargeInformationOrChargePrices, ReasonCode.D02)]
        [InlineAutoMoqData(ValidationRuleIdentifier.CommandSenderMustBeAnExistingMarketParticipant, ReasonCode.D02)]
        [InlineAutoMoqData(ValidationRuleIdentifier.DocumentTypeMustBeRequestChangeOfPriceList, ReasonCode.D02)]
        [InlineAutoMoqData(ValidationRuleIdentifier.MeteringPointDoesNotExist, ReasonCode.E10)]
        [InlineAutoMoqData(ValidationRuleIdentifier.ChargeDoesNotExist, ReasonCode.E0I)]
        [InlineAutoMoqData(ValidationRuleIdentifier.ChargeLinkUpdateNotYetSupported, ReasonCode.D13)]
        [InlineAutoMoqData(ValidationRuleIdentifier.SubsequentBundleOperationsFail, ReasonCode.D14)]
        [InlineAutoMoqData(ValidationRuleIdentifier.TransparentInvoicingIsNotAllowedForFee, ReasonCode.D67)]
        [InlineAutoMoqData(ValidationRuleIdentifier.ChargeResolutionCanNotBeUpdated, ReasonCode.D23)]
        [InlineAutoMoqData(ValidationRuleIdentifier.NumberOfPointsMatchTimeIntervalAndResolution, ReasonCode.E87)]
        [InlineAutoMoqData(ValidationRuleIdentifier.ChargeNameIsRequired, ReasonCode.E0H)]
        [InlineAutoMoqData(ValidationRuleIdentifier.ChargeDescriptionIsRequired, ReasonCode.E0H)]
        [InlineAutoMoqData(ValidationRuleIdentifier.ChargeOwnerHasLengthLimits, ReasonCode.E86)]
        [InlineAutoMoqData(ValidationRuleIdentifier.TransparentInvoicingIsRequired, ReasonCode.E0H)]
        [InlineAutoMoqData(ValidationRuleIdentifier.CreateChargeIsNotAllowedATerminationDate, ReasonCode.D14)]
        public void Create_ReturnsExpectedCode(
            ValidationRuleIdentifier identifier,
            ReasonCode expected,
            CimValidationErrorCodeFactory sut)
        {
            var actual = sut.Create(identifier);
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData]
        public void Create_HandlesAllIdentifiers(CimValidationErrorCodeFactory sut)
        {
            foreach (var value in Enum.GetValues<ValidationRuleIdentifier>())
            {
                // Assert that create does not throw (ensures that we are mapping all enum values)
                sut.Create(value);
            }
        }

        [Theory]
        [InlineAutoMoqData(-1)]
        public void Create_AnUnknownValidationRuleIdentifierIsProvided_throwsNotImplementedException(
            ValidationRuleIdentifier identifier,
            CimValidationErrorCodeFactory sut)
        {
            Assert.Throws<NotImplementedException>(() => sut.Create(identifier));
        }
    }
}
