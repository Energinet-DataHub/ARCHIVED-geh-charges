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
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Iso8601;
using NodaTime;
using NodaTime.Testing;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargePriceCommands.Validation.BusinessRules.ValidationRules
{
    [UnitTest]
    public class MonthlyPriceSeriesEndDateMustBeFirstOfMonthRuleTests
    {
        [Theory]
        [InlineAutoMoqData(15, 1, 0, 0, false)]
        [InlineAutoMoqData(15, 1, 16, 1, false)]
        [InlineAutoMoqData(15, 1, 15, 1, true)]
        [InlineAutoMoqData(1, 2, 0, 0, true)]
        [InlineAutoMoqData(1, 2, 1, 2, true)]
        [InlineAutoMoqData(1, 2, 15, 2, true)]
        public void IsValid_WhenCalled_Should_ReturnExpectedResult(
            int priceEndDayDateTime,
            int priceEndMonthDateTime,
            int priceStopDayDateTime,
            int priceStopMonthDateTime,
            bool isValidExpectedValue)
        {
            // Arrange
            var endDate = GetInstantFromMonthAndDay(priceEndDayDateTime, priceEndMonthDateTime);
            Instant? stopDate = priceStopMonthDateTime == 0
                ? null
                : GetInstantFromMonthAndDay(priceStopDayDateTime, priceStopMonthDateTime);
            var sut = new MonthlyPriceSeriesEndDateMustBeFirstOfMonthRule(
                GetZonedDateTimeService(),
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
            var sut = new MonthlyPriceSeriesEndDateMustBeFirstOfMonthRule(
                GetZonedDateTimeService(),
                GetInstantFromMonthAndDay(1, 1),
                GetInstantFromMonthAndDay(1, 1));

            // Assert
            sut.ValidationRuleIdentifier.Should()
                .Be(ValidationRuleIdentifier.MonthlyPriceSeriesEndDateMustBeFirstOfMonth);
        }

        private static Instant GetInstantFromMonthAndDay(int priceEndDayDateTime, int priceEndMonthDateTime)
        {
            return Instant.FromDateTimeUtc(new DateTime(
                2023,
                priceEndMonthDateTime,
                priceEndDayDateTime,
                0,
                0,
                0,
                DateTimeKind.Utc));
        }

        private static ZonedDateTimeService GetZonedDateTimeService()
        {
            var clock = new FakeClock(InstantHelper.GetTodayAtMidnightUtc());
            return new ZonedDateTimeService(clock, new Iso8601ConversionConfiguration("Europe/Copenhagen"));
        }
    }
}
