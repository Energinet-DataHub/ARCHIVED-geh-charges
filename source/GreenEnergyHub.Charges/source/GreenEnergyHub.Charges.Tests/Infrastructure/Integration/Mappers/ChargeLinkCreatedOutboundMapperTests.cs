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
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Domain.Events.Integration;
using GreenEnergyHub.Charges.Infrastructure.Integration.ChargeLinkCreated;
using GreenEnergyHub.Charges.Infrastructure.Integration.Mappers;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.TestHelpers.FluentAssertionsExtensions;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Integration.Mappers
{
    [UnitTest]
    public class ChargeLinkCreatedOutboundMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalled_MapsToCorrectValues(
            [NotNull] ChargeLinkCreatedOutboundMapper sut)
        {
            // Arrange
            var createdEvent = GetCreatedEvent();

            // Act
            var result = (ChargeLinkCreatedContract)sut.Convert(createdEvent);

            // Assert
            result.Should().NotContainNullsOrEmptyEnumerables();
            result.ChargeLinkId.Should().BeEquivalentTo(createdEvent.ChargeLinkId);
            result.MeteringPointId.Should().BeEquivalentTo(createdEvent.MeteringPointId);
            result.ChargeId.Should().BeEquivalentTo(createdEvent.ChargeId);
            result.ChargeType.Should().BeEquivalentTo(createdEvent.ChargeType);
            result.ChargeOwner.Should().BeEquivalentTo(createdEvent.ChargeOwner);
            Assert.NotNull(result.ChargeLinkPeriod);
            result.ChargeLinkPeriod.StartDateTime.Seconds.Should()
                .Be(createdEvent.ChargeLinkPeriod.StartDateTime.ToUnixTimeSeconds());
            result.ChargeLinkPeriod.EndDateTime.Seconds.Should()
                .Be(createdEvent.ChargeLinkPeriod.EndDateTime.ToUnixTimeSeconds());
            result.ChargeLinkPeriod.Factor.Should().Be(createdEvent.ChargeLinkPeriod.Factor);
        }

        [Fact]
        public void Convert_WhenCalledWithNull_ShouldThrow()
        {
            var mapper = new ChargeLinkCreatedOutboundMapper();
            ChargeLinkCreatedEvent? chargeLinkCreatedEvent = null;
            Assert.Throws<InvalidOperationException>(() => mapper.Convert(chargeLinkCreatedEvent!));
        }

        private static ChargeLinkCreatedEvent GetCreatedEvent()
        {
            var period = new ChargeLinkPeriod(
                Instant.FromUtc(2021, 8, 31, 22, 0, 0),
                Instant.FromUtc(2021, 9, 30, 22, 0, 0),
                5);
            return new ChargeLinkCreatedEvent(
                "chargeLinkId",
                "meteringPointId",
                "chargeId",
                ChargeType.Tariff,
                "chargeOwner",
                period);
        }
    }
}
