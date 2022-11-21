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
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.Repositories
{
    /// <summary>
    /// Tests <see cref="ChargeHistoryRepository"/> using a database.
    /// </summary>
    [IntegrationTest]
    public class ChargeHistoryRepositoryTests : IClassFixture<ChargesQueryDatabaseFixture>
    {
        private readonly ChargesQueryDatabaseManager _databaseManager;

        public ChargeHistoryRepositoryTests(ChargesQueryDatabaseFixture fixture)
        {
            _databaseManager = fixture.DatabaseManager;
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task AddAsync_WhenChargeHistoriesAreValid_ChargeHistoriesAreAdded(
            ChargeHistoryBuilder chargeHistoryBuilder)
        {
            // Arrange
            await using var chargesQueryDatabaseWriteContext = _databaseManager.CreateDbContext();
            var chargeHistory = chargeHistoryBuilder
                .WithSenderProvidedChargeId("ChargeFee_id")
                .WithChargeType(ChargeType.Fee)
                .WithOwner("ChargeOwner")
                .WithName("ChargeName")
                .WithTaxIndicator(TaxIndicator.Tax)
                .WithTransparentInvoicing(TransparentInvoicing.Transparent)
                .Build();
            var chargeHistories = new List<ChargeHistory>
            {
                chargeHistoryBuilder
                    .WithSenderProvidedChargeId("ChargeFee_id")
                    .WithChargeType(ChargeType.Fee)
                    .WithOwner("ChargeOwner")
                    .WithName("ChargeName")
                    .WithTaxIndicator(TaxIndicator.Tax)
                    .WithTransparentInvoicing(TransparentInvoicing.Transparent)
                    .Build(),
            };

            var sut = new ChargeHistoryRepository(chargesQueryDatabaseWriteContext);

            // Act
            await sut.AddRangeAsync(chargeHistories);
            await chargesQueryDatabaseWriteContext.SaveChangesAsync();

            // Assert
            await using var chargesQueryDatabaseReadContext = _databaseManager.CreateDbContext();
            var actual = await chargesQueryDatabaseReadContext.ChargeHistories.SingleAsync(x =>
                x.SenderProvidedChargeId == chargeHistory.SenderProvidedChargeId &&
                x.Type == chargeHistory.Type &&
                x.Owner == chargeHistory.Owner);
            actual.SenderProvidedChargeId.Should().Be(chargeHistory.SenderProvidedChargeId);
            actual.Type.Should().Be(chargeHistory.Type);
            actual.Owner.Should().Be(chargeHistory.Owner);
            actual.Name.Should().Be(chargeHistory.Name);
            actual.TaxIndicator.Should().Be(chargeHistory.TaxIndicator);
            actual.TransparentInvoicing.Should().Be(chargeHistory.TransparentInvoicing);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task AddAsync_WhenChargeHistoryIsNull_ThrowsArgumentNullException(ChargeHistoryRepository sut)
        {
            // Arrange
            List<ChargeHistory>? chargeHistories = null;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.AddRangeAsync(chargeHistories!));
        }
    }
}
