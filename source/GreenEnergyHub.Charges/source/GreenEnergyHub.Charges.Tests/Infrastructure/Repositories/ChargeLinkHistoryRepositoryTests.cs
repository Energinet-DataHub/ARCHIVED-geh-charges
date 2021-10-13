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
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommands;
using GreenEnergyHub.Charges.Domain.ChargeLinkHistory;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Repositories;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Database;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Repositories
{
    [UnitTest]
    public class ChargeLinkHistoryRepositoryTests : IClassFixture<ChargesDatabaseFixture>
    {
        private readonly ChargesDatabaseManager _databaseManager;

        public ChargeLinkHistoryRepositoryTests(ChargesDatabaseFixture fixture)
        {
            _databaseManager = fixture.DatabaseManager;
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task StoreAsync_StoresChargeLinkHistory([Frozen] Mock<IChargeLinkFactory> chargeLinkFactory)
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();

            var expected = CreateChargeLinkHistory();
            chargeLinkFactory
                .Setup(x => x.MapChargeLinkCommandAcceptedEvent(It.IsAny<ChargeLinkCommandAcceptedEvent>(), It.IsAny<MarketParticipant>()))
                .Returns(expected);

            var sut = new ChargeLinkHistoryRepository(chargesDatabaseWriteContext, chargeLinkFactory.Object);

            // Act
            await sut.StoreAsync(null!, null!).ConfigureAwait(false);

            // Assert
            await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();
            var actual = chargesDatabaseReadContext.ChargeLinkHistories.First();
            actual.Owner.Should().Be(expected.Owner);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task GetMeteringPointAsync_WithMeteringPointId_ThenSuccessReturnedAsync([Frozen] Mock<IChargeLinkFactory> chargeLinkFactory)
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            var expected = CreateChargeLinkHistory();
            await chargesDatabaseWriteContext.ChargeLinkHistories.AddAsync(expected).ConfigureAwait(false);
            await chargesDatabaseWriteContext.SaveChangesAsync().ConfigureAwait(false);

            await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();
            var sut = new ChargeLinkHistoryRepository(chargesDatabaseReadContext, chargeLinkFactory.Object);

            // Act
            var actual =
                await sut.GetChargeHistoriesAsync(new List<Guid> { expected.PostOfficeId })
                .ConfigureAwait(false);

            // Assert
            Assert.NotNull(actual);
        }

        private ChargeLinkHistory CreateChargeLinkHistory()
        {
            var chargeLinkHistory = new ChargeLinkHistory(
                "some recipient",
                MarketParticipantRole.EnergyAgency,
                BusinessReasonCode.UpdateChargeInformation,
                "charge id",
                "mp id",
                "some owner",
                5,
                ChargeType.Fee,
                SystemClock.Instance.GetCurrentInstant(),
                SystemClock.Instance.GetCurrentInstant(),
                Guid.NewGuid());

            return chargeLinkHistory;
        }
    }
}
