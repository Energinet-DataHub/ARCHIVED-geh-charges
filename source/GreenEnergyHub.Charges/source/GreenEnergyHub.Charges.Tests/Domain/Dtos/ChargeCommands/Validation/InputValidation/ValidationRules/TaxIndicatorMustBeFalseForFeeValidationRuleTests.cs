﻿// Copyright 2020 Energinet DataHub A/S
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

using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class TaxIndicatorMustBeFalseForFeeValidationRuleTests
    {
        [Theory]
        [InlineAutoMoqData(ChargeType.Fee, TaxIndicator.Tax, false)]
        [InlineAutoMoqData(ChargeType.Fee, TaxIndicator.NoTax, true)]
        [InlineAutoMoqData(ChargeType.Fee, TaxIndicator.Unknown, true)]
        [InlineAutoMoqData(ChargeType.Subscription, TaxIndicator.Tax, true)]
        [InlineAutoMoqData(ChargeType.Subscription, TaxIndicator.NoTax, true)]
        [InlineAutoMoqData(ChargeType.Tariff, TaxIndicator.Tax, true)]
        [InlineAutoMoqData(ChargeType.Tariff, TaxIndicator.NoTax, true)]
        [InlineAutoMoqData(ChargeType.Unknown, TaxIndicator.NoTax, true)]
        public void IsValid_Test(
            ChargeType chargeType,
            TaxIndicator taxIndicator,
            bool expected,
            ChargeInformationOperationDtoBuilder chargeInformationOperationDtoBuilder)
        {
            var chargeOperationDto = chargeInformationOperationDtoBuilder
                .WithChargeType(chargeType)
                .WithTaxIndicator(taxIndicator).Build();

            var sut = new TaxIndicatorMustBeFalseForFeeValidationRule(chargeOperationDto);
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeInformationOperationDtoBuilder builder)
        {
            var chargeOperationDto = builder.WithChargeType(ChargeType.Fee).WithTaxIndicator(TaxIndicator.Tax).Build();
            var sut = new TaxIndicatorMustBeFalseForFeeValidationRule(chargeOperationDto);
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.TaxIndicatorMustBeFalseForFee);
        }
    }
}
