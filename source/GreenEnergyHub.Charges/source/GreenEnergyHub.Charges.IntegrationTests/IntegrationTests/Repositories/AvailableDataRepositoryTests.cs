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
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.MessageHub;
using NodaTime;
using Xunit;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.Repositories
{
    public abstract class AvailableDataRepositoryTests<TAvailableData> : IClassFixture<MessageHubDatabaseFixture>
        where TAvailableData : AvailableDataBase
    {
        private readonly MessageHubDatabaseManager _databaseManager;

        protected AvailableDataRepositoryTests(MessageHubDatabaseFixture fixture)
        {
            _databaseManager = fixture.DatabaseManager;
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task StoreAsync_StoresAvailableData(List<TAvailableData> unfixedExpectedList)
        {
            // Arrange
            var expectedList = RepositoryAutoMoqDataFixer
                .GetAvailableDataListBasedOn(unfixedExpectedList.Cast<AvailableDataBase>().ToList())
                .Cast<TAvailableData>()
                .ToList();

            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();

            var sut = new AvailableDataRepository<TAvailableData>(chargesDatabaseWriteContext);

            // Act
            await sut.StoreAsync(expectedList).ConfigureAwait(false);

            // Assert
            await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();

            foreach (var expected in expectedList)
            {
                var actual = chargesDatabaseReadContext
                    .Set<TAvailableData>()
                    .Single(x => x.AvailableDataReferenceId == expected.AvailableDataReferenceId);
                actual.Should().BeEquivalentTo(expected);
            }
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task GetAsync_GivenAnExistingAvailableDataReferenceId_ReturnsAvailableData(TAvailableData expected)
        {
            // Arrange
            expected = (TAvailableData)RepositoryAutoMoqDataFixer.GetAvailableDataBasedOn(expected);
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            await chargesDatabaseWriteContext.Set<TAvailableData>().AddAsync(expected).ConfigureAwait(false);
            await chargesDatabaseWriteContext.SaveChangesAsync().ConfigureAwait(false);

            await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();
            var sut = new AvailableDataRepository<TAvailableData>(chargesDatabaseReadContext);

            // Act
            var actual =
                await sut.GetAsync(new List<Guid> { expected.AvailableDataReferenceId })
                .ConfigureAwait(false);

            // Assert
            Assert.NotNull(actual);
            actual.Should().ContainSingle();
            actual[0].Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task GetAsync_WhenChargeDataAvailableIsOperationOrdered_ReturnsOrderedData()
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            var availableChargeDataList = GenerateListOfAvailableChargeDataForSameCharge(3);
            await chargesDatabaseWriteContext.AddRangeAsync(availableChargeDataList);
            await chargesDatabaseWriteContext.SaveChangesAsync();

            await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();
            var sut = new AvailableDataRepository<AvailableChargeData>(chargesDatabaseReadContext);

            // Act
            var actual =
                await sut.GetAsync(new List<Guid>
                    {
                        availableChargeDataList[2].AvailableDataReferenceId,
                        availableChargeDataList[0].AvailableDataReferenceId,
                        availableChargeDataList[1].AvailableDataReferenceId,
                    })
                .ConfigureAwait(false);

            // Assert
            Assert.NotNull(actual);
            actual.Should().BeInAscendingOrder(a => a.RequestDateTime)
                .And.ThenBeInAscendingOrder(a => a.OperationOrder);
        }

        private static List<AvailableChargeData> GenerateListOfAvailableChargeDataForSameCharge(int numberOfAvailableChargeData)
        {
            var builder = new AvailableChargeDataBuilder();
            var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
            var availableChargeDataList = new List<AvailableChargeData>();

            for (var i = 0; i < numberOfAvailableChargeData; i++)
            {
                var data = builder.WithRequestDateTime(now).WithOperationOrder(i).Build();
                availableChargeDataList.Add(data);
            }

            return availableChargeDataList;
        }
    }
}
