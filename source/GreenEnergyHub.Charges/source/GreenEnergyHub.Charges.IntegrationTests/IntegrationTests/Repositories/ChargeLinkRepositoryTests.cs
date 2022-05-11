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
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.ChargeInformation;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.ChargePrices;
using GreenEnergyHub.Charges.Domain.Common;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.Infrastructure.Persistence;
using GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.Repositories
{
    /// <summary>
    /// Tests <see cref="ChargeInformationRepository"/> using a database.
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
                    c => c.ChargeInformationId == ids.ChargeInformationId && c.MeteringPointId == ids.MeteringPointId)
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

        private ChargeLink CreateExpectedChargeLink(ChargeLink chargeLink, (Guid ChargeInformationId, Guid MeteringPointId) ids)
        {
            return new ChargeLink(
                ids.ChargeInformationId,
                ids.MeteringPointId,
                chargeLink.StartDateTime,
                chargeLink.EndDateTime,
                chargeLink.Factor);
        }

        private static (Guid ChargeInformationId, Guid MeteringPointId) SeedDatabase(ChargesDatabaseContext context)
        {
            var marketParticipantId = "MarketParticipantId";
            var existingMeteringPoint = context.MeteringPoints.FirstOrDefault();
            var existingCharge = context.ChargeInformations.FirstOrDefault();

            if (existingMeteringPoint is not null && existingCharge is not null)
                return (existingCharge.Id, existingMeteringPoint.Id);

            var marketParticipant = new MarketParticipant(
                Guid.NewGuid(),
                marketParticipantId,
                true,
                MarketParticipantRole.EnergySupplier);

            context.MarketParticipants.Add(marketParticipant);
            context.SaveChanges(); // Sets marketParticipant.RowId

            var charge = new ChargeInformation(
                Guid.NewGuid(),
                "charge id",
                marketParticipant.Id,
                ChargeType.Tariff,
                Resolution.P1D,
                false,
                new List<ChargePeriod>
                {
                    new ChargePeriod(
                        Guid.NewGuid(),
                        "charge name",
                        "charge description",
                        VatClassification.Vat25,
                        false,
                        SystemClock.Instance.GetCurrentInstant(),
                        SystemClock.Instance.GetCurrentInstant()),
                });
            context.ChargeInformations.Add(charge);

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
