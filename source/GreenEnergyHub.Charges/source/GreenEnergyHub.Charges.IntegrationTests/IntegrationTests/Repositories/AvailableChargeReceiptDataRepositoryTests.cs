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
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.Infrastructure.Repositories;
using GreenEnergyHub.Charges.IntegrationTests.Database;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.Repositories
{
    [IntegrationTest]
    public class AvailableChargeReceiptDataRepositoryTests : IClassFixture<ChargesDatabaseFixture>
    {
        private readonly ChargesDatabaseManager _databaseManager;

        public AvailableChargeReceiptDataRepositoryTests(ChargesDatabaseFixture fixture)
        {
            _databaseManager = fixture.DatabaseManager;
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task StoreAsync_StoresAvailableChargeReceiptData([NotNull] List<AvailableChargeReceiptData> expectedList)
        {
            // Arrange
            expectedList = RepositoryAutoMoqDataFixer.GetAvailableChargeReceiptDataListBasedOn(expectedList);

            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();

            var sut = new AvailableChargeReceiptDataRepository(chargesDatabaseWriteContext);

            // Act
            await sut.StoreAsync(expectedList).ConfigureAwait(false);

            // Assert
            await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();

            foreach (var expected in expectedList)
            {
                var actual = chargesDatabaseReadContext
                    .AvailableChargeReceiptData
                    .Single(x => x.AvailableDataReferenceId == expected.AvailableDataReferenceId);
                actual.Should().BeEquivalentTo(expected);
            }
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task GetAsync_GivenAnExistingAvailableDataReferenceId_ReturnsAvailableChargeReceiptData(
            [NotNull] AvailableChargeReceiptData expected)
        {
            // Arrange
            expected = RepositoryAutoMoqDataFixer.GetAvailableChargeReceiptDataBasedOn(expected);
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            await chargesDatabaseWriteContext.AvailableChargeReceiptData.AddAsync(expected).ConfigureAwait(false);
            await chargesDatabaseWriteContext.SaveChangesAsync().ConfigureAwait(false);

            await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();
            var sut = new AvailableChargeReceiptDataRepository(chargesDatabaseReadContext);

            // Act
            var actual =
                await sut.GetAsync(new List<Guid> { expected.AvailableDataReferenceId })
                .ConfigureAwait(false);

            // Assert
            Assert.NotNull(actual);
            actual.Should().ContainSingle();
            actual[0].Should().BeEquivalentTo(expected);
        }
    }
}
