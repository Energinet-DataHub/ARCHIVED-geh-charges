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

using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class TaxIndicatorIsNotAllowedForSubscriptionValidationRuleTests
    {
        [Theory]
        [InlineAutoMoqData(ChargeType.Fee, TaxIndicator.Tax, true)]
        [InlineAutoMoqData(ChargeType.Fee, TaxIndicator.NoTax, true)]
        [InlineAutoMoqData(ChargeType.Subscription, TaxIndicator.Tax, false)]
        [InlineAutoMoqData(ChargeType.Subscription, TaxIndicator.NoTax, true)]
        [InlineAutoMoqData(ChargeType.Tariff, TaxIndicator.Tax, true)]
        [InlineAutoMoqData(ChargeType.Tariff, TaxIndicator.NoTax, true)]
        [InlineAutoMoqData(ChargeType.Unknown, TaxIndicator.NoTax, true)]
        public void IsValid_Test(
            ChargeType chargeType,
            TaxIndicator taxIndicator,
            bool expected,
            ChargeOperationDtoBuilder chargeOperationDtoBuilder)
        {
            var chargeOperationDto = chargeOperationDtoBuilder
                .WithChargeType(chargeType)
                .WithTaxIndicator(taxIndicator).Build();

            var sut = new TaxIndicatorIsNotAllowedForSubscriptionValidationRule(chargeOperationDto);
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeOperationDtoBuilder builder)
        {
            var chargeOperationDto = builder.WithChargeType(ChargeType.Subscription).WithTaxIndicator(TaxIndicator.Tax).Build();
            var sut = new TaxIndicatorIsNotAllowedForSubscriptionValidationRule(chargeOperationDto);
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.TaxIndicatorMustBeFalseForSubscription);
        }
    }
}
