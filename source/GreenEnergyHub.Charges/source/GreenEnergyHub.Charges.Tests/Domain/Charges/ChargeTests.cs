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
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using NodaTime;
using Xunit;
using Xunit.Categories;
using Charge = GreenEnergyHub.Charges.Domain.Charges.Charge;
using Period = GreenEnergyHub.Charges.Domain.Charges.Period;
using PeriodBuilder = GreenEnergyHub.Charges.Tests.Builders.Command.PeriodBuilder;

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
            var newPeriodEndDate = Instant.FromUtc(9999, 12, 31, 23, 59, 59);
            var newPeriod = new PeriodBuilder()
                .WithName("newPeriod")
                .WithStartDateTime(newPeriodStartDate)
                .WithEndDateTime(newPeriodEndDate)
                .Build();

            var sut = new ChargeBuilder().WithPeriods(CreatePeriods()).Build();

            // Act
            sut.UpdateCharge(newPeriod);

            // Assert
            var newTimeline = sut.Periods.OrderBy(p => p.StartDateTime).ToList();
            Assert.True(newTimeline.Count == 2);
            Assert.Equal("periodOne", newTimeline[0].Name);
            Assert.Equal(newPeriodStartDate, newTimeline[0].EndDateTime);
            Assert.Equal("newPeriod", newTimeline[1].Name);
            Assert.Equal(newPeriodEndDate, newTimeline[1].EndDateTime);
        }

        // [Fact]
        // public Task UpdateCharge_WhenNoOverlappingPeriodExist_NewPeriodInserted()
        // {
        //     // Arrange
        //     // Act
        //     // Assert
        // }
        private List<Period> CreatePeriods()
        {
            return new List<Period>
            {
                new PeriodBuilder()
                    .WithName("periodOne")
                    .WithStartDateTime(Instant.FromUtc(2021, 8, 1, 22, 0, 0))
                    .WithEndDateTime(Instant.FromUtc(2021, 8, 10, 22, 0, 0))
                    .Build(),
                new PeriodBuilder()
                    .WithName("periodTwo")
                    .WithStartDateTime(Instant.FromUtc(2021, 8, 10, 22, 0, 0))
                    .WithEndDateTime(Instant.FromUtc(2021, 8, 15, 22, 0, 0))
                    .Build(),
                new PeriodBuilder()
                    .WithName("periodThree")
                    .WithStartDateTime(Instant.FromUtc(2021, 8, 15, 22, 0, 0))
                    .WithEndDateTime(Instant.FromUtc(2021, 8, 30, 22, 0, 0))
                    .Build(),
            };
        }
    }
}
