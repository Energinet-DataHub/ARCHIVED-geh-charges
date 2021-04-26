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
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.TestHelpers;
using GreenEnergyHub.TestHelpers.Traits;
using Moq;
using NodaTime;
using NodaTime.Text;
using Xunit;

namespace GreenEnergyHub.Iso8601.Tests
{
    [Trait(TraitNames.Category, TraitValues.UnitTest)]
    public class Iso8601DurationsTests
    {
        [Theory]
        [InlineAutoDomainData(0)]
        [InlineAutoDomainData(1)]
        [InlineAutoDomainData(2)]
        [InlineAutoDomainData(10)]
        public void Iso8601Durations_AddOneHour(
            int hours,
            Iso8601Durations iso8601Durations)
        {
            // Arrange
            var startTime = SystemClock.Instance.GetCurrentInstant();

            // Act
            var actual = iso8601Durations.AddDuration(startTime, "PT1H", hours);

            // Assert
            var duration = actual - startTime;
            duration.Hours.Should().Be(hours);
        }

        [Theory]
        [InlineAutoDomainData(0)]
        [InlineAutoDomainData(1)]
        [InlineAutoDomainData(2)]
        [InlineAutoDomainData(3)]
        public void Iso8601Durations_AddQuarterOfAnHour(
            int quarters,
            Iso8601Durations iso8601Durations)
        {
            // Arrange
            var startTime = SystemClock.Instance.GetCurrentInstant();

            // Act
            var actual = iso8601Durations.AddDuration(startTime, "PT15M", quarters);

            // Assert
            var duration = actual - startTime;
            duration.Minutes.Should().Be(15 * quarters);
        }

        [Theory]
        [InlineAutoDomainData("2020-12-31T23:00:00Z", 0, "Europe/Copenhagen", "2020-12-31T23:00:00Z")]
        [InlineAutoDomainData("2020-12-31T23:00:00Z", 1, "Europe/Copenhagen", "2021-01-31T23:00:00Z")]
        [InlineAutoDomainData("2020-12-31T23:00:00Z", 2, "Europe/Copenhagen", "2021-02-28T23:00:00Z")]
        [InlineAutoDomainData("2020-12-31T23:00:00Z", 3, "Europe/Copenhagen", "2021-03-31T22:00:00Z")]
        [InlineAutoDomainData("2020-12-31T23:00:00Z", 10, "Europe/Copenhagen", "2021-10-31T23:00:00Z")]
        [InlineAutoDomainData("2020-12-31T23:00:00Z", 10, "America/New_York", "2021-10-31T22:00:00Z")]
        [InlineAutoDomainData("2020-09-25T02:00:00Z", 1, "Europe/Copenhagen", "2020-10-25T03:00:00Z")]
        [InlineAutoDomainData("2020-09-25T02:00:00Z", 2, "Europe/Copenhagen", "2020-11-25T03:00:00Z")]
        [InlineAutoDomainData("2020-09-25T02:00:00Z", 12, "Europe/Copenhagen", "2021-09-25T02:00:00Z")]
        [InlineAutoDomainData("2023-12-31T23:00:00Z", 2, "Europe/Copenhagen", "2024-02-29T23:00:00Z")]
        [InlineAutoDomainData("2023-12-31T23:00:00Z", 3, "Europe/Copenhagen", "2024-03-31T22:00:00Z")]
        public void Iso8601Durations_AddMonth(
            string startTimeString,
            int months,
            string timeZoneId,
            string expectedTimeString,
            [Frozen] Mock<IIso8601ConversionConfiguration> configuration,
            Iso8601Durations sut)
        {
            // Arrange
            configuration.Setup(x => x.GetTZDatabaseName()).Returns(timeZoneId);
            var startTime = InstantPattern.General.Parse(startTimeString).Value;
            var expectedTime = InstantPattern.General.Parse(expectedTimeString).Value;

            // Act
            var actual = sut.AddDuration(startTime, "P1M", months);

            // Assert
            Assert.Equal(expectedTime, actual);
        }

