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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;
using ChargeType = GreenEnergyHub.Charges.Domain.Charges.ChargeType;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class FeeMustHaveSinglePriceRuleTests
    {
        [Theory]
        [InlineAutoMoqData(0, false)]
        [InlineAutoMoqData(1, true)]
        [InlineAutoMoqData(2, false)]
        public void IsValid_WhenCalledWith1PricePoint_ShouldParseValidation(
            int priceCount,
            bool expected,
            ChargeCommandTestBuilder builder)
        {
            // Arrange
            var chargeCommand = builder.WithChargeType(ChargeType.Fee).WithPointWithXNumberOfPrices(priceCount).Build();

            // Act
            var sut = new FeeMustHaveSinglePriceRule(chargeCommand);

            // Assert
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData(ChargeType.Tariff)]
        [InlineAutoMoqData(ChargeType.Unknown)]
        public void IsValid_WhenNeitherFeeOrSubscription_ShouldParseValidation(
            ChargeType chargeType,
            ChargeCommandTestBuilder builder)
        {
            var chargeCommand = builder.WithChargeType(chargeType).Build();
            var sut = new FeeMustHaveSinglePriceRule(chargeCommand);
            sut.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeCommand command)
        {
            var sut = new FeeMustHaveSinglePriceRule(command);
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.FeeMustHaveSinglePrice);
        }
    }
}
