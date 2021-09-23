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

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.ChargeCommands;
using GreenEnergyHub.Charges.Domain.ChargeCommands.Validation;
using GreenEnergyHub.Charges.Domain.ChargeCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class ResolutionSubscriptionValidationRuleTests
    {
        [Theory]
        [InlineAutoMoqData(Resolution.Unknown, true)]
        [InlineAutoMoqData(Resolution.P1D, true)]
        [InlineAutoMoqData(Resolution.P1M, true)]
        [InlineAutoMoqData(Resolution.PT1H, true)]
        [InlineAutoMoqData(Resolution.PT15M, true)]
        public void ResolutionTariffValidationRule_WithTariffType_EqualsExpectedResult(
            Resolution resolution,
            bool expected,
            [NotNull] ChargeCommand command)
        {
            // Arrange
            command.ChargeOperation.Type = ChargeType.Tariff;
            command.ChargeOperation.Resolution = resolution;

            // Act
            var sut = new ResolutionSubscriptionValidationRule(command);

            // Assert
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData(Resolution.Unknown, false)]
        [InlineAutoMoqData(Resolution.P1D, false)]
        [InlineAutoMoqData(Resolution.P1M, true)]
        [InlineAutoMoqData(Resolution.PT1H, false)]
        [InlineAutoMoqData(Resolution.PT15M, false)]
        public void ResolutionTariffValidationRule_WithSubscriptionType_EqualsExpectedResult(
            Resolution resolution,
            bool expected,
            [NotNull] ChargeCommand command)
        {
            // Arrange
            command.ChargeOperation.Type = ChargeType.Subscription;
            command.ChargeOperation.Resolution = resolution;

            // Act
            var sut = new ResolutionSubscriptionValidationRule(command);

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
            [NotNull] ChargeCommand command)
        {
            // Assert
            command.ChargeOperation.Type = ChargeType.Fee;
            command.ChargeOperation.Resolution = resolution;

            // Act
            var sut = new ResolutionSubscriptionValidationRule(command);

            // Assert
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo([NotNull] ChargeCommand command)
        {
            var sut = new ResolutionSubscriptionValidationRule(command);
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.ResolutionSubscriptionValidation);
        }
    }
}
