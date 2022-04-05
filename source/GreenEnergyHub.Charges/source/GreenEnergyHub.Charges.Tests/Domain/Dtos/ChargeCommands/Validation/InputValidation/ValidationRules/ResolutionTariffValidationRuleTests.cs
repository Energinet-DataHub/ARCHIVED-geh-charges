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

using System.Linq;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class ResolutionTariffValidationRuleTests
    {
        [Theory]
        [InlineAutoMoqData(Resolution.Unknown, false)]
        [InlineAutoMoqData(Resolution.P1D, true)]
        [InlineAutoMoqData(Resolution.P1M, false)]
        [InlineAutoMoqData(Resolution.PT1H, true)]
        [InlineAutoMoqData(Resolution.PT15M, true)]
        [InlineAutoMoqData(-1, false)]
        public void ResolutionTariffValidationRule_WithTariffType_EqualsExpectedResult(
            Resolution resolution,
            bool expected,
            ChargeOperationDtoBuilder builder)
        {
            // Arrange
            var chargeOperationDto = builder
                .WithChargeType(ChargeType.Tariff)
                .WithResolution(resolution)
                .Build();

                // Act
            var sut = new ResolutionTariffValidationRule(chargeOperationDto);

            // Assert
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData(Resolution.Unknown, true)]
        [InlineAutoMoqData(Resolution.P1D, true)]
        [InlineAutoMoqData(Resolution.P1M, true)]
        [InlineAutoMoqData(Resolution.PT1H, true)]
        [InlineAutoMoqData(Resolution.PT15M, true)]
        public void ResolutionTariffValidationRule_WithSubscriptionType_EqualsExpectedResult(
            Resolution resolution,
            bool expected,
            ChargeOperationDtoBuilder builder)
        {
            // Arrange
            var chargeOperationDto = builder
                .WithChargeType(ChargeType.Subscription)
                .WithResolution(resolution)
                .Build();

            // Act
            var sut = new ResolutionTariffValidationRule(chargeOperationDto);

            // Assert
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData(Resolution.Unknown, true)]
        [InlineAutoMoqData(Resolution.P1D, true)]
        [InlineAutoMoqData(Resolution.P1M, true)]
        [InlineAutoMoqData(Resolution.PT1H, true)]
        [InlineAutoMoqData(Resolution.PT15M, true)]
        public void ResolutionTariffValidationRule_WithFeeType_EqualsExpectedResult(
            Resolution resolution,
            bool expected,
            ChargeOperationDtoBuilder builder)
        {
            // Arrange
            var chargeOperationDto = builder
                .WithChargeType(ChargeType.Fee)
                .WithResolution(resolution)
                .Build();

                // Act
            var sut = new ResolutionTariffValidationRule(chargeOperationDto);

            // Assert
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeOperationDtoBuilder builder)
        {
            var chargeOperationDto = builder
                .WithChargeType(ChargeType.Tariff)
                .WithResolution(Resolution.Unknown)
                .Build();
            var sut = new ResolutionTariffValidationRule(chargeOperationDto);
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.ResolutionTariffValidation);
        }
    }
}
