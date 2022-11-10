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
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Iso8601;
using NodaTime.Text;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.Iso8601
{
    public class Iso8601DurationsTests
    {
        [Theory]
        [InlineAutoMoqData("2022-12-31T23:00:00Z", "P1M", "2023-01-31T23:00:00Z", "regular monthly price series")]
        [InlineAutoMoqData("2023-01-14T23:00:00Z", "P1M", "2023-01-31T23:00:00Z", "irregular monthly price series")]
        [InlineAutoMoqData("2023-03-14T23:00:00Z", "P1M", "2023-03-31T22:00:00Z", "irregular monthly price series crossing daylight saving")]
        [InlineAutoMoqData("2023-10-14T22:00:00Z", "P1M", "2023-10-31T23:00:00Z", "irregular monthly price series crossing daylight saving")]
        public void AddDurationWithIrregularSupport_WhenCalled_ShouldReturnCorrectlyAddedDuration(
            string isoString, string duration, string expectedIsoString, string reason)
        {
            // Arrange
            var sut = new Iso8601Durations(new Iso8601ConversionConfiguration("Europe/Copenhagen"));
            var input = InstantPattern.ExtendedIso.Parse(isoString).Value;
            var expected = InstantPattern.ExtendedIso.Parse(expectedIsoString).Value;

            // Act
            var actual = sut.AddDurationWithIrregularSupport(input, duration);

            // Assert
            actual.Should().Be(expected, reason);
        }
    }
}
