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

using System;
using System.Collections.Generic;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargePriceCommands.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class NumberOfPointsMatchTimeIntervalAndResolutionRuleTests
    {
        [Theory]
        [InlineAutoMoqData(Resolution.PT15M, 2022, 1, 1, 23, 2022, 1, 2, 23, 96, "")]
        [InlineAutoMoqData(Resolution.PT1H, 2022, 1, 1, 23, 2022, 1, 2, 23, 24, "")]
        [InlineAutoMoqData(Resolution.P1D, 2022, 1, 1, 23, 2022, 1, 2, 23, 1, "")]
        [InlineAutoMoqData(Resolution.P1M, 2021, 12, 31, 23, 2022, 1, 31, 23, 1, "")]
        [InlineAutoMoqData(Resolution.P1M, 2022, 1, 15, 23, 2022, 1, 31, 23, 1, "irregular price series must be allowed")]
        [InlineAutoMoqData(Resolution.P1M, 2022, 1, 15, 23, 2022, 2, 28, 23, 2, "irregular price series must be allowed")]
        [InlineAutoMoqData(Resolution.PT1H, 2022, 3, 26, 23, 2022, 3, 27, 22, 23, "switching to Daylight Saving Time must be supported")]
        [InlineAutoMoqData(Resolution.P1D, 2022, 3, 26, 23, 2022, 3, 27, 22, 1, "switching to Daylight Saving Time must be supported")]
        [InlineAutoMoqData(Resolution.P1M, 2022, 2, 28, 23, 2022, 3, 31, 22, 1, "switching to Daylight Saving Time must be supported")]
        [InlineAutoMoqData(Resolution.P1M, 2022, 3, 26, 23, 2022, 3, 31, 22, 1, "switching to Daylight Saving Time with irregular price series must be supported")]
        [InlineAutoMoqData(Resolution.PT1H, 2022, 10, 29, 22, 2022, 10, 30, 23, 25, "switching to Normal Time must be supported")]
        [InlineAutoMoqData(Resolution.P1D, 2022, 10, 29, 22, 2022, 10, 30, 23, 1, "switching to Normal Time must be supported")]
        [InlineAutoMoqData(Resolution.P1M, 2022, 9, 30, 22, 2022, 10, 31, 23, 1, "switching to Normal Time must be supported")]
        [InlineAutoMoqData(Resolution.P1M, 2022, 10, 29, 22, 2022, 10, 31, 23, 1, "switching to Normal Time, with irregular price series must be supported")]
        [InlineAutoMoqData(Resolution.P1D, 2022, 1, 1, 23, 2022, 1, 6, 23, 5, "longer price series must be supported")]
        [InlineAutoMoqData(Resolution.P1M, 2021, 12, 31, 23, 2023, 12, 31, 23, 24, "longer price series spanning years must be supported")]
        [InlineAutoMoqData(Resolution.P1M, 2022, 6, 14, 22, 2022, 7, 31, 22, 2, "irregular price series covering more than a single month must be supported")]
        [InlineAutoMoqData(Resolution.P1M, 2022, 6, 14, 22, 2022, 06, 20, 22, 1, "irregular price series starting and stopping within a single month must be supported")]
        [InlineAutoMoqData(Resolution.P1M, 2022, 6, 14, 22, 2022, 10, 31, 23, 5, "longer irregular price series spanning months including switch to Normal Time must be supported")]
        public void IsValid_WhenCalledWithCorrectNumberOfPrices_ShouldParse(
            Resolution resolution,
            int startYear,
            int startMonth,
            int startDay,
            int startHour,
            int endYear,
            int endMonth,
            int endDay,
            int endHour,
            int expectedNumberOfPoints,
            string because)
        {
            // Arrange
            var start = Instant.FromUtc(startYear, startMonth, startDay, startHour, 0);
            var end = Instant.FromUtc(endYear, endMonth, endDay, endHour, 0);

            var dto = new ChargePriceOperationDtoBuilder()
                .WithPriceResolution(resolution)
                .WithPointWithXNumberOfPrices(expectedNumberOfPoints)
                .WithPointsInterval(start, end)
                .Build();

            var sut = new NumberOfPointsMatchTimeIntervalAndResolutionRule(dto);

            // Act
            var actual = sut.IsValid;

            // Assert
            actual.Should().BeTrue(because);
        }

        [Fact]
        public void IsValid_WhenCalledWithUnknownPriceResolution_ShouldParse()
        {
            // Arrange
            var dto = new ChargePriceOperationDto(
                "operationId",
                ChargeType.Tariff,
                "chargeId",
                "owner",
                Instant.MinValue,
                Instant.MinValue,
                Instant.MinValue,
                Instant.MinValue,
                Resolution.Unknown,
                new List<Point>());

            var sut = new NumberOfPointsMatchTimeIntervalAndResolutionRule(dto);

            // Act
            var actual = sut.IsValid;

            // Assert
            actual.Should().BeTrue("To avoid throwing an exception, instead it parses with 0 points expected");
        }

        [Fact]
        public void IsValid_WhenCalledWithNonExistingPriceResolution_ShouldThrowArgumentOutOfRangeException()
        {
            // Arrange
            // var dto = new ChargeInformationOperationDtoBuilder().WithPriceResolution((Resolution)99).Build();
            var dto = new ChargePriceOperationDtoBuilder().WithPriceResolution((Resolution)99).Build();

            // Act
            Action act = () =>
            {
                _ = new NumberOfPointsMatchTimeIntervalAndResolutionRule(dto);
            };

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }
    }
}