        [Theory]
        [InlineAutoDomainData("2021-02-28T01:00:00Z", "P1M", 1, "Europe/Copenhagen")]
        [InlineAutoDomainData("2021-03-27T01:00:00Z", "P1D", 1, "Europe/Copenhagen")]
        public void Iso8601Durations_AddDurationWhenTimeHasAmbiguity_ThrowSkippedTimeException(
            string startTimeString,
            string duration,
            int months,
            string timeZoneId,
            [Frozen] Mock<IIso8601ConversionConfiguration> configuration,
            Iso8601Durations sut)
        {
            // Arrange
            configuration.Setup(x => x.GetTZDatabaseName()).Returns(timeZoneId);
            var startTime = InstantPattern.General.Parse(startTimeString).Value;

            // Act and assert
            Assert.Throws<NodaTime.SkippedTimeException>(() => sut.AddDuration(startTime, duration, months));
        }

        [Theory]
        [InlineAutoDomainData(1)]
        public void Iso8601Durations_AddMonthIfConfiguredWrong_ThrowArgumentException(
            int months,
            [Frozen] Mock<IIso8601ConversionConfiguration> configuration,
            Iso8601Durations iso8601Durations)
        {
            // Arrange
            var wrongTimeZone = "NoTimeZoneIsCalledThis";
            configuration.Setup(x => x.GetTZDatabaseName()).Returns(wrongTimeZone);
            var startTime = SystemClock.Instance.GetCurrentInstant();

            // Act and assert
            Assert.Throws<ArgumentException>(() => iso8601Durations.AddDuration(startTime, "P1M", months));
        }

        [Theory]
        [InlineAutoDomainData("P1Y", 1)]
        public void Iso8601Durations_UseUnsupportedDuration_ThrowArgumentException(
            string duration,
            int hours,
            Iso8601Durations iso8601Durations)
        {
            // Arrange
            var startTime = SystemClock.Instance.GetCurrentInstant();

            // Act and assert
            Assert.Throws<ArgumentException>(() => iso8601Durations.AddDuration(startTime, duration, hours));
        }

        [Theory]
        [InlineAutoDomainData("2020-12-31T23:00:00Z", 0, "Europe/Copenhagen", "2020-12-31T23:00:00Z")]
        [InlineAutoDomainData("2020-12-31T23:00:00Z", 1, "Europe/Copenhagen", "2021-01-01T23:00:00Z")]
        [InlineAutoDomainData("2020-12-31T23:00:00Z", 2, "Europe/Copenhagen", "2021-01-02T23:00:00Z")]
        [InlineAutoDomainData("2020-12-31T23:00:00Z", 3, "Europe/Copenhagen", "2021-01-03T23:00:00Z")]
        [InlineAutoDomainData("2021-02-27T23:00:00Z", 1, "Europe/Copenhagen", "2021-02-28T23:00:00Z")]
        [InlineAutoDomainData("2021-02-27T23:00:00Z", 2, "Europe/Copenhagen", "2021-03-01T23:00:00Z")]
        [InlineAutoDomainData("2024-02-27T23:00:00Z", 1, "Europe/Copenhagen", "2024-02-28T23:00:00Z")]
        [InlineAutoDomainData("2024-02-27T23:00:00Z", 2, "Europe/Copenhagen", "2024-02-29T23:00:00Z")]
        [InlineAutoDomainData("2021-03-27T23:00:00Z", 1, "Europe/Copenhagen", "2021-03-28T22:00:00Z")]
        [InlineAutoDomainData("2020-10-24T22:00:00Z", 1, "Europe/Copenhagen", "2020-10-25T23:00:00Z")]
        public void Iso8601Durations_AddDay(
            string startTimeString,
            int days,
            string timeZoneId,
            string expectedTimeString,
            [Frozen] Mock<IIso8601ConversionConfiguration> configuration,
            Iso8601Durations sut)
        {
            // Arrange
            configuration.Setup(x => x.GetTZDatabaseName()).Returns(timeZoneId);
            var startTime = InstantPattern.General.Parse(startTimeString).Value;
            var expectedTime = InstantPattern.General.Parse(expectedTimeString).Value;

            // Act
            var actual = sut.AddDuration(startTime, "P1D", days);

            // Assert
            Assert.Equal(expectedTime, actual);
        }
    }
}
