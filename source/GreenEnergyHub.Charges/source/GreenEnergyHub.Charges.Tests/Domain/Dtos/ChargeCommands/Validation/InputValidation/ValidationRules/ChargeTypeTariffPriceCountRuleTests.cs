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
using System.Linq;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;
using ChargeType = GreenEnergyHub.Charges.Domain.Charges.ChargeType;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class ChargeTypeTariffPriceCountRuleTests
    {
        [Fact]
        public void IsValid_WhenPointsCountIsZero_IsTrue()
        {
            // Arrange
            var chargeCommand = new ChargeCommandBuilder().WithChargeType(ChargeType.Tariff).Build();
            var chargeOperationDto = chargeCommand.Charges.First();

            // Act
            var sut = new ChargeTypeTariffPriceCountRule(chargeOperationDto);

            // Assert
            sut.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineAutoMoqData(1, false)]
        [InlineAutoMoqData(23, false)]
        [InlineAutoMoqData(24, true)]
        [InlineAutoMoqData(25, false)]
        [InlineAutoMoqData(96, false)]
        public void IsValid_WhenPT1HAnd24PricePoints_IsTrue(
            int priceCount,
            bool expected,
            ChargeCommandBuilder builder)
        {
            // Arrange
            var chargeCommand = builder
                .WithChargeType(ChargeType.Tariff)
                .WithPointWithXNumberOfPrices(priceCount)
                .Build();

            var chargeOperationDto = chargeCommand.Charges.First();

            // Act
            var sut = new ChargeTypeTariffPriceCountRule(chargeOperationDto);

            // Assert
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData(1, true)]
        [InlineAutoMoqData(2, false)]
        [InlineAutoMoqData(24, false)]
        [InlineAutoMoqData(96, false)]
        public void IsValid_WhenP1DAnd1PricePoint_IsTrue(
            int priceCount,
            bool expected,
            ChargeCommandBuilder builder)
        {
            // Arrange
            var chargeCommand = builder
                .WithChargeType(ChargeType.Tariff)
                .WithResolution(Resolution.P1D)
                .WithPointWithXNumberOfPrices(priceCount).Build();

            var chargeOperationDto = chargeCommand.Charges.First();

            // Act
            var sut = new ChargeTypeTariffPriceCountRule(chargeOperationDto);

            // Assert
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData(1, true)]
        [InlineAutoMoqData(2, false)]
        public void IsValid_WhenP1MAnd1PricePoint_IsTrue(
            int priceCount,
            bool expected,
            ChargeCommandBuilder builder)
        {
            // Arrange
            var chargeCommand = builder
                .WithChargeType(ChargeType.Tariff)
                .WithResolution(Resolution.P1M)
                .WithPointWithXNumberOfPrices(priceCount).Build();
            var chargeOperationDto = chargeCommand.Charges.First();

            // Act
            var sut = new ChargeTypeTariffPriceCountRule(chargeOperationDto);

            // Assert
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData(1, false)]
        [InlineAutoMoqData(2, false)]
        [InlineAutoMoqData(24, false)]
        [InlineAutoMoqData(95, false)]
        [InlineAutoMoqData(96, true)]
        [InlineAutoMoqData(97, false)]
        public void IsValid_WhenPT15MAnd96PricePoints_IsTrue(
            int priceCount,
            bool expected,
            ChargeCommandBuilder builder)
        {
            // Arrange
            var chargeCommand = builder
                .WithChargeType(ChargeType.Tariff)
                .WithResolution(Resolution.PT15M)
                .WithPointWithXNumberOfPrices(priceCount).Build();

            var chargeOperationDto = chargeCommand.Charges.First();

            // Act
            var sut = new ChargeTypeTariffPriceCountRule(chargeOperationDto);

            // Assert
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData(ChargeType.Fee)]
        [InlineAutoMoqData(ChargeType.Subscription)]
        [InlineAutoMoqData(ChargeType.Unknown)]
        public void IsValid_WhenNotTariff_IsValid(
            ChargeType chargeType,
            ChargeCommandBuilder builder)
        {
            var chargeCommand = builder.WithChargeType(chargeType).Build();
            var chargeOperationDto = chargeCommand.Charges.First();
            var sut = new ChargeTypeTariffPriceCountRule(chargeOperationDto);
            sut.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineAutoMoqData(Resolution.Unknown)]
        public void IsValid_WhenTariffAndUnknownResolutionType_Throws(
            Resolution resolution,
            ChargeCommandBuilder builder)
        {
            // Arrange
            var chargeCommand = builder
                .WithChargeType(ChargeType.Tariff)
                .WithResolution(resolution)
                .WithPointWithXNumberOfPrices(24)
                .Build();
            var chargeOperationDto = chargeCommand.Charges.First();
            var chargeTypeTariffPriceCountRule = new ChargeTypeTariffPriceCountRule(chargeOperationDto);

            // Act
            Action act = () => chargeTypeTariffPriceCountRule.IsValid.Should().BeTrue();

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeCommandBuilder builder)
        {
            var sut = CreateInvalidRule(builder);
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.ChargeTypeTariffPriceCount);
        }

        private static ChargeTypeTariffPriceCountRule CreateInvalidRule(ChargeCommandBuilder builder)
        {
            var invalidCommand = CreateInvalidCommand(builder);
            var chargeOperationDto = invalidCommand.Charges.First();
            return new ChargeTypeTariffPriceCountRule(chargeOperationDto);
        }

        private static ChargeCommand CreateInvalidCommand(ChargeCommandBuilder builder)
        {
            return builder
                .WithChargeType(ChargeType.Tariff)
                .WithResolution(Resolution.P1D)
                .Build();
        }
    }
}
