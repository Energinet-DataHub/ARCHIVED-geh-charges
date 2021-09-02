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
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.TestCore.Attributes;
using NodaTime.Text;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Charges
{
    [UnitTest]
    public class DefaultChargeLinkTests
    {
        [Theory]
        [InlineAutoMoqData("2020-05-10T13:00:00Z", "2020-05-11T13:00:00Z", "2020-05-11T13:00:00Z")]
        [InlineAutoMoqData("2020-05-12T13:00:00Z", "2020-05-11T13:00:00Z", "2020-05-12T13:00:00Z")]
        [InlineAutoMoqData("2020-05-13T13:00:00Z", "2020-05-13T13:00:00Z", "2020-05-13T13:00:00Z")]
        public void WhenStartDateTimeIsCalled_LatestDateIsUsed_FromMeteringPointCreatedDateTimeAndSettingStartDateTime(
            string meteringPointDate,
            string startDate,
            string expectedStartDate)
        {
            // Arrange
            var meteringPointCreatedDateTime = InstantPattern.General.Parse(meteringPointDate).Value;
            var startDateTime = InstantPattern.General.Parse(startDate).Value;

            // Act
            var sut = new DefaultChargeLink(startDateTime, null, 0, MeteringPointType.Consumption);

            // Assert
            sut.GetStartDateTime(meteringPointCreatedDateTime).Should().BeEquivalentTo(InstantPattern.General.Parse(expectedStartDate).Value);
        }

        [Fact]
        public void ApplicableForLinking_WhenEndDateTimeIsNullAndMpTypeMatch_ReturnsTrue()
        {
            // Arrange
            var startDateTime = InstantPattern.General.Parse("2020-05-10T13:00:00Z").Value;
            var meteringPointCreatedDateTime = startDateTime;

            var sut = new DefaultChargeLink(startDateTime, null, 0, MeteringPointType.Consumption);

            // Act / Assert
            Assert.True(sut.ApplicableForLinking(meteringPointCreatedDateTime, MeteringPointType.Consumption));
        }

        [Fact]
        public void ApplicableForLinking_WhenEndDateTimeIsLaterThanStartDateTimeAndMpTypeMatch_ReturnsTrue()
        {
            // Arrange
            var startDateTime = InstantPattern.General.Parse("2020-05-10T13:00:00Z").Value;
            var endDateTime = InstantPattern.General.Parse("2020-05-11T13:00:00Z").Value;
            var meteringPointCreatedDateTime = startDateTime;

            var sut = new DefaultChargeLink(startDateTime, endDateTime, 0, MeteringPointType.Consumption);

            // Act / Assert
            Assert.True(sut.ApplicableForLinking(meteringPointCreatedDateTime, MeteringPointType.Consumption));
        }

        [Fact]
        public void ApplicableForLinking_WhenEndDateTimeEqualsStartDateTime_ReturnsFalse()
        {
            // Arrange
            var startDateTime = InstantPattern.General.Parse("2020-05-10T13:00:00Z").Value;
            var endDateTime = startDateTime;
            var meteringPointCreatedDateTime = startDateTime;

            var sut = new DefaultChargeLink(startDateTime, endDateTime, 0, MeteringPointType.Consumption);

            // Act / Assert
            Assert.False(sut.ApplicableForLinking(meteringPointCreatedDateTime, MeteringPointType.Consumption));
        }

        [Fact]
        public void ApplicableForLinking_WhenMpTypesDoNotMatch_ReturnsFalse()
        {
            // Arrange
            var startDateTime = InstantPattern.General.Parse("2020-05-10T13:00:00Z").Value;
            var meteringPointCreatedDateTime = startDateTime;

            var sut = new DefaultChargeLink(startDateTime, null, 0, MeteringPointType.Consumption);

            // Assert
            Assert.False(sut.ApplicableForLinking(meteringPointCreatedDateTime, MeteringPointType.Production));
        }
    }
}
