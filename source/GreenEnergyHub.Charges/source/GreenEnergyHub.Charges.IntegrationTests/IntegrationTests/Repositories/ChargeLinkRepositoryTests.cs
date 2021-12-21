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
using System.Threading.Tasks;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.Infrastructure.Context;
using GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures.Database;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.Repositories
{
    /// <summary>
    /// Tests <see cref="ChargeRepository"/> using a SQL database.
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
        public async Task StoreAsync_StoresChargeLink(ChargeLink chargeLink)
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();

            var ids = SeedDatabase(chargesDatabaseWriteContext);
            var expected = CreateExpectedChargeLink(chargeLink, ids);
            var sut = new ChargeLinkRepository(chargesDatabaseWriteContext);

            // Act
            await sut.StoreAsync(new List<ChargeLink> { expected }).ConfigureAwait(false);

            // Assert
            await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();

            var actual = await chargesDatabaseReadContext.ChargeLinks.SingleAsync(
                    c => c.ChargeId == ids.ChargeId && c.MeteringPointId == ids.MeteringPointId)
                .ConfigureAwait(false);
            actual.Should().BeEquivalentTo(expected);
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
            var marketParticipant = new MarketParticipant
            {
                BusinessProcessRole = MarketParticipantRole.EnergySupplier,
                MarketParticipantId = "MarketParticipantId",
            };

            context.MarketParticipants.Add(marketParticipant);
            context.SaveChanges(); // Sets marketParticipant.RowId

            var charge = new Charge(
                id: Guid.NewGuid(),
                senderProvidedChargeId: "charge id",
                name: "charge name",
                description: "charge description",
                ownerId: marketParticipant.Id,
                startDateTime: SystemClock.Instance.GetCurrentInstant(),
                endDateTime: SystemClock.Instance.GetCurrentInstant(),
                type: ChargeType.Tariff,
                vatClassification: VatClassification.Vat25,
                resolution: Resolution.P1D,
                transparentInvoicing: false,
                taxIndicator: false,
                points: new List<Point>());
            context.Charges.Add(charge);

            var meteringPoint = MeteringPoint.Create(
                Guid.NewGuid().ToString("N"),
                MeteringPointType.ElectricalHeating,
                "some-area-id",
                SystemClock.Instance.GetCurrentInstant(),
                ConnectionState.Connected,
                SettlementMethod.Flex);
            context.MeteringPoints.Add(meteringPoint);

            context.SaveChanges();

            return (charge.Id, meteringPoint.Id);
        }
    }
}
