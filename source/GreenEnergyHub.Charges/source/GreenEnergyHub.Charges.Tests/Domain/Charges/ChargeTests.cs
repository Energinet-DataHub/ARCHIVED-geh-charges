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
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Domain.Charges
{
    [UnitTest]
    public class ChargeTests
    {
        /*[Fact]
        public void UpdateCharge_NewPeriodInsideSingleExistingPeriod_InsertsNewPeriod()
        {
            // Arrange
            var existingCharge = new ChargeBuilder()
                .WithName("ExistingPeriod")
                .WithStartDateTime(InstantHelper.GetTodayAtMidnightUtc())
                .Build();
            var existingCharges = new List<Charge> { existingCharge };

            /*var sut = new ChargeBuilder()
                .WithPeriods(new List<ChargePeriod> { existingPeriod })
                .Build();#1#

            var sut = new ChargeBuilder()
                .WithName("NewPeriod")
                .WithStartDateTime(InstantHelper.GetTomorrowAtMidnightUtc())
                .Build();

            // Act
            sut.Update(sut);
            existingCharges.Add(sut);

            // Assert TODO
            existingCharges.Count.Should().Be(2);
            /*var actualTimeline = sut.Periods.OrderBy(p => p.StartDateTime).ToList();
            var actualFirstPeriod = actualTimeline[0];
            var actualSecondPeriod = actualTimeline[1];

            actualTimeline.Count.Should().Be(2);
            actualFirstPeriod.Name.Should().Be("ExistingPeriod");
            actualFirstPeriod.StartDateTime.Should().Be(InstantHelper.GetTodayAtMidnightUtc());
            // actualFirstPeriod.EndDateTime.Should().Be(InstantHelper.GetTomorrowAtMidnightUtc());
            actualSecondPeriod.Name.Should().Be("NewPeriod");
            actualSecondPeriod.StartDateTime.Should().Be(InstantHelper.GetTomorrowAtMidnightUtc());
            // actualSecondPeriod.EndDateTime.Should().Be(InstantHelper.GetEndDefault());#1#
        }*/

        /*[Fact]
        public void UpdateCharge_NewPeriodStartsInsideFirstOfThreeExistingPeriods_InsertsNewPeriod()
        {
            // Arrange
            var existingCharges = CreateThreeExistingCharges();
            /*var sut = new ChargeBuilder()
                .WithPeriods(CreateThreeExistingPeriods())
                .Build();#1#

            var todayAtMidnightUtc = InstantHelper.GetTodayAtMidnightUtc();
            var sut = new ChargeBuilder()
                .WithName("NewPeriod")
                .WithStartDateTime(todayAtMidnightUtc)
                .Build();

            // Act
            sut.Update(sut);
            existingCharges.Add(sut);

            // Assert TODO
            existingCharges.Count.Should().Be(4);
            /*var actualTimeline = existingPeriods.Periods.OrderBy(p => p.StartDateTime).ToList();
            var actualFirstPeriod = actualTimeline[0];
            // var actualSecondPeriod = actualTimeline[1];
            actualTimeline.Count.Should().Be(4);
            actualFirstPeriod.Name.Should().Be("FirstPeriod");
            actualFirstPeriod.StartDateTime.Should().Be(InstantHelper.GetYesterdayAtMidnightUtc());
            // actualFirstPeriod.EndDateTime.Should().Be(InstantHelper.GetTodayAtMidnightUtc());
            existingPeriods.Periods.GetValidChargePeriodAsOf(todayAtMidnightUtc)?.Name.Should().Be("NewPeriod");
            existingPeriods.Periods.GetValidChargePeriodAsOf(InstantHelper.GetTomorrowAtMidnightUtc())?.Name.Should().Be("NewPeriod");
            existingPeriods.Periods.GetValidChargePeriodAsOf(InstantHelper.GetTodayPlusDaysAtMidnightUtc(2))?.Name.Should().Be("NewPeriod");
            /*actualSecondPeriod.Name.Should().Be("NewPeriod");
            actualSecondPeriod.StartDateTime.Should().Be(todayAtMidnightUtc);#2#
            // actualSecondPeriod.EndDateTime.Should().Be(InstantHelper.GetEndDefault());#1#
        }*/

        /*[Fact]
        public void UpdateCharge_NewPeriodStartEqualsSecondOfThreeExistingPeriods_NewPeriodPrecedesSecondAndThird()
        {
            // Arrange
            var existingCharges = CreateThreeExistingCharges();
            /*var sut = new ChargeBuilder()
                .WithPeriods(CreateThreeExistingCharges())
                .Build();#1#

            var tomorrowAtMidnightUtc = InstantHelper.GetTomorrowAtMidnightUtc();
            var sut = new ChargeBuilder()
                .WithName("NewPeriod")
                .WithStartDateTime(tomorrowAtMidnightUtc)
                .Build();

            // Act
            sut.Update(sut);
            existingCharges.Add(sut);

            // Assert TODO
            existingCharges.Count.Should().Be(4);
            /*var actualTimeline = existingCharges.Periods.OrderBy(p => p.StartDateTime).ToList();
            actualTimeline.Count.Should().Be(4);

            var actualFirstPeriod = actualTimeline[0];
            /*var actualSecondPeriod = actualTimeline[1];#2#

            actualFirstPeriod.Name.Should().Be("FirstPeriod");
            actualFirstPeriod.StartDateTime.Should().Be(InstantHelper.GetYesterdayAtMidnightUtc());
            // actualFirstPeriod.EndDateTime.Should().Be(InstantHelper.GetTomorrowAtMidnightUtc());
            /*actualSecondPeriod.Name.Should().Be("NewPeriod");
            actualSecondPeriod.StartDateTime.Should().Be(InstantHelper.GetTomorrowAtMidnightUtc());
            // actualSecondPeriod.EndDateTime.Should().Be(InstantHelper.GetEndDefault());#2#
            existingCharges.Periods.GetValidChargePeriodAsOf(tomorrowAtMidnightUtc)?.Name.Should().Be("NewPeriod");#1#
        }*/

        /*[Fact]
        public void UpdateCharge_NewPeriodStartsBeforeExistingPeriod_NewPeriodPrecedesExisting()
        {
            // Arrange
            var existingCharge = new ChargeBuilder()
                .WithName("ExistingPeriod")
                .WithStartDateTime(InstantHelper.GetTodayAtMidnightUtc())
                .Build();

            /*var sut = new ChargeBuilder().WithPeriods(new List<ChargePeriod> { existingCharge }).Build();#1#

            var existingCharges = new List<Charge> { existingCharge };

            var sut = new ChargeBuilder()
                .WithName("NewPeriod")
                .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                .Build();

            // Act
            sut.Update(sut);
            existingCharges.Add(sut);

            // Assert TODO
            existingCharges.Count.Should().Be(2);
            /*var actualTimeline = sut.Periods.OrderBy(p => p.StartDateTime).ToList();
            var actualFirstPeriod = actualTimeline[0];

            actualTimeline.Count.Should().Be(2);
            actualFirstPeriod.Name.Should().Be("NewPeriod");
            actualFirstPeriod.StartDateTime.Should().Be(InstantHelper.GetYesterdayAtMidnightUtc());
            // actualFirstPeriod.EndDateTime.Should().Be(InstantHelper.GetEndDefault());
            sut.Periods.GetValidChargePeriodAsOf(InstantHelper.GetTodayAtMidnightUtc())?.Name.Should().Be("NewPeriod");
            sut.Periods.GetValidChargePeriodAsOf(InstantHelper.GetYesterdayAtMidnightUtc())?.Name.Should().Be("NewPeriod");#1#
        }*/

        /*[Theory]
        [InlineAutoMoqData]
        public void UpdateCharge_WhenChargePeriodIsNull_ThrowsArgumentNullException(Charge sut)
        {
            Charge? charge = null;

            Assert.Throws<ArgumentNullException>(() => sut.Update(charge!));
        }*/

        /*[Fact]
        public void UpdateCharge_WhenEndDateIsBound_ThenThrowException()
        {
            // Arrange
            var existingPeriod = new ChargeBuilder()
                .WithName("ExistingPeriod")
                .WithStartDateTime(InstantHelper.GetTodayAtMidnightUtc())
                .Build();

            var sut = new ChargeBuilder()
                .WithPeriods(new List<ChargePeriod> { existingPeriod })
                .Build();

            var newPeriod = new ChargeBuilder()
                .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                // .WithEndDateTime(InstantHelper.GetTomorrowAtMidnightUtc())
                .WithIsStop(InstantHelper.GetTomorrowAtMidnightUtc())
                .Build();

            // Act
            Assert.Throws<InvalidOperationException>(() => sut.Update(newPeriod));
        }*/

        /*[Fact]
        public void UpdateCharge_NewPeriodStartsBeforeExistingStopDate_SetsNewEndDateForExistingPeriodAndInsertsNewPeriod()
        {
            // Arrange
            var existingCharges = BuildStoppedChargePeriods().ToList();
            /*var sut = new ChargeBuilder().WithPeriods(BuildStoppedChargePeriods()).Build();#1#
            var sut = new ChargeBuilder()
                .WithName("New")
                .WithStartDateTime(InstantHelper.GetTodayPlusDaysAtMidnightUtc(3))
                // .WithEndDateTime(InstantHelper.GetEndDefault())
                .Build();

            // Act
            sut.Update(sut);
            existingCharges.Add(sut);

            // Assert TODO
            existingCharges.Count.Should().Be(3);
            /*var actualTimeline = sut.Periods.OrderBy(p => p.StartDateTime).ToList();
            actualTimeline.Count.Should().Be(3);
            var actualFirstPeriod = actualTimeline[0];
            var actualSecondPeriod = actualTimeline[1];
            var actualThirdPeriod = actualTimeline[2];
            actualFirstPeriod.Name.Should().Be("First");
            actualFirstPeriod.StartDateTime.Should().Be(InstantHelper.GetTodayAtMidnightUtc());
            // actualFirstPeriod.EndDateTime.Should().Be(InstantHelper.GetTodayPlusDaysAtMidnightUtc(2));
            actualSecondPeriod.Name.Should().Be("Second");
            actualSecondPeriod.StartDateTime.Should().Be(InstantHelper.GetTodayPlusDaysAtMidnightUtc(2));
            // actualSecondPeriod.EndDateTime.Should().Be(InstantHelper.GetTodayPlusDaysAtMidnightUtc(3));
            actualThirdPeriod.Name.Should().Be("New");
            actualThirdPeriod.StartDateTime.Should().Be(InstantHelper.GetTodayPlusDaysAtMidnightUtc(3));
            // actualThirdPeriod.EndDateTime.Should().Be(InstantHelper.GetTodayPlusDaysAtMidnightUtc(4));#1#
        }*/

        /*[Fact]
        public void UpdateCharge_NewPeriodStartsBeforeExistingStopDate_PreceedesSubsequentsPeriodAndInsertsNewPeriod()
        {
            // Arrange
            var existingCharges = BuildStoppedChargePeriods().ToList();
            /*var sut = new ChargeBuilder().WithPeriods(BuildStoppedChargePeriods()).Build();#1#
            var sut = new ChargeBuilder()
                .WithName("NewPeriod")
                .WithStartDateTime(InstantHelper.GetTodayAtMidnightUtc())
                // .WithEndDateTime(InstantHelper.GetEndDefault())
                .Build();

            // Act
            sut.Update(sut);
            existingCharges.Add(sut);

            // Assert TODO
            existingCharges.Count.Should().Be(3);
            /*var actualTimeline = sut.Periods.OrderedByReceivedDateTimeAndOrder().ToList();
            actualTimeline.Count.Should().Be(3);
            var actualFirstPeriod = sut.Periods.GetValidChargePeriodAsOf(InstantHelper.GetTodayAtMidnightUtc());
            actualFirstPeriod?.Name.Should().Be("NewPeriod");
            actualFirstPeriod?.StartDateTime.Should().Be(InstantHelper.GetTodayAtMidnightUtc());
            // actualFirstPeriod.EndDateTime.Should().Be(InstantHelper.GetTodayPlusDaysAtMidnightUtc(4));#1#
        }*/

        /*[Fact]
        public void Stop_WhenSingleExistingChargePeriod_SetIsStopTrue()
        {
            // Arrange
            var today = InstantHelper.GetTodayAtMidnightUtc();
            var dayAfterTomorrow = InstantHelper.GetTodayPlusDaysAtMidnightUtc(2);
            var sut = new ChargeBuilder()
                .WithStartDateTime(dayAfterTomorrow)
                .WithIsStop(true)
                .Build();
            var existingPeriod = new ChargeBuilder()
                .WithStartDateTime(today)
                // .WithEndDateTime(InstantHelper.GetEndDefault())
                .Build();

            var existingCharges = new List<Charge> { existingPeriod };
            /*var sut = new ChargeBuilder().WithPeriods(new List<ChargePeriod> { existingPeriod }).Build();#1#

            // Act
            sut.Stop(sut);

            // Assert TODO
            existingCharges.Count.Should().Be(2);
            /*var actual = sut.Periods.OrderByDescending(p => p.StartDateTime).First();
            // actual.EndDateTime.Should().BeEquivalentTo(dayAfterTomorrow);
            actual.IsStop.Should().BeTrue();#1#
        }*/

        /*[Fact]
        public void Stop_WhenThreeExistingPeriods_StopPrecedesOtherPeriods()
        {
            // Arrange
            var existingCharges = CreateThreeExistingCharges();
            /*var sut = new ChargeBuilder().WithPeriods(CreateThreeExistingCharges()).Build();#1#
            var stopDate = InstantHelper.GetTomorrowAtMidnightUtc();
            var sut = new ChargeBuilder()
                .WithStartDateTime(stopDate)
                .WithIsStop(true)
                .Build();

            // Act
            sut.Stop(sut);
            existingCharges.Add(sut);

            // Assert TODO
            existingCharges.Count.Should().Be(4);
            /*sut.Periods.Count.Should().Be(4);
            sut.Periods.OrderedByReceivedDateTimeAndOrder().First().StartDateTime.Should().Be(stopDate);#1#
        }*/

        /*[Fact]
        public void Stop_WhenNoPeriodsExist_ThenThrowException()
        {
            // Arrange
            var sut = new ChargeBuilder().Build();
            var stopChargePeriod = new ChargeBuilder().WithIsStop(true).Build();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => sut.Stop(stopChargePeriod));
        }*/

        /*[Fact]
        public void Stop_WhenStopExistAfterStopDate_ThenNewStopReplacesOldStop()
        {
            // Arrange
            var charge = new ChargeBuilder()
                .WithStartDateTime(InstantHelper.GetTodayAtMidnightUtc())
                .Build();
            var existingCharges = new List<Charge> { charge };
            /*var sut = new ChargeBuilder().WithPeriods(charges).Build();#1#
            var stopDate = InstantHelper.GetTomorrowAtMidnightUtc();
            var sut = new ChargeBuilder()
                .WithStartDateTime(stopDate)
                .WithIsStop(true)
                .Build();

            // Act
            sut.Stop(sut);
            existingCharges.Add(sut);

            // Assert TODO
            existingCharges.Count.Should().Be(2);
            /*var actual = sut.Periods.OrderByDescending(p => p.StartDateTime).First();
            // actual.EndDateTime.Should().Be(stopDate);
            actual.IsStop.Should().Be(true);#1#
        }*/

        /*[Fact]
        public void Stop_WhenStopExistBeforeStopDate_ThenThrowException()
        {
            // Arrange
            var charge = new ChargeBuilder()
                .WithStartDateTime(InstantHelper.GetTodayAtMidnightUtc())
                .WithIsStop(true)
                .Build();
            var existingCharges = new List<Charge> { charge };
            /*var sut = new ChargeBuilder().WithPeriods(charges).Build();#1#
            var stopDate = InstantHelper.GetTodayPlusDaysAtMidnightUtc(10);
            var sut = new ChargeBuilder()
                .WithStartDateTime(stopDate)
                .WithIsStop(true)
                .Build();

            // Act & Assert
            existingCharges.Count.Should().Be(1);
            Assert.Throws<InvalidOperationException>(() => sut.Stop(sut));
        }*/

        /*[Fact]
        public void Stop_WhenChargeStartDateAfterStopDate_ThenThrowException()
        {
            // Arrange
            var charge = new ChargeBuilder()
                .WithStartDateTime(InstantHelper.GetTodayPlusDaysAtMidnightUtc(5))
                .Build();
            var existingCharges = new List<Charge>() { charge };
            /*var sut = new ChargeBuilder().WithPeriods(charges).Build();#1#
            var stopDate = InstantHelper.GetTodayPlusDaysAtMidnightUtc(2);
            var sut = new ChargeBuilder()
                .WithStartDateTime(stopDate)
                .WithIsStop(true)
                .Build();

            // Act & Assert
            existingCharges.Count.Should().Be(1);
            Assert.Throws<InvalidOperationException>(() => sut.Stop(sut));
        }*/

        /*[Fact]
        public void Stop_WhenNewEndDateIsNot_ThenThrowException()
        {
            // Arrange
            var existingCharge = new ChargeBuilder().Build();
            var existingCharges = new List<Charge> { existingCharge };
            Charge sut = null!;

            // Act & Assert TODO
            existingCharges.Count.Should().Be(1);
            Assert.Throws<ArgumentNullException>(() => sut.Stop(sut));
        }*/

        /*[Fact]
        public void StopCharge_WhenPointsExistAfterStopDate_PointsRemoved()
        {
            // Arrange
            var points = new List<Point>
            {
                new(0, decimal.One, InstantHelper.GetTodayPlusDaysAtMidnightUtc(0)),
                new(0, decimal.One, InstantHelper.GetTodayPlusDaysAtMidnightUtc(1)),
                new(0, decimal.One, InstantHelper.GetTodayPlusDaysAtMidnightUtc(2)),
                new(0, decimal.One, InstantHelper.GetTodayPlusDaysAtMidnightUtc(3)),
                new(0, decimal.One, InstantHelper.GetTodayPlusDaysAtMidnightUtc(4)),
            };
            var existingCharge = new ChargeBuilder()
                /*.WithPeriods(new List<ChargePeriod> { new ChargeBuilder().Build() })#1#
                .WithPoints(points).Build();
            var existingCharges = new List<Charge> { existingCharge };

            var sut = new ChargeBuilder()
                .WithStartDateTime(InstantHelper.GetTodayPlusDaysAtMidnightUtc(2))
                .WithIsStop(true)
                .Build();

            // Act
            sut.Stop(sut);
            existingCharges.Add(sut);

            // Assert
            existingCharges.Count.Should().Be(2);
            sut.Points.Count.Should().Be(2);
        }*/

        /*[Fact]
        public void CancelStop_WhenStopPeriodExists_ThenAddNewLastPeriod()
        {
            // Arrange
            var stopDate = InstantHelper.GetTomorrowAtMidnightUtc();
            var sut = new ChargeBuilder()
                .WithName("newName")
                .WithStartDateTime(stopDate)
                .Build();

            var existingCharges = new List<Charge>
                {
                    new ChargeBuilder()
                        .WithName("oldName")
                        /*.WithEndDateTime(stopDate)#1#
                        .Build(),
                };

            // Act
            sut.CancelStop();

            // Assert TODO
            existingCharges.Count.Should().Be(1);
            /*var orderedPeriods = sut.Periods.OrderBy(p => p.StartDateTime).ToList();
            var actualFirst = orderedPeriods.First();
            var actualLast = orderedPeriods.Last();
            actualFirst.Name.Should().Be("oldName");
            actualFirst.StartDateTime.Should().BeEquivalentTo(InstantHelper.GetStartDefault());
            actualFirst.EndDateTime.Should().BeEquivalentTo(stopDate);
            actualLast.Name.Should().Be("newName");
            actualLast.StartDateTime.Should().BeEquivalentTo(stopDate);
            actualLast.EndDateTime.Should().BeEquivalentTo(InstantHelper.GetEndDefault());#1#
        }*/

        /*[Fact]
        public void CancelStop_WhenChargeNotStopped_ThenThrowException()
        {
            // Arrange
            var existingCharges = new List<Charge> { new ChargeBuilder().Build() };
            /*var sut = new ChargeBuilder().WithPeriods(charges).Build();#1#
            var cancelPeriod = new ChargeBuilder().WithStartDateTime(InstantHelper.GetTodayAtMidnightUtc()).Build();

            // Act & Assert TODO
            Assert.Throws<InvalidOperationException>(() => cancelPeriod.CancelStop());
            existingCharges.Count.Should().Be(1);
        }*/

        /*[Fact]
        public void CancelStop_WhenNoExistingPeriods_ThenThrowException()
        {
            // Arrange
            var existingCharge = new ChargeBuilder().Build();
            var sut = new ChargeBuilder().Build();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => sut.CancelStop());
        }*/

        private static IEnumerable<Charge> BuildStoppedChargePeriods()
        {
            return new List<Charge>
            {
                new ChargeBuilder()
                    .WithName("First")
                    .WithStartDateTime(InstantHelper.GetTodayAtMidnightUtc())
                    // .WithEndDateTime(InstantHelper.GetTodayPlusDaysAtMidnightUtc(2))
                    .Build(),
                new ChargeBuilder()
                    .WithName("Second")
                    .WithStartDateTime(InstantHelper.GetTodayPlusDaysAtMidnightUtc(2))
                    // .WithEndDateTime(InstantHelper.GetTodayPlusDaysAtMidnightUtc(4))
                    .WithIsStop(true)
                    .Build(),
            };
        }

        private static List<Charge> CreateThreeExistingCharges()
        {
            return new List<Charge>
            {
                new ChargeBuilder()
                    .WithName("FirstPeriod")
                    .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                    .Build(),
                new ChargeBuilder()
                    .WithName("SecondPeriod")
                    .WithStartDateTime(InstantHelper.GetTomorrowAtMidnightUtc())
                    .Build(),
                new ChargeBuilder()
                    .WithName("ThirdPeriod")
                    .WithStartDateTime(InstantHelper.GetTodayPlusDaysAtMidnightUtc(2))
                    .Build(),
            };
        }
    }
}
