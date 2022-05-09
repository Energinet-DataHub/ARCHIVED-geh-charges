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
using GreenEnergyHub.Charges.Domain.Common;
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
    public class ResolutionSubscriptionValidationRuleTests
    {
        [Theory]
        [InlineAutoMoqData(Resolution.Unknown, true)]
        [InlineAutoMoqData(Resolution.P1D, true)]
        [InlineAutoMoqData(Resolution.P1M, true)]
        [InlineAutoMoqData(Resolution.PT1H, true)]
        [InlineAutoMoqData(Resolution.PT15M, true)]
        public void ResolutionSubscriptionValidationRule_WithTariffType_EqualsExpectedResult(
            Resolution resolution,
            bool expected,
            ChargeOperationDtoBuilder chargeOperationDtoBuilder)
        {
            // Arrange
            var chargeOperationDto = chargeOperationDtoBuilder
                .WithChargeType(ChargeType.Tariff)
                .WithResolution(resolution)
                .Build();

            // Act
            var sut = new ResolutionSubscriptionValidationRule(chargeOperationDto);

            // Assert
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData(Resolution.Unknown, false)]
        [InlineAutoMoqData(Resolution.P1D, false)]
        [InlineAutoMoqData(Resolution.P1M, true)]
        [InlineAutoMoqData(Resolution.PT1H, false)]
        [InlineAutoMoqData(Resolution.PT15M, false)]
        public void ResolutionSubscriptionValidationRule_WithSubscriptionType_EqualsExpectedResult(
            Resolution resolution,
            bool expected,
            ChargeOperationDtoBuilder chargeOperationDtoBuilder)
        {
            // Arrange
            var chargeOperationDto = chargeOperationDtoBuilder
                .WithChargeType(ChargeType.Subscription)
                .WithResolution(resolution)
                .Build();

            // Act
            var sut = new ResolutionSubscriptionValidationRule(chargeOperationDto);

            // Assert
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoMoqData(Resolution.Unknown, true)]
        [InlineAutoMoqData(Resolution.P1D, true)]
        [InlineAutoMoqData(Resolution.P1M, true)]
        [InlineAutoMoqData(Resolution.PT1H, true)]
        [InlineAutoMoqData(Resolution.PT15M, true)]
        public void ResolutionSubscriptionValidationRule_WithFeeType_EqualsExpectedResult(
            Resolution resolution,
            bool expected,
            ChargeOperationDtoBuilder chargeOperationDtoBuilder)
        {
            // Assert
            var chargeOperationDto = chargeOperationDtoBuilder
                .WithChargeType(ChargeType.Fee)
                .WithResolution(resolution)
                .Build();

            // Act
            var sut = new ResolutionSubscriptionValidationRule(chargeOperationDto);

            // Assert
            sut.IsValid.Should().Be(expected);
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargeOperationDtoBuilder chargeOperationDtoBuilder)
        {
            var chargeOperationDto = chargeOperationDtoBuilder
                .WithChargeType(ChargeType.Subscription)
                .WithResolution(Resolution.Unknown)
                .Build();
            var sut = new ResolutionSubscriptionValidationRule(chargeOperationDto);
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.ResolutionSubscriptionValidation);
        }

        [Theory]
        [InlineAutoDomainData]
        public void OperationId_ShouldBe_EqualTo(ChargeOperationDto chargeOperationDto)
        {
            var sut = new ResolutionSubscriptionValidationRule(chargeOperationDto);
            sut.OperationId.Should().Be(chargeOperationDto.Id);
        }
    }
}
