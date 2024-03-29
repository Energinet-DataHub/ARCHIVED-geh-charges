﻿// Copyright 2020 Energinet DataHub A/S
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
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.Infrastructure.Persistence;
using GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.TestHelpers;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.Repositories
{
    /// <summary>
    /// Tests <see cref="MeteringPointRepository"/> using a database.
    /// </summary>
    [IntegrationTest]
    public class MeteringPointRepositoryTests : IClassFixture<ChargesDatabaseFixture>
    {
        private readonly ChargesDatabaseManager _databaseManager;

        public MeteringPointRepositoryTests(ChargesDatabaseFixture fixture)
        {
            _databaseManager = fixture.DatabaseManager;
        }

        [Fact]
        public async Task StoreMeteringPointAsync_WhenMeteringPointIsCreated_StoresMeteringPointInDatabase()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            var expected = GetMeteringPoint(chargesDatabaseWriteContext);
            var sut = new MeteringPointRepository(chargesDatabaseWriteContext);

            // Act
            await sut.AddAsync(expected).ConfigureAwait(false);
            await chargesDatabaseWriteContext.SaveChangesAsync();

            // Assert
            await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();
            var actual = chargesDatabaseReadContext.MeteringPoints.Single(x => x.MeteringPointId == expected.MeteringPointId);
            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task StoreMeteringPointAsync_WhenMeteringPointIsNull_ShouldThrow(MeteringPointRepository sut)
        {
            await Assert
                .ThrowsAsync<ArgumentNullException>(() => sut.AddAsync(null!))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task GetMeteringPointAsync_WithMeteringPointId_ThenSuccessReturnedAsync()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            var expected = GetMeteringPoint(chargesDatabaseWriteContext);
            await chargesDatabaseWriteContext.MeteringPoints.AddAsync(expected).ConfigureAwait(false);
            await chargesDatabaseWriteContext.SaveChangesAsync().ConfigureAwait(false);

            await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();
            var sut = new MeteringPointRepository(chargesDatabaseReadContext);

            // Act
            var actual = await sut.GetMeteringPointAsync(expected.MeteringPointId).ConfigureAwait(false);

            // Assert
            Assert.NotNull(actual);
        }

        [Fact]
        public async Task GetOrNullAsync_WithExistingMeteringPointId_ReturnsMeteringPoint()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            var expected = GetMeteringPoint(chargesDatabaseWriteContext);
            await chargesDatabaseWriteContext.MeteringPoints.AddAsync(expected).ConfigureAwait(false);
            await chargesDatabaseWriteContext.SaveChangesAsync().ConfigureAwait(false);

            await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();
            var sut = new MeteringPointRepository(chargesDatabaseReadContext);

            // Act
            var actual = await sut.GetOrNullAsync(expected.MeteringPointId).ConfigureAwait(false);

            // Assert
            Assert.Equal(expected.MeteringPointId, actual?.MeteringPointId);
        }

        [Fact]
        public async Task GetOrNullAsync_WithUnknownMeteringPointId_ReturnsNull()
        {
            // Arrange
            await using var chargeDatabaseReadContext = _databaseManager.CreateDbContext();
            var sut = new MeteringPointRepository(chargeDatabaseReadContext);

            // Act
            var actual = await sut.GetOrNullAsync("1234567890").ConfigureAwait(false);

            // Assert
            Assert.Null(actual);
        }

        private static MeteringPoint GetMeteringPoint(IChargesDatabaseContext context)
        {
            var gridAreaLinkId = context.GridAreaLinks.First().Id;
            return MeteringPoint.Create(
                Guid.NewGuid().ToString("N"),
                MeteringPointType.Consumption,
                gridAreaLinkId,
                SystemClock.Instance.GetCurrentInstant(),
                ConnectionState.Connected,
                SettlementMethod.Flex);
        }
    }
}
