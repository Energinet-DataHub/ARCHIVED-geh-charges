﻿// Copyright 2020 Energinet DataHub A/S
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
using GreenEnergyHub.Charges.Domain.ChargeInformation;
using GreenEnergyHub.Charges.Domain.Common;
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
        [InlineAutoDomainData]
        public void IsValid_WhenResolutionIsTheSame_ShouldReturnTrue(
            ChargeOperationDtoBuilder builder,
            ChargeInformation chargeInformation)
        {
            var chargeOperationDto = builder.WithResolution(chargeInformation.Resolution).Build();
            var sut = new ChargeResolutionCanNotBeUpdatedRule(chargeInformation, chargeOperationDto);
            sut.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineAutoDomainData]
        public void IsValid_WhenResolutionIsNotTheSame_ShouldReturnFalse(
            ChargeOperationDtoBuilder builder,
            ChargeInformation chargeInformation)
        {
            var chargeOperationDto = builder
                .WithResolution(chargeInformation.Resolution == Resolution.P1D ? Resolution.P1M : Resolution.PT1H)
                .Build();
            var sut = new ChargeResolutionCanNotBeUpdatedRule(chargeInformation, chargeOperationDto);
            sut.IsValid.Should().BeFalse();
        }
    }
}
