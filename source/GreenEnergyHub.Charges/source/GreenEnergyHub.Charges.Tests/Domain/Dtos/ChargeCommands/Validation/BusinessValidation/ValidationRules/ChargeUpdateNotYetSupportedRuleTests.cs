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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules
{
    [UnitTest]
    public class ChargeUpdateNotYetSupportedRuleTests
    {
        [Theory]
        [InlineAutoDomainData]
        public void IsValid_WhenChargeIsNull_IsTrue(ChargeCommand chargeCommand)
        {
            var sut = new ChargeUpdateNotYetSupportedRule(chargeCommand, null);
            sut.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineAutoDomainData]
        public void IsValid_WhenChargeIsNotNull_IsFalse(ChargeCommand chargeCommand, Charge charge)
        {
            var sut = new ChargeUpdateNotYetSupportedRule(chargeCommand, charge);
            sut.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldContain_RequiredErrorMessageParameterTypes(
            ChargeCommand chargeCommand, Charge charge)
        {
            // Arrange
            // Act
            var sut = new ChargeUpdateNotYetSupportedRule(chargeCommand, charge);

            // Assert
            sut.ValidationError.ValidationErrorMessageParameters
                .Select(x => x.ParameterType)
                .Should().Contain(ValidationErrorMessageParameterType.DocumentSenderProvidedChargeId);
            sut.ValidationError.ValidationErrorMessageParameters
                .Select(x => x.ParameterType)
                .Should().Contain(ValidationErrorMessageParameterType.ChargeOwner);
            sut.ValidationError.ValidationErrorMessageParameters
                .Select(x => x.ParameterType)
                .Should().Contain(ValidationErrorMessageParameterType.ChargeType);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_RequiredErrorMessageParameters(ChargeCommand chargeCommand, Charge charge)
        {
            // Arrange
            // Act
            var sut = new ChargeUpdateNotYetSupportedRule(chargeCommand, charge);

            // Assert
            sut.ValidationError.ValidationErrorMessageParameters
                .Single(x => x.ParameterType == ValidationErrorMessageParameterType.DocumentSenderProvidedChargeId)
                .MessageParameter.Should().Be(chargeCommand.ChargeOperation.ChargeId);
            sut.ValidationError.ValidationErrorMessageParameters
                .Single(x => x.ParameterType == ValidationErrorMessageParameterType.ChargeOwner)
                .MessageParameter.Should().Be(chargeCommand.ChargeOperation.ChargeOwner);
            sut.ValidationError.ValidationErrorMessageParameters
                .Single(x => x.ParameterType == ValidationErrorMessageParameterType.ChargeType)
                .MessageParameter.Should().Be(chargeCommand.ChargeOperation.Type.ToString());
        }
    }
}
