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
        private readonly ChargePeriodBuilder _chargePeriodBuilder;

        public ChargeTests()
        {
            _chargePeriodBuilder = new ChargePeriodBuilder();
        }

        [Fact]
        public void UpdateCharge_NewPeriodInsideSingleExistingPeriod_SetsNewEndDateForExistingPeriodAndInsertsNewPeriod() // Update scenario 1
        {
            // Arrange
            var existingPeriod = _chargePeriodBuilder
                .WithName("ExistingPeriod")
                .WithStartDateTime(InstantHelper.GetTodayAtMidnightUtc())
                .Build();

            var sut = new ChargeBuilder().WithPeriods(new List<ChargePeriod> { existingPeriod }).Build();

            var newPeriod = _chargePeriodBuilder
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

        // public void UpdateCharge_WhenOverlappingPeriodExist_OverlappingEndDateIsSetAndNewPeriodInserted() - is new test method name better?
        public void UpdateCharge_NewPeriodStartsInsideFirstOfThreeExistingPeriods_SetsNewEndDateTimeForFirstExistingPeriod_AndRemovesSecondAndThird_AndInsertsNewPeriod() // Update scenario 2
        {
            // Arrange
            var newPeriodStartDate = Instant.FromUtc(2021, 8, 8, 22, 0, 0);
            var newPeriodEndDate = InstantExtensions.GetEndDefault();
            var newPeriod = _chargePeriodBuilder
                .WithName("newPeriod")
                .WithStartDateTime(newPeriodStartDate)
                .WithEndDateTime(newPeriodEndDate)
                .Build();

            var sut = new ChargeBuilder().WithPeriods(CreatePeriods()).Build();

            // Act
            sut.UpdateCharge(newPeriod);

            // Assert
            var newTimeline = sut.Periods.OrderBy(p => p.StartDateTime).ToList();
            newTimeline.Count.Should().Be(2);
            newTimeline[0].Name.Should().Be("periodOne");
            newTimeline[0].EndDateTime.Should().Be(newPeriodStartDate);
            newTimeline[1].Name.Should().Be("newPeriod");
            newTimeline[1].EndDateTime.Should().Be(newPeriodEndDate);
        }

        private static List<ChargePeriod> CreatePeriods()
        {
            return new List<ChargePeriod>
            {
                new ChargePeriodBuilder()
                    .WithName("periodOne")
                    .WithStartDateTime(Instant.FromUtc(2021, 8, 1, 22, 0, 0))
                    .WithEndDateTime(Instant.FromUtc(2021, 8, 10, 22, 0, 0))
                    .Build(),
                new ChargePeriodBuilder()
                    .WithName("periodTwo")
                    .WithStartDateTime(Instant.FromUtc(2021, 8, 10, 22, 0, 0))
                    .WithEndDateTime(Instant.FromUtc(2021, 8, 15, 22, 0, 0))
                    .Build(),
                new ChargePeriodBuilder()
                    .WithName("periodThree")
                    .WithStartDateTime(Instant.FromUtc(2021, 8, 15, 22, 0, 0))
                    .WithEndDateTime(Instant.FromUtc(2021, 8, 30, 22, 0, 0))
                    .Build(),
            };
        }
    }
}
