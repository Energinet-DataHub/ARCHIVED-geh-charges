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

using System.Linq;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class SubscriptionMustHaveSinglePriceRuleTests
    {
        [Theory]
        [InlineAutoMoqData(0, false)]
        [InlineAutoMoqData(1, true)]
        [InlineAutoMoqData(2, false)]
        public void IsValid_WhenCalledWith1PricePoint_ShouldParseValidation(
            int priceCount,
            bool expected,
            ChargeCommandBuilder builder)
        {
            // Arrange
            var chargeCommand = builder
                .WithChargeType(ChargeType.Subscription)
                .WithPointWithXNumberOfPrices(priceCount)
                .Build();

            // Act
            var sut = new SubscriptionMustHaveSinglePriceRule(chargeCommand);

            // Assert
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData(ChargeType.Tariff)]
        [InlineAutoMoqData(ChargeType.Unknown)]
        public void IsValid_NeitherFeeOrSubscription_ShouldParseValidation(
            ChargeType chargeType,
            ChargeCommandBuilder builder)
        {
            var chargeCommand = builder.WithChargeType(chargeType).Build();
            var sut = new SubscriptionMustHaveSinglePriceRule(chargeCommand);
            sut.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeCommand command)
        {
            var sut = new SubscriptionMustHaveSinglePriceRule(command);
            sut.ValidationError.ValidationRuleIdentifier.Should()
                .Be(ValidationRuleIdentifier.SubscriptionMustHaveSinglePrice);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationErrorMessageParameters_ShouldContain_RequiredErrorMessageParameterTypes(ChargeCommand command)
        {
            // Arrange
            // Act
            var sut = new SubscriptionMustHaveSinglePriceRule(command);

            // Assert
            sut.ValidationError.ValidationErrorMessageParameters
                .Select(x => x.ParameterType)
                .Should().Contain(ValidationErrorMessageParameterType.ChargePointsCount);
            sut.ValidationError.ValidationErrorMessageParameters
                .Select(x => x.ParameterType)
                .Should().Contain(ValidationErrorMessageParameterType.DocumentSenderProvidedChargeId);
            sut.ValidationError.ValidationErrorMessageParameters
                .Select(x => x.ParameterType)
                .Should().Contain(ValidationErrorMessageParameterType.ChargeResolution);
        }

        [Theory]
        [InlineAutoDomainData]
        public void MessageParameter_ShouldBe_RequiredErrorMessageParameters(ChargeCommand command)
        {
            // Arrange
            // Act
            var sut = new SubscriptionMustHaveSinglePriceRule(command);

            // Assert
            sut.ValidationError.ValidationErrorMessageParameters
                .Single(x => x.ParameterType == ValidationErrorMessageParameterType.ChargePointsCount).
                ParameterValue.Should().Be(command.ChargeOperation.Points.Count.ToString());
            sut.ValidationError.ValidationErrorMessageParameters
                .Single(x => x.ParameterType == ValidationErrorMessageParameterType.DocumentSenderProvidedChargeId).
                ParameterValue.Should().Be(command.ChargeOperation.ChargeId);
            sut.ValidationError.ValidationErrorMessageParameters
                .Single(x => x.ParameterType == ValidationErrorMessageParameterType.ChargeResolution).
                ParameterValue.Should().Be(command.ChargeOperation.Resolution.ToString());
        }
    }
}
