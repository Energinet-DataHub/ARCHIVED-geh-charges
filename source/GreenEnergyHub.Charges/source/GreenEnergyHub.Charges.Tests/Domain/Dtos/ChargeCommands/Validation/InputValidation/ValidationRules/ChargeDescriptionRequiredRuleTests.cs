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

using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class ChargeDescriptionRequiredRuleTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void IsValid_WhenChargeDescriptionIsValid_ShouldReturnTrue(ChargeOperationDtoBuilder builder)
        {
            // Arrange
            var dto = builder.WithDescription("Valid name").Build();
            var sut = new ChargeDescriptionRequiredRule(dto);

            // Assert
            sut.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineAutoMoqData]
        public void IsValid_WhenChargeDescriptionIsNull_ShouldReturnFalse(ChargeOperationDtoBuilder builder)
        {
            // Arrange
            var dto = builder.WithDescription(null!).Build();
            var sut = new ChargeDescriptionRequiredRule(dto);

            // Assert
            sut.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineAutoMoqData]
        public void IsValid_WhenChargeDescriptionIsEmpty_ShouldReturnFalse(ChargeOperationDtoBuilder builder)
        {
            // Arrange
            var dto = builder.WithDescription(string.Empty).Build();
            var sut = new ChargeDescriptionRequiredRule(dto);

            // Assert
            sut.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineAutoMoqData]
        public void IsValid_WhenChargeDescriptionIsAllWhitespace_ShouldReturnFalse(ChargeOperationDtoBuilder builder)
        {
            // Arrange
            var dto = builder.WithChargeName("       ").Build();
            var sut = new ChargeDescriptionRequiredRule(dto);

            // Assert
            sut.IsValid.Should().BeFalse();
        }
    }
}
