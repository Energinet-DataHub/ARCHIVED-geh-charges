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
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.Repositories
{
    [IntegrationTest]
    public class AvailableChargeDataRepositoryTests : IClassFixture<MessageHubDatabaseFixture>
    {
        private readonly MessageHubDatabaseManager _databaseManager;

        public AvailableChargeDataRepositoryTests(MessageHubDatabaseFixture fixture)
        {
            _databaseManager = fixture.DatabaseManager;
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task StoreAsync_StoresAvailableChargeData([NotNull] AvailableChargeData expected)
        {
            // Arrange
            expected = RepositoryAutoMoqDataFixer.GetAvailableChargeDataBasedOn(expected);
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();

            var sut = new AvailableChargeDataRepository(chargesDatabaseWriteContext);

            // Act
            await sut.StoreAsync(new List<AvailableChargeData>() { expected }).ConfigureAwait(false);

            // Assert
            await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();
            var actual =
                chargesDatabaseReadContext.AvailableChargeData
                    .Single(x => x.AvailableDataReferenceId == expected.AvailableDataReferenceId);
            actual.VatClassification.Should().Be(expected.VatClassification);
            actual.Points.Should().BeEquivalentTo(expected.Points);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task GetAsync_GivenAnExistingAvailableDataReferenceId_ReturnsAvailableChargeData(
            [NotNull] AvailableChargeData expected)
        {
            // Arrange
            expected = RepositoryAutoMoqDataFixer.GetAvailableChargeDataBasedOn(expected);
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            await chargesDatabaseWriteContext.AvailableChargeData.AddAsync(expected).ConfigureAwait(false);
            await chargesDatabaseWriteContext.SaveChangesAsync().ConfigureAwait(false);

            await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();
            var sut = new AvailableChargeDataRepository(chargesDatabaseReadContext);

            // Act
            var actual =
                await sut.GetAsync(new List<Guid> { expected.AvailableDataReferenceId })
                .ConfigureAwait(false);

            // Assert
            Assert.NotNull(actual);
        }
    }
}
