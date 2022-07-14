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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Moq;
using NodaTime;
using NodaTime.Text;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules
{
    public class CreateChargeIsNotAllowedATerminationDateRuleTests
    {
        [Theory]
        [InlineAutoMoqData("2020-05-13T13:00:00Z", false)]
        [InlineAutoMoqData("9999-12-31T23:59:59Z", true)]
        public void CreateChargeIsNotAllowedATerminationDateTests_WhenNewChargeAndTerminationDateIsPresentAndNotDefaultEndDate(
            string stopDate, bool expected)
        {
            // Arrange
            var sut = new CreateChargeIsNotAllowedATerminationRuleDate(InstantPattern.General.Parse(stopDate).Value);

            // Act
            var isValid = sut.IsValid;

            // Assert
            isValid.Should().Be(expected);
        }

        [Fact]
        public void CreateChargeIsNotAllowedATerminationDateTests_WhenNewChargeAndTerminationDateIsNull_IsTrue()
        {
            // Arrange
            var sut = new CreateChargeIsNotAllowedATerminationRuleDate(null);

            // Act
            var isValid = sut.IsValid;

            // Assert
            isValid.Should().Be(true);
        }

        [Fact]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo()
        {
            var sut = new CreateChargeIsNotAllowedATerminationRuleDate(It.IsAny<Instant>());
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.CreateChargeIsNotAllowedATerminationDate);
        }
    }
}
