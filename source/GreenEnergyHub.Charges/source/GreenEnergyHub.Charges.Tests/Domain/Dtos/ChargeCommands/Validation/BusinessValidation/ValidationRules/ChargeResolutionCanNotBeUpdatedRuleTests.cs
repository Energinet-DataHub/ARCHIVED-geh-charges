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
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.BusinessValidation.ValidationRules
{
    [UnitTest]
    public class ChargeResolutionCanNotBeUpdatedRuleTests
    {
        [Theory]
        [InlineAutoDomainData(Resolution.Unknown)]
        [InlineAutoDomainData(Resolution.P1D)]
        [InlineAutoDomainData(Resolution.P1M)]
        [InlineAutoDomainData(Resolution.PT1H)]
        [InlineAutoDomainData(Resolution.PT15M)]
        public void IsValid_WhenResolutionIsTheSame_ShouldReturnTrue(
            Resolution resolution,
            ChargeBuilder chargeBuilder)
        {
            var charge = chargeBuilder.WithResolution(resolution).Build();
            var sut = new ChargeResolutionCanNotBeUpdatedRule(charge.Resolution, resolution);
            sut.IsValid.Should().BeTrue();
        }

        // To any future developers reading this boilerplate code. Know that Mr. Henrik Sommer made me do it.
        // Don't hate the player, hate the game.
        [Theory]
        [InlineAutoDomainData(Resolution.Unknown, Resolution.Unknown, true)]
        [InlineAutoDomainData(Resolution.Unknown, Resolution.P1D, false)]
        [InlineAutoDomainData(Resolution.Unknown, Resolution.P1M, false)]
        [InlineAutoDomainData(Resolution.Unknown, Resolution.PT1H, false)]
        [InlineAutoDomainData(Resolution.Unknown, Resolution.PT15M, false)]
        [InlineAutoDomainData(Resolution.P1D, Resolution.Unknown, false)]
        [InlineAutoDomainData(Resolution.P1D, Resolution.P1D, true)]
        [InlineAutoDomainData(Resolution.P1D, Resolution.P1M, false)]
        [InlineAutoDomainData(Resolution.P1D, Resolution.PT1H, false)]
        [InlineAutoDomainData(Resolution.P1D, Resolution.PT15M, false)]
        [InlineAutoDomainData(Resolution.P1M, Resolution.Unknown, false)]
        [InlineAutoDomainData(Resolution.P1M, Resolution.P1D, false)]
        [InlineAutoDomainData(Resolution.P1M, Resolution.P1M, true)]
        [InlineAutoDomainData(Resolution.P1M, Resolution.PT1H, false)]
        [InlineAutoDomainData(Resolution.P1M, Resolution.PT15M, false)]
        [InlineAutoDomainData(Resolution.PT1H, Resolution.Unknown, false)]
        [InlineAutoDomainData(Resolution.PT1H, Resolution.P1D, false)]
        [InlineAutoDomainData(Resolution.PT1H, Resolution.P1M, false)]
        [InlineAutoDomainData(Resolution.PT1H, Resolution.PT1H, true)]
        [InlineAutoDomainData(Resolution.PT1H, Resolution.PT15M, false)]
        [InlineAutoDomainData(Resolution.PT15M, Resolution.Unknown, false)]
        [InlineAutoDomainData(Resolution.PT15M, Resolution.P1D, false)]
        [InlineAutoDomainData(Resolution.PT15M, Resolution.P1M, false)]
        [InlineAutoDomainData(Resolution.PT15M, Resolution.PT1H, false)]
        [InlineAutoDomainData(Resolution.PT15M, Resolution.PT15M, true)]
        public void IsValid_WhenResolutionIsNotTheSame_ShouldReturnFalse(
            Resolution existingResolution,
            Resolution newResolution,
            bool isValid,
            ChargeBuilder chargeBuilder)
        {
            var charge = chargeBuilder.WithResolution(existingResolution).Build();
            var sut = new ChargeResolutionCanNotBeUpdatedRule(charge.Resolution, newResolution);
            sut.IsValid.Should().Be(isValid);
        }
    }
}
