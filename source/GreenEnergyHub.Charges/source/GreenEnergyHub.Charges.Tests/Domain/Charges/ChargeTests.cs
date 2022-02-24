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
using System.Runtime.InteropServices;
using FluentAssertions;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using NodaTime;
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
            actualTimeline.Count.Should().Be(2);
            actualTimeline[0].Name.Should().Be("ExistingPeriod");
            actualTimeline[0].StartDateTime.Should().Be(InstantHelper.GetTodayAtMidnightUtc());
            actualTimeline[0].EndDateTime.Should().Be(InstantHelper.GetTomorrowAtMidnightUtc());
            actualTimeline[1].Name.Should().Be("NewPeriod");
            actualTimeline[1].StartDateTime.Should().Be(InstantHelper.GetTomorrowAtMidnightUtc());
            actualTimeline[1].EndDateTime.Should().Be(InstantHelper.GetEndDefault());
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
            actualTimeline.Count.Should().Be(2);
            actualTimeline[0].Name.Should().Be("FirstPeriod");
            actualTimeline[0].StartDateTime.Should().Be(InstantHelper.GetYesterdayAtMidnightUtc());
            actualTimeline[0].EndDateTime.Should().Be(InstantHelper.GetTodayAtMidnightUtc());
            actualTimeline[1].Name.Should().Be("NewPeriod");
            actualTimeline[1].StartDateTime.Should().Be(InstantHelper.GetTodayAtMidnightUtc());
            actualTimeline[1].EndDateTime.Should().Be(InstantHelper.GetEndDefault());
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
            actualTimeline.Count.Should().Be(2);
            actualTimeline[0].Name.Should().Be("FirstPeriod");
            actualTimeline[0].StartDateTime.Should().Be(InstantHelper.GetYesterdayAtMidnightUtc());
            actualTimeline[0].EndDateTime.Should().Be(InstantHelper.GetTomorrowAtMidnightUtc());
            actualTimeline[1].Name.Should().Be("NewPeriod");
            actualTimeline[1].StartDateTime.Should().Be(InstantHelper.GetTomorrowAtMidnightUtc());
            actualTimeline[1].EndDateTime.Should().Be(InstantHelper.GetEndDefault());
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
            actualTimeline.Count.Should().Be(1);
            actualTimeline[0].Name.Should().Be("NewPeriod");
            actualTimeline[0].StartDateTime.Should().Be(InstantHelper.GetYesterdayAtMidnightUtc());
            actualTimeline[0].EndDateTime.Should().Be(InstantHelper.GetEndDefault());
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
