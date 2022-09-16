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
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class ResolutionIsRequiredRuleTests
    {
        [Theory]
        [InlineAutoMoqData(Resolution.Unknown, false)]
        [InlineAutoMoqData(Resolution.PT15M, true)]
        [InlineAutoMoqData(Resolution.PT1H, true)]
        [InlineAutoMoqData(Resolution.P1D, true)]
        [InlineAutoMoqData(Resolution.P1M, true)]
        public void IsValid_WhenCalled_ShouldReturnExpectedValue(
        Resolution resolution,
        bool expectedResult,
        ChargeInformationOperationDtoBuilder builder)
        {
            // Arrange
            var dto = builder.WithResolution(resolution).Build();

            // Act
            var sut = new ResolutionIsRequiredRule(dto);

            // Assert
            sut.IsValid.Should().Be(expectedResult);
        }
    }
}
