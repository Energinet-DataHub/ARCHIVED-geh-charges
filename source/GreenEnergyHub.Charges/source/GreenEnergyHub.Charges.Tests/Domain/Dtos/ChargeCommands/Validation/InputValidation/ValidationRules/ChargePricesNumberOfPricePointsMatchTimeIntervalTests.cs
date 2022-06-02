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

using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using Microsoft.EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation.InputValidation.ValidationRules
{
    [UnitTest]
    public class ChargePricesNumberOfPricePointsMatchTimeIntervalTests
    {
        [Theory]
        [InlineAutoMoqData(Resolution.PT15M, 1, 1, 23, 1, 2, 23, 96, "")]
        [InlineAutoMoqData(Resolution.PT1H, 1, 1, 23, 1, 2, 23, 24, "")]
        [InlineAutoMoqData(Resolution.P1D, 1, 1, 23, 1, 2, 23, 1, "")]
        [InlineAutoMoqData(Resolution.P1M, 1, 1, 23, 2, 1, 23, 1, "")]
        [InlineAutoMoqData(Resolution.P1M, 1, 15, 23, 2, 1, 23, 1, "irregular price series must be allowed")]
        [InlineAutoMoqData(Resolution.PT1H, 3, 26, 23, 3, 27, 22, 23, "switching to Daylight Saving Time must be supported")]
        [InlineAutoMoqData(Resolution.PT1H, 10, 29, 22, 10, 30, 23, 25, "switching to Normal Time must be supported")]
        [InlineAutoMoqData(Resolution.P1D, 1, 1, 23, 1, 6, 23, 5, "longer price series must be supported")]
        public void IsValid_When_Should(
            Resolution resolution,
            int startMonth,
            int startDay,
            int startHour,
            int endMonth,
            int endDay,
            int endHour,
            int expectedNumberOfPoints,
            string because)
        {
            // Arrange
            var start = Instant.FromUtc(2022, startMonth, startDay, startHour, 0);
            var end = Instant.FromUtc(2022, endMonth, endDay, endHour, 0);

            var dto = new ChargeOperationDtoBuilder()
                .WithResolution(resolution)
                .WithPointWithXNumberOfPrices(expectedNumberOfPoints)
                .WithPointsInterval(start, end)
                .Build();

            var sut = new ChargePricesNumberOfPricePointsMatchTimeInterval(dto);

            // Act
            var actual = sut.IsValid;

            // Assert
            actual.Should().BeTrue(because);
        }
    }
}
