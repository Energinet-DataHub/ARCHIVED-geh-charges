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
            var command = builder.WithTaxIndicator(!charge.TaxIndicator).Build();
            var sut = new ChangingTariffTaxValueNotAllowedRule(command, charge);
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
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeCommand command, Charge charge)
        {
            var sut = new ChangingTariffTaxValueNotAllowedRule(command, charge);
            sut.ValidationError.ValidationRuleIdentifier.Should()
                .Be(ValidationRuleIdentifier.ChangingTariffTaxValueNotAllowed);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldContain_RequiredErrorMessageParameterTypes(
            ChargeCommand command, Charge charge)
        {
            // Arrange
            // Act
            var sut = new ChangingTariffTaxValueNotAllowedRule(command, charge);

            // Assert
            sut.ValidationError.ValidationErrorMessageParameters
                .Select(x => x.ParameterType)
                .Should().Contain(ValidationErrorMessageParameterType.ChargeTaxIndicator);
            sut.ValidationError.ValidationErrorMessageParameters
                .Select(x => x.ParameterType)
                .Should().Contain(ValidationErrorMessageParameterType.DocumentSenderProvidedChargeId);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_RequiredErrorMessageParameters(
            ChargeCommand command, Charge charge)
        {
            // Arrange
            // Act
            var sut = new ChangingTariffTaxValueNotAllowedRule(command, charge);

            // Assert
            sut.ValidationError.ValidationErrorMessageParameters
                .Single(x => x.ParameterType == ValidationErrorMessageParameterType.ChargeTaxIndicator)
                .MessageParameter.Should().Be(command.ChargeOperation.TaxIndicator.ToString());
            sut.ValidationError.ValidationErrorMessageParameters
                .Single(x => x.ParameterType == ValidationErrorMessageParameterType.DocumentSenderProvidedChargeId)
                .MessageParameter.Should().Be(command.ChargeOperation.ChargeId);
        }
    }
}
