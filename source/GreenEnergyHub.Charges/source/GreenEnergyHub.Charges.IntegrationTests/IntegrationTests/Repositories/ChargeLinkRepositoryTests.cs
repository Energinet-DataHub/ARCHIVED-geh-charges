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
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.Infrastructure.Persistence;
using GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.Repositories
{
    /// <summary>
    /// Tests <see cref="ChargeRepository"/> using a database.
    /// </summary>
    [IntegrationTest]
    public class ChargeLinkRepositoryTests : IClassFixture<ChargesDatabaseFixture>
    {
        private readonly ChargesDatabaseManager _databaseManager;

        public ChargeLinkRepositoryTests(ChargesDatabaseFixture fixture)
        {
            _databaseManager = fixture.DatabaseManager;
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task AddRangeAsync_StoresChargeLink(ChargeLink chargeLink)
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            var ids = SeedDatabase(chargesDatabaseWriteContext);
            var expected = CreateExpectedChargeLink(chargeLink, ids);
            var sut = new ChargeLinksRepository(chargesDatabaseWriteContext);

            // Act
            await sut.AddAsync(expected).ConfigureAwait(false);
            await chargesDatabaseWriteContext.SaveChangesAsync();

            // Assert
            await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();

            var actual = await chargesDatabaseReadContext.ChargeLinks.SingleAsync(
                    c => c.ChargeId == ids.ChargeId && c.MeteringPointId == ids.MeteringPointId)
                .ConfigureAwait(false);
            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task AddRangeAsync_WhenIdenticalChargeLinksAreReceived_ThrowsUniqueConstraintException(ChargeLink chargeLink)
        {
            // Seed Arrange
            await using var firstChargesContext = _databaseManager.CreateDbContext();
            var ids = SeedDatabase(firstChargesContext);
            var setupRepo = new ChargeLinksRepository(firstChargesContext);

            var seededChargeLink = CreateExpectedChargeLink(chargeLink, ids);

            await setupRepo.AddAsync(seededChargeLink);
            await firstChargesContext.SaveChangesAsync();

            // Arrange copy
            await using var secondChargesContext = _databaseManager.CreateDbContext();
            var sut = new ChargeLinksRepository(secondChargesContext);
            var copyChargeLink = CreateExpectedChargeLink(chargeLink, ids);

            // Act
            Func<Task> act = async () =>
            {
                await sut.AddAsync(copyChargeLink);
                await secondChargesContext.SaveChangesAsync();
            };

            // Assert
            await act.Should()
                .ThrowAsync<DbUpdateException>()
                .WithInnerException(typeof(SqlException))
                .WithMessage("Violation of UNIQUE KEY constraint 'UQ_DefaultOverlap_StartDateTime'*");
        }

        private ChargeLink CreateExpectedChargeLink(ChargeLink chargeLink, (Guid ChargeId, Guid MeteringPointId) ids)
        {
            return new ChargeLink(
                ids.ChargeId,
                ids.MeteringPointId,
                chargeLink.StartDateTime,
                chargeLink.EndDateTime,
                chargeLink.Factor);
        }

        private static (Guid ChargeId, Guid MeteringPointId) SeedDatabase(ChargesDatabaseContext context)
        {
            var marketParticipantId = "MarketParticipantId";
            var existingMeteringPoint = context.MeteringPoints.FirstOrDefault();
            var existingCharge = context.Charges.FirstOrDefault();

            if (existingMeteringPoint is not null && existingCharge is not null)
                return (existingCharge.Id, existingMeteringPoint.Id);

            var marketParticipant = new MarketParticipant(
                Guid.NewGuid(),
                marketParticipantId,
                true,
                MarketParticipantRole.EnergySupplier);

            context.MarketParticipants.Add(marketParticipant);
            context.SaveChanges(); // Sets marketParticipant.RowId

            var charge = new ChargeBuilder().WithOwnerId(marketParticipant.Id).Build();
            context.Charges.Add(charge);

            var gridAreaLinkId = context.GridAreaLinks.First().Id;

            var meteringPoint = MeteringPoint.Create(
                Guid.NewGuid().ToString("N"),
                MeteringPointType.ElectricalHeating,
                gridAreaLinkId,
                SystemClock.Instance.GetCurrentInstant(),
                ConnectionState.Connected,
                SettlementMethod.Flex);
            context.MeteringPoints.Add(meteringPoint);

            context.SaveChanges();

            return (charge.Id, meteringPoint.Id);
        }
    }
}
