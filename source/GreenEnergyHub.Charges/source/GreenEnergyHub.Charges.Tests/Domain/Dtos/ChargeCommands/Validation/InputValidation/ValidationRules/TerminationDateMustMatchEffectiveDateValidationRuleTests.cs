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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class TerminationDateMustMatchEffectiveDateValidationRuleTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void IsValid_WhenEndDateIsEmpty_ShouldParseValidation(ChargeInformationOperationDtoBuilder chargeInformationOperationDtoBuilder)
        {
            // Arrange
            var dto = chargeInformationOperationDtoBuilder.WithEndDateTime(null).Build();
            var sut = new TerminationDateMustMatchEffectiveDateValidationRule(dto);

            // Assert
            sut.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineAutoMoqData]
        public void IsValid_WhenEndDateIsTheSameAsStartDate_ShouldParseValidation(ChargeInformationOperationDtoBuilder chargeInformationOperationDtoBuilder)
        {
            // Arrange
            var instant = SystemClock.Instance.GetCurrentInstant();
            var dto = chargeInformationOperationDtoBuilder.WithStartDateTime(instant).WithEndDateTime(instant).Build();
            var sut = new TerminationDateMustMatchEffectiveDateValidationRule(dto);

            // Assert
            sut.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineAutoMoqData]
        public void IsValid_WhenEndDateIsDifferentFromStartDate_ShouldFailValidation(ChargeInformationOperationDtoBuilder chargeInformationOperationDtoBuilder)
        {
            // Arrange
            var dto = chargeInformationOperationDtoBuilder.Build();
            var sut = new TerminationDateMustMatchEffectiveDateValidationRule(dto);

            // Assert
            sut.IsValid.Should().BeFalse();
        }
    }
}
