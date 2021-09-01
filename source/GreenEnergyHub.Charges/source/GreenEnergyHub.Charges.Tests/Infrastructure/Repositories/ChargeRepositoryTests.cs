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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction;
using GreenEnergyHub.Charges.Domain.MarketDocument;
using GreenEnergyHub.Charges.Infrastructure.Context;
using GreenEnergyHub.Charges.Infrastructure.Repositories;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Squadron;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Squadron;
using Xunit;
using Xunit.Categories;
using Charge = GreenEnergyHub.Charges.Domain.Charge;
using MarketParticipant = GreenEnergyHub.Charges.Infrastructure.Context.Model.MarketParticipant;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Repositories
{
    /// <summary>
    /// Tests <see cref="ChargeRepository"/> using an SQLite in-memory database.
    /// </summary>
    [UnitTest]
    public class ChargeRepositoryTest : IClassFixture<SqlServerResource<SqlServerOptions>>
    {
        private const string MarketParticipantId = "MarketParticipantId";
        private readonly SqlServerResource<SqlServerOptions> _resource;

        public ChargeRepositoryTest(SqlServerResource<SqlServerOptions> resource)
        {
            _resource = resource;
        }

        [Fact]
        public async Task GetChargeAsync_WhenChargeIsCreated_ThenSuccessReturnedAsync()
        {
            await using var chargesDatabaseContext = await SquadronContextFactory
                .GetDatabaseContextAsync(_resource)
                .ConfigureAwait(false);
            // Arrange
            var charge = GetValidCharge();
            await SeedDatabase(chargesDatabaseContext).ConfigureAwait(false);
            var sut = new ChargeRepository(chargesDatabaseContext);

            // Act
            await sut.StoreChargeAsync(charge).ConfigureAwait(false);
            var expected = await sut.GetChargeAsync(charge.Id, charge.Owner, charge.Type).ConfigureAwait(false);

            // Assert
            Assert.NotNull(expected);
        }

        [Fact]
        public async Task CheckIfChargeExistsAsync_WhenChargeIsCreated_ThenSuccessReturnedAsync()
        {
            await using var chargesDatabaseContext = await SquadronContextFactory
                .GetDatabaseContextAsync(_resource)
                .ConfigureAwait(false);
            // Arrange
            var charge = GetValidCharge();
            await SeedDatabase(chargesDatabaseContext).ConfigureAwait(false);
            var sut = new ChargeRepository(chargesDatabaseContext);

            // Act
            await sut.StoreChargeAsync(charge).ConfigureAwait(false);

            var expected = await sut.CheckIfChargeExistsAsync(
                charge.Id,
                charge.Owner,
                charge.Type).ConfigureAwait(false);

            // Assert
            Assert.True(expected);
        }

        [Fact]
        public async Task CheckIfChargeExistsByCorrelationIdAsync_WhenChargeIsCreated_ThenSuccessReturnedAsync()
        {
            await using var chargesDatabaseContext = await SquadronContextFactory
                .GetDatabaseContextAsync(_resource)
                .ConfigureAwait(false);

            // Arrange
            var charge = GetValidCharge();
            await SeedDatabase(chargesDatabaseContext).ConfigureAwait(false);
            var sut = new ChargeRepository(chargesDatabaseContext);

            // Act
            await sut.StoreChargeAsync(charge).ConfigureAwait(false);
            var expected =
                await sut.CheckIfChargeExistsByCorrelationIdAsync(charge.CorrelationId).ConfigureAwait(false);

            // Assert
            Assert.True(expected);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task StoreChargeAsync_WhenChargeIsNull_ThrowsArgumentNullException(
            [NotNull] ChargeRepository sut)
        {
            // Arrange
            Charge? charge = null;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => sut.StoreChargeAsync(charge!))
                .ConfigureAwait(false);
        }

        private static Charge GetValidCharge()
        {
            var transaction = new Charge
            {
                Name = "description",
                Id = "Id",
                Owner = MarketParticipantId,
                StartDateTime = SystemClock.Instance.GetCurrentInstant(),
                Points = new List<Point>
                    {
                        new Point { Position = 0, Time = SystemClock.Instance.GetCurrentInstant(), Price = 200m },
                    },
                Resolution = Resolution.P1D,
                Type = ChargeType.Fee,
                VatClassification = VatClassification.NoVat,
                Description = "LongDescription",

                Document = new Document
                {
                    Id = "id",
                    RequestDate = SystemClock.Instance.GetCurrentInstant(),
                    Type = DocumentType.RequestUpdateChargeInformation,
                    IndustryClassification = IndustryClassification.Electricity,
                    CreatedDateTime = SystemClock.Instance.GetCurrentInstant(),
                    Sender = new Domain.MarketDocument.MarketParticipant
                    {
                        Id = MarketParticipantId,
                        BusinessProcessRole = (MarketParticipantRole)1,
                    },
                    BusinessReasonCode = BusinessReasonCode.UpdateChargeInformation,
                },
                ChargeOperationId = "id",
                LastUpdatedBy = "LastUpdatedBy",
                CorrelationId = "CorrelationId",
            };
            return transaction;
        }

        private static async Task SeedDatabase(ChargesDatabaseContext context)
        {
            var marketParticipant = await context.MarketParticipants.SingleOrDefaultAsync(x => x.MarketParticipantId == MarketParticipantId)
                .ConfigureAwait(false);

            if (marketParticipant == null)
            {
                context.MarketParticipants.Add(
                                new MarketParticipant { Name = "Name", Role = 1, MarketParticipantId = MarketParticipantId });
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}
