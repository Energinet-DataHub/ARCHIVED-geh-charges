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
using FluentAssertions;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Iso8601;
using Microsoft.EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using NodaTime;
using NodaTime.Testing;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargePriceCommands.Validation.BusinessRules.ValidationRules
{
    [UnitTest]
    public class MonthlyPriceSeriesEndDateMustBeFirstOfMonthRuleOrEqualChargeStopDateTests
    {
        [Theory]
        [InlineAutoMoqData(Resolution.P1M, 15, 1, 0, 0, false)]
        [InlineAutoMoqData(Resolution.P1D, 15, 1, 0, 0, true)]
        [InlineAutoMoqData(Resolution.PT15M, 15, 1, 0, 0, true)]
        [InlineAutoMoqData(Resolution.PT1H, 15, 1, 0, 0, true)]
        [InlineAutoMoqData(Resolution.P1M, 15, 1, 16, 1, false)]
        [InlineAutoMoqData(Resolution.P1M, 15, 1, 15, 1, true)]
        [InlineAutoMoqData(Resolution.P1M, 1, 2, 0, 0, true)]
        [InlineAutoMoqData(Resolution.P1M, 1, 2, 1, 2, true)]
        [InlineAutoMoqData(Resolution.P1M, 15, 2, 15, 2, true)]
        public void IsValid_WhenCalled_Should_ReturnExpectedResult(
            Resolution resolution,
            int priceEndDayDateTime,
            int priceEndMonthDateTime,
            int priceStopDayDateTime,
            int priceStopMonthDateTime,
            bool isValidExpectedValue)
        {
            // Arrange
            var endDate = GetInstantFromMonthAndDay(priceEndDayDateTime, priceEndMonthDateTime);
            var stopDate = priceStopMonthDateTime == 0
                ? InstantHelper.GetEndDefault()
                : GetInstantFromMonthAndDay(priceStopDayDateTime, priceStopMonthDateTime);
            var sut = new MonthlyPriceSeriesEndDateMustBeFirstOfMonthOrEqualChargeStopDateRule(
                resolution,
                endDate,
                stopDate);

            // Act
            var actual = sut.IsValid;

            // Assert
            actual.Should().Be(isValidExpectedValue);
        }

        [Fact]
        public void ValidationRuleIdentifier_ShouldBeUpdateTaxTariffOnlyAllowedBySystemOperator()
        {
            // Arrange
            // Act
            var sut = new MonthlyPriceSeriesEndDateMustBeFirstOfMonthOrEqualChargeStopDateRule(
                Resolution.P1M,
                GetInstantFromMonthAndDay(1, 1),
                GetInstantFromMonthAndDay(1, 1));

            // Assert
            sut.ValidationRuleIdentifier.Should()
                .Be(ValidationRuleIdentifier.MonthlyPriceSeriesEndDateMustBeFirstOfMonthOrEqualChargeStopDate);
        }

        private static Instant GetInstantFromMonthAndDay(int priceEndDayDateTimeUtc, int priceEndMonthDateTimeUtc)
        {
            return Instant.FromDateTimeUtc(new DateTime(
                2023,
                priceEndMonthDateTimeUtc,
                priceEndDayDateTimeUtc,
                0,
                0,
                0,
                DateTimeKind.Utc))
                .PlusHours(-1);
        }

        private static ZonedDateTimeService GetZonedDateTimeService()
        {
            var clock = new FakeClock(InstantHelper.GetTodayAtMidnightUtc());
            return new ZonedDateTimeService(clock, new Iso8601ConversionConfiguration("Europe/Copenhagen"));
        }
    }
}
