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

using System;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using GreenEnergyHub.TestHelpers;
using NodaTime;
using NodaTime.Extensions;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargePriceCommands.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class EffectiveDateMustMatchStartDateRuleTests
    {
        [Theory]
        [AutoDomainData]
        public void IsValid_WhenEffectiveDateAndPointStartDateAreEqual_ShouldReturnTrue(ChargePriceOperationDtoBuilder chargePriceOperationDtoBuilder)
        {
            // Arrange
            var startDateTime = new DateTime(2020, 2, 2, 0, 0, 0, DateTimeKind.Utc).ToInstant();
            var pointStartDateTime = new DateTime(2020, 2, 2, 0, 0, 0, DateTimeKind.Utc).ToInstant();

            var chargeOperationDto = chargePriceOperationDtoBuilder
                .WithStartDateTime(startDateTime)
                .WithPointsInterval(pointStartDateTime, pointStartDateTime).Build();
            chargeOperationDto.Points.Clear();
            var sut = new EffectiveDateMustMatchStartDateRule(chargeOperationDto);

            // Act / Assert
            sut.IsValid.Should().BeTrue();
        }

        [Theory]
        [AutoDomainData]
        public void IsValid_WhenEffectiveDateAndPointStartDateAreNotEqual_ShouldReturnFalse(ChargePriceOperationDtoBuilder chargePriceOperationDtoBuilder)
        {
            // Arrange
            var chargeOperationDto = CreateInvalidOperation(chargePriceOperationDtoBuilder);
            chargeOperationDto.Points.Clear();
            var sut = new EffectiveDateMustMatchStartDateRule(chargeOperationDto);

            // Act / Assert
            sut.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineAutoDomainData]
        public void ValidationRuleIdentifier_ShouldBe_EqualTo(ChargePriceOperationDtoBuilder chargePriceOperationDtoBuilder)
        {
            // Arrange And Act
            var sut = new EffectiveDateMustMatchStartDateRule(CreateInvalidOperation(chargePriceOperationDtoBuilder));

            // Assert
            sut.ValidationRuleIdentifier.Should().Be(ValidationRuleIdentifier.EffectiveDateMustMatchStartDate);
        }

        private static ChargePriceOperationDto CreateInvalidOperation(ChargePriceOperationDtoBuilder chargePriceOperationDtoBuilder)
        {
            var startDateTime = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToInstant();
            var pointStartDateTime = new DateTime(2020, 2, 2, 0, 0, 0, DateTimeKind.Utc).ToInstant();

            return chargePriceOperationDtoBuilder
                .WithStartDateTime(startDateTime)
                .WithPointsInterval(pointStartDateTime, pointStartDateTime).Build();
        }
    }
}
