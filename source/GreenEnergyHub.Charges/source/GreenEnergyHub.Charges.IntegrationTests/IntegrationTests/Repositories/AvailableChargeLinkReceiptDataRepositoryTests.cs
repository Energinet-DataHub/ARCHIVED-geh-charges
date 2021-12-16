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
using GreenEnergyHub.Charges.IntegrationTests.Database;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinkReceiptData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.Repositories
{
    [IntegrationTest]
    public class AvailableChargeLinkReceiptDataRepositoryTests : IClassFixture<MessageHubDatabaseFixture>
    {
        private readonly MessageHubDatabaseManager _databaseManager;

        public AvailableChargeLinkReceiptDataRepositoryTests(MessageHubDatabaseFixture fixture)
        {
            _databaseManager = fixture.DatabaseManager;
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task StoreAsync_StoresAvailableChargeLinkReceiptData([NotNull] List<AvailableChargeLinkReceiptData> expectedList)
        {
            // Arrange
            expectedList = RepositoryAutoMoqDataFixer.GetAvailableChargeLinkReceiptDataListBasedOn(expectedList);

            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();

            var sut = new AvailableChargeLinkReceiptDataRepository(chargesDatabaseWriteContext);

            // Act
            await sut.StoreAsync(expectedList).ConfigureAwait(false);

            // Assert
            await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();

            foreach (var expected in expectedList)
            {
                var actual = chargesDatabaseReadContext
                    .AvailableChargeLinkReceiptData
                    .Single(x => x.AvailableDataReferenceId == expected.AvailableDataReferenceId);
                actual.Should().BeEquivalentTo(expected);
            }
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task GetAsync_GivenAnExistingAvailableDataReferenceId_ReturnsAvailableChargeLinkReceiptData(
            [NotNull] AvailableChargeLinkReceiptData expected)
        {
            // Arrange
            expected = RepositoryAutoMoqDataFixer.GetAvailableChargeLinkReceiptDataBasedOn(expected);
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            await chargesDatabaseWriteContext.AvailableChargeLinkReceiptData.AddAsync(expected).ConfigureAwait(false);
            await chargesDatabaseWriteContext.SaveChangesAsync().ConfigureAwait(false);

            await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();
            var sut = new AvailableChargeLinkReceiptDataRepository(chargesDatabaseReadContext);

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
