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
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Microsoft.Identity.Client.Extensions.Msal;
using NodaTime;
using NuGet.Frameworks;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Charges
{
    [UnitTest]
    public class ChargeTests
    {
        [Fact]
        public void UpdateCharge_NewPeriodInsideSingleExistingPeriod_SetsNewEndDateForExistingPeriodAndInsertsNewPeriod()
        {
            // Arrange
            var existingPeriod = new ChargePeriodBuilder()
                .WithName("ExistingPeriod")
                .WithStartDateTime(InstantHelper.GetTodayAtMidnightUtc())
                .Build();

            var sut = new ChargeBuilder()
                .WithPeriods(new List<ChargePeriod> { existingPeriod })
                .Build();

            var newPeriod = new ChargePeriodBuilder()
                .WithName("NewPeriod")
                .WithStartDateTime(InstantHelper.GetTomorrowAtMidnightUtc())
                .Build();

            // Act
            sut.UpdateCharge(newPeriod);

            // Assert
            var actualTimeline = sut.Periods.OrderBy(p => p.StartDateTime).ToList();
            var actualFirstPeriod = actualTimeline[0];
            var actualSecondPeriod = actualTimeline[1];

            actualTimeline.Count.Should().Be(2);
            actualFirstPeriod.Name.Should().Be("ExistingPeriod");
            actualFirstPeriod.StartDateTime.Should().Be(InstantHelper.GetTodayAtMidnightUtc());
            actualFirstPeriod.EndDateTime.Should().Be(InstantHelper.GetTomorrowAtMidnightUtc());
            actualSecondPeriod.Name.Should().Be("NewPeriod");
            actualSecondPeriod.StartDateTime.Should().Be(InstantHelper.GetTomorrowAtMidnightUtc());
            actualSecondPeriod.EndDateTime.Should().Be(InstantHelper.GetEndDefault());
        }

        [Fact]
        public void UpdateCharge_NewPeriodStartsInsideFirstOfThreeExistingPeriods_SetsNewEndDateTimeForFirstExistingPeriod_AndRemovesSecondAndThird_AndInsertsNewPeriod()
        {
            // Arrange
            var sut = new ChargeBuilder()
                .WithPeriods(CreateThreeExistingPeriods())
                .Build();

            var newPeriod = new ChargePeriodBuilder()
                .WithName("NewPeriod")
                .WithStartDateTime(InstantHelper.GetTodayAtMidnightUtc())
                .Build();

            // Act
            sut.UpdateCharge(newPeriod);

            // Assert
            var actualTimeline = sut.Periods.OrderBy(p => p.StartDateTime).ToList();
            var actualFirstPeriod = actualTimeline[0];
            var actualSecondPeriod = actualTimeline[1];

            actualTimeline.Count.Should().Be(2);
            actualFirstPeriod.Name.Should().Be("FirstPeriod");
            actualFirstPeriod.StartDateTime.Should().Be(InstantHelper.GetYesterdayAtMidnightUtc());
            actualFirstPeriod.EndDateTime.Should().Be(InstantHelper.GetTodayAtMidnightUtc());
            actualSecondPeriod.Name.Should().Be("NewPeriod");
            actualSecondPeriod.StartDateTime.Should().Be(InstantHelper.GetTodayAtMidnightUtc());
            actualSecondPeriod.EndDateTime.Should().Be(InstantHelper.GetEndDefault());
        }

        [Fact]
        public void UpdateCharge_NewPeriodStartEqualsSecondOfThreeExistingPeriods_NewPeriodOverwritesSecondAndThird()
        {
            // Arrange
            var sut = new ChargeBuilder()
                .WithPeriods(CreateThreeExistingPeriods())
                .Build();

            var newPeriod = new ChargePeriodBuilder()
                .WithName("NewPeriod")
                .WithStartDateTime(InstantHelper.GetTomorrowAtMidnightUtc())
                .Build();

            // Act
            sut.UpdateCharge(newPeriod);

            // Assert
            var actualTimeline = sut.Periods.OrderBy(p => p.StartDateTime).ToList();
            var actualFirstPeriod = actualTimeline[0];
            var actualSecondPeriod = actualTimeline[1];

            actualTimeline.Count.Should().Be(2);
            actualFirstPeriod.Name.Should().Be("FirstPeriod");
            actualFirstPeriod.StartDateTime.Should().Be(InstantHelper.GetYesterdayAtMidnightUtc());
            actualFirstPeriod.EndDateTime.Should().Be(InstantHelper.GetTomorrowAtMidnightUtc());
            actualSecondPeriod.Name.Should().Be("NewPeriod");
            actualSecondPeriod.StartDateTime.Should().Be(InstantHelper.GetTomorrowAtMidnightUtc());
            actualSecondPeriod.EndDateTime.Should().Be(InstantHelper.GetEndDefault());
        }

        [Fact]
        public void UpdateCharge_NewPeriodStartsBeforeExistingPeriod_NewPeriodOverwritesExisting()
        {
            // Arrange
            var existingPeriod = new ChargePeriodBuilder()
                .WithName("ExistingPeriod")
                .WithStartDateTime(InstantHelper.GetTodayAtMidnightUtc())
                .Build();

            var sut = new ChargeBuilder()
                .WithPeriods(new List<ChargePeriod> { existingPeriod })
                .Build();

            var newPeriod = new ChargePeriodBuilder()
                .WithName("NewPeriod")
                .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                .Build();

            // Act
            sut.UpdateCharge(newPeriod);

            // Assert
            var actualTimeline = sut.Periods.OrderBy(p => p.StartDateTime).ToList();
            var actualFirstPeriod = actualTimeline[0];

            actualTimeline.Count.Should().Be(1);
            actualFirstPeriod.Name.Should().Be("NewPeriod");
            actualFirstPeriod.StartDateTime.Should().Be(InstantHelper.GetYesterdayAtMidnightUtc());
            actualFirstPeriod.EndDateTime.Should().Be(InstantHelper.GetEndDefault());
        }

        [Theory]
        [InlineAutoMoqData]
        public void UpdateCharge_WhenChargePeriodIsNull_ThrowsArgumentNullException(Charge sut)
        {
            ChargePeriod? chargePeriod = null;

            Assert.Throws<ArgumentNullException>(() => sut.UpdateCharge(chargePeriod!));
        }

        [Fact]
        public void Validate_WhenGapsInChargePeriodTimeline_ThrowsInvalidOperationException()
        {
            // Arrange
            var firstChargePeriod = new ChargePeriodBuilder()
                .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                .WithEndDateTime(InstantHelper.GetTodayAtMidnightUtc())
                .Build();

            var secondChargePeriod = new ChargePeriodBuilder()
                .WithStartDateTime(InstantHelper.GetTomorrowAtMidnightUtc())
                .WithEndDateTime(InstantHelper.GetEndDefault())
                .Build();

            var sut = new ChargeBuilder()
                .WithPeriods(new List<ChargePeriod> { firstChargePeriod })
                .Build();

            // Act / Assert
            Assert.Throws<InvalidOperationException>(() => sut.UpdateCharge(secondChargePeriod));
        }

        [Fact]
        public void StopCharge_WhenSingleExistingChargePeriod_SetNewEndDate()
        {
            // Arrange
            var today = InstantHelper.GetTodayAtMidnightUtc();
            var dayAfterTomorrow = InstantHelper.GetTodayPlusDaysAtMidnightUtc(2);
            var existingPeriod = new ChargePeriodBuilder()
                .WithStartDateTime(today)
                .WithEndDateTime(InstantHelper.GetEndDefault())
                .Build();

            var sut = new ChargeBuilder().WithPeriods(new List<ChargePeriod> { existingPeriod }).Build();

            // Act
            sut.StopCharge(dayAfterTomorrow);

            // Assert
            var actual = sut.Periods.OrderByDescending(p => p.StartDateTime).First();
            actual.EndDateTime.Should().BeEquivalentTo(dayAfterTomorrow);
        }

        [Fact]
        public void StopCharge_WhenThreeExistingPeriods_ThenRemoveAllAfterStop()
        {
            // Arrange
            var sut = new ChargeBuilder().WithPeriods(CreateThreeExistingPeriods()).Build();
            var stopDate = InstantHelper.GetTomorrowAtMidnightUtc();

            // Act
            sut.StopCharge(stopDate);

            // Assert
            sut.Periods.Count.Should().Be(1);
        }

        private static List<ChargePeriod> CreateThreeExistingPeriods()
        {
            return new List<ChargePeriod>
            {
                new ChargePeriodBuilder()
                    .WithName("FirstPeriod")
                    .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                    .WithEndDateTime(InstantHelper.GetTomorrowAtMidnightUtc())
                    .Build(),
                new ChargePeriodBuilder()
                    .WithName("SecondPeriod")
                    .WithStartDateTime(InstantHelper.GetTomorrowAtMidnightUtc())
                    .WithEndDateTime(InstantHelper.GetTodayPlusDaysAtMidnightUtc(2))
                    .Build(),
                new ChargePeriodBuilder()
                    .WithName("ThirdPeriod")
                    .WithStartDateTime(InstantHelper.GetTodayPlusDaysAtMidnightUtc(2))
                    .WithEndDateTime(InstantHelper.GetEndDefault())
                    .Build(),
            };
        }
    }
}
