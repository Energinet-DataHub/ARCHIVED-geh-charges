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

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Validation.BusinessValidation.ValidationRules
{
    [UnitTest]
    public class ChargeUpdateNotYetSupportedRuleTests
    {
        [Fact]
        public void IsValid_WhenChargeIsNull_IsTrue()
        {
            var sut = new ChargeUpdateNotYetSupportedRule(null);
            sut.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineAutoDomainData]
        public void IsValid_WhenChargeIsNotNull_IsFalse(
            [NotNull] Charge charge)
        {
            var sut = new ChargeUpdateNotYetSupportedRule(charge);
            sut.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldContain_RequiredErrorMessageParameterTypes(Charge charge)
        {
            // Arrange
            // Act
            var sut = new ChargeUpdateNotYetSupportedRule(charge);

            // Assert
            sut.ValidationError.ValidationErrorMessageParameters
                .Select(x => x.ParameterType)
                .Should().Contain(ValidationErrorMessageParameterType.PartyChargeTypeId);
            sut.ValidationError.ValidationErrorMessageParameters
                .Select(x => x.ParameterType)
                .Should().Contain(ValidationErrorMessageParameterType.ChargeTypeOwner);
            sut.ValidationError.ValidationErrorMessageParameters
                .Select(x => x.ParameterType)
                .Should().Contain(ValidationErrorMessageParameterType.ChargeType);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_RequiredErrorMessageParameters(Charge charge)
        {
            // Arrange
            // Act
            var sut = new ChargeUpdateNotYetSupportedRule(charge);

            // Assert
            sut.ValidationError.ValidationErrorMessageParameters
                .Single(x => x.ParameterType == ValidationErrorMessageParameterType.PartyChargeTypeId)
                .MessageParameter.Should().Be(charge.SenderProvidedChargeId); // todo correct?
            sut.ValidationError.ValidationErrorMessageParameters
                .Single(x => x.ParameterType == ValidationErrorMessageParameterType.ChargeTypeOwner)
                .MessageParameter.Should().Be(charge.OwnerId.ToString()); // todo correct?
            sut.ValidationError.ValidationErrorMessageParameters
                .Single(x => x.ParameterType == ValidationErrorMessageParameterType.ChargeType)
                .MessageParameter.Should().Be(charge.Type.ToString()); // correct?
        }
    }
}
