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
using FluentAssertions;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Charges;
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
        public void UpdateCharge_WhenOverlappingPeriodExist_OverlappingEndDateIsSetAndNewPeriodInserted()
        {
            // Arrange
            var newPeriodStartDate = Instant.FromUtc(2021, 8, 8, 22, 0, 0);
            var newPeriodEndDate = InstantExtensions.GetEndDefault();
            var newPeriod = new ChargePeriodBuilder()
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

        // [Fact]
        // public Task UpdateCharge_WhenNoOverlappingPeriodExist_NewPeriodInserted()
        // {
        //     // Arrange
        //     // Act
        //     // Assert
        // }
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
