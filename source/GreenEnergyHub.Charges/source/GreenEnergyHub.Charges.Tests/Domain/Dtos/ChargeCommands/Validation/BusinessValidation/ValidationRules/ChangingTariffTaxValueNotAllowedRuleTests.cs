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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Tests.Builders;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules
{
    [UnitTest]
    public class ChangingTariffTaxValueNotAllowedRuleTests
    {
        [Theory]
        [InlineAutoDomainData]
        public void IsValid_WhenTaxIndicatorInCommandDoesNotMatchCharge_IsFalse(
            ChargeCommandBuilder builder,
            Charge charge)
        {
            var invalidCommand = CreateInvalidCommand(builder, charge);
            var sut = new ChangingTariffTaxValueNotAllowedRule(invalidCommand, charge);
            Assert.False(sut.IsValid);
        }

        [Theory]
        [InlineAutoDomainData]
        public void IsValid_WhenTaxIndicatorInCommandMatches_IsTrue(
            ChargeCommandBuilder builder,
            Charge charge)
        {
            var command = builder.WithTaxIndicator(charge.TaxIndicator).Build();
            var sut = new ChangingTariffTaxValueNotAllowedRule(command, charge);
            Assert.True(sut.IsValid);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationError_WhenIsValid_IsNull(
            ChargeCommandBuilder builder,
            Charge charge)
        {
            var validCommand = builder.WithTaxIndicator(charge.TaxIndicator).Build();
            var sut = new ChangingTariffTaxValueNotAllowedRule(validCommand, charge);
            Assert.Null(sut.ValidationError);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeCommandBuilder builder, Charge charge)
        {
            var invalidCommand = CreateInvalidCommand(builder, charge);
            var sut = new ChangingTariffTaxValueNotAllowedRule(invalidCommand, charge);
            sut.ValidationError!.ValidationRuleIdentifier.Should()
                .Be(ValidationRuleIdentifier.ChangingTariffTaxValueNotAllowed);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationErrorMessageParameters_ShouldContain_RequiredErrorMessageParameterTypes(
            ChargeCommandBuilder builder,
            Charge charge)
        {
            // Arrange
            var invalidCommand = CreateInvalidCommand(builder, charge);

            // Act
            var sut = new ChangingTariffTaxValueNotAllowedRule(invalidCommand, charge);

            // Assert
            sut.ValidationError!.ValidationErrorMessageParameters
                .Select(x => x.ParameterType)
                .Should().Contain(ValidationErrorMessageParameterType.ChargeTaxIndicator);
            sut.ValidationError.ValidationErrorMessageParameters
                .Select(x => x.ParameterType)
                .Should().Contain(ValidationErrorMessageParameterType.DocumentSenderProvidedChargeId);
        }

        [Theory]
        [InlineAutoDomainData]
        public void MessageParameter_ShouldBe_RequiredErrorMessageParameters(
            ChargeCommandBuilder builder, Charge charge)
        {
            // Arrange
            var invalidCommand = CreateInvalidCommand(builder, charge);

            // Act
            var sut = new ChangingTariffTaxValueNotAllowedRule(invalidCommand, charge);

            // Assert
            sut.ValidationError!.ValidationErrorMessageParameters
                .Single(x => x.ParameterType == ValidationErrorMessageParameterType.ChargeTaxIndicator)
                .ParameterValue.Should().Be(invalidCommand.ChargeOperation.TaxIndicator.ToString());
            sut.ValidationError.ValidationErrorMessageParameters
                .Single(x => x.ParameterType == ValidationErrorMessageParameterType.DocumentSenderProvidedChargeId)
                .ParameterValue.Should().Be(invalidCommand.ChargeOperation.ChargeId);
        }

        private static ChargeCommand CreateInvalidCommand(ChargeCommandBuilder builder, Charge charge)
        {
            return builder.WithTaxIndicator(!charge.TaxIndicator).Build();
        }
    }
}
