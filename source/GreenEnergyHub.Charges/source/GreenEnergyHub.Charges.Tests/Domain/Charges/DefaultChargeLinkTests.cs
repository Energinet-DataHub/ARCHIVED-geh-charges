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
using GreenEnergyHub.Charges.TestCore.Attributes;
using NodaTime;
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
            public void WhenStartDateTimeIsCalled_LatestDateIsUsed_FromMeteringPointCreatedDateTimeAndSettingStartDateTime2(
                string meteringPointDate,
                string settingStartDate,
                string expectedStartDate)
            {
                // Arrange
                var meteringPointCreatedDateTime = InstantPattern.General.Parse(meteringPointDate).Value;
                var settingStartDateTime = InstantPattern.General.Parse(settingStartDate).Value;

                // Act
                var sut = new DefaultChargeLink(meteringPointCreatedDateTime, settingStartDateTime, null, 0);

                // Assert
                sut.StartDateTime.Should().BeEquivalentTo(InstantPattern.General.Parse(expectedStartDate).Value);
            }

            [Fact]
            public void WhenStartDateTimeIsCalled_LatestDateIsUsed_FromMeteringPointCreatedDateTimeAndSettingStartDateTime()
            {
                // Arrange
                var meteringPointCreatedDateTime = SystemClock.Instance.GetCurrentInstant();
                var settingStartDateTime = SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromDays(1));

                // Act
                var sut = new DefaultChargeLink(meteringPointCreatedDateTime, settingStartDateTime, null, 0);

                // Assert
                sut.StartDateTime.Should().BeEquivalentTo(settingStartDateTime);
            }

            [Fact]
            public void WhenApplicableForLinkingIsCalled_ReturnsTrue_WhenEndDateTimeIsNull()
            {
                // Arrange
                var meteringPointCreatedDateTime = SystemClock.Instance.GetCurrentInstant();
                var settingStartDateTime = SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromDays(1));

                // Act
                var sut = new DefaultChargeLink(meteringPointCreatedDateTime, settingStartDateTime, null, 0);

                // Assert
                sut.ApplicableForLinking.Should().BeTrue();
            }

            [Fact]
            public void WhenApplicableForLinkingIsCalled_ReturnsTrue_WhenEndDateTimeIsAfterStartDateTime()
            {
                // Arrange
                var meteringPointCreatedDateTime = SystemClock.Instance.GetCurrentInstant();
                var settingStartDateTime = SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromDays(1));
                var endDateTime = SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromDays(2));

                // Act
                var sut = new DefaultChargeLink(meteringPointCreatedDateTime, settingStartDateTime, endDateTime, 0);

                // Assert
                sut.ApplicableForLinking.Should().BeTrue();
            }

            [Fact]
            public void WhenApplicableForLinkingIsCalled_ReturnsFalse_WhenEndDateTimeIsBeforeStartDateTime()
            {
                // Arrange
                var meteringPointCreatedDateTime = SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromDays(1));
                var settingStartDateTime = SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromDays(-1));
                var endDateTime = SystemClock.Instance.GetCurrentInstant();

                // Act
                var sut = new DefaultChargeLink(meteringPointCreatedDateTime, settingStartDateTime, endDateTime, 0);

                // Assert
                sut.ApplicableForLinking.Should().BeFalse();
            }

            [Fact]
            public void WhenApplicableForLinkingIsCalled_ReturnsFalse_WhenEndDateTimeIsEqualToStartDateTime()
            {
                // Arrange
                var meteringPointCreatedDateTime = SystemClock.Instance.GetCurrentInstant();
                var startDateTime = SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromDays(1));

                // Act
                var sut = new DefaultChargeLink(meteringPointCreatedDateTime, startDateTime, startDateTime, 0);

                // Assert
                sut.ApplicableForLinking.Should().BeFalse();
            }
        }
}
