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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class ChargeNameRequiredRuleTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void IsValid_WhenChargeNameIsValid_ShouldReturnTrue(ChargeInformationOperationDtoBuilder builder)
        {
            // Arrange
            var dto = builder.WithChargeName("Valid name").Build();
            var sut = new ChargeNameRequiredRule(dto);

            // Assert
            sut.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineAutoMoqData]
        public void IsValid_WhenChargeNameIsNull_ShouldReturnFalse(ChargeInformationOperationDtoBuilder builder)
        {
            // Arrange
            var dto = builder.WithChargeName(null!).Build();
            var sut = new ChargeNameRequiredRule(dto);

            // Assert
            sut.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineAutoMoqData]
        public void IsValid_WhenChargeNameIsEmpty_ShouldReturnFalse(ChargeInformationOperationDtoBuilder builder)
        {
            // Arrange
            var dto = builder.WithChargeName(string.Empty).Build();
            var sut = new ChargeNameRequiredRule(dto);

            // Assert
            sut.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineAutoMoqData]
        public void IsValid_WhenChargeNameIsAllWhitespace_ShouldReturnFalse(ChargeInformationOperationDtoBuilder builder)
        {
            // Arrange
            var dto = builder.WithChargeName("       ").Build();
            var sut = new ChargeNameRequiredRule(dto);

            // Assert
            sut.IsValid.Should().BeFalse();
        }
    }
}
