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

using System.Threading.Tasks;
using FluentAssertions;
using GreenEnergyHub.Charges.Infrastructure.Outbox;
using GreenEnergyHub.Charges.Infrastructure.Persistence;
using GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.Repositories
{
    /// <summary>
    /// Tests <see cref="OutboxMessageRepository"/> using a database.
    /// </summary>
    [IntegrationTest]
    public class OutboxMessageRepositoryTests : IClassFixture<ChargesDatabaseFixture>
    {
        private readonly ChargesDatabaseManager _databaseManager;

        public OutboxMessageRepositoryTests(ChargesDatabaseFixture fixture)
        {
            _databaseManager = fixture.DatabaseManager;
        }

        [Theory]
        [AutoDomainData]
        public async Task Add_WhenOutboxMessageProvided_OutboxMessageIsAdded(OutboxMessageBuilder outboxMessageBuilder)
        {
            // Arrange
            var outboxMessage = outboxMessageBuilder.Build();
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            var sut = new OutboxMessageRepository(chargesDatabaseWriteContext);

            // Act
            sut.Add(outboxMessage);
            await chargesDatabaseWriteContext.SaveChangesAsync();

            // Assert
            await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();

            var actual = await chargesDatabaseReadContext.OutboxMessages
                .SingleAsync(x => x.Id == outboxMessage.Id);

            actual.Should().BeEquivalentTo(outboxMessage);
        }

        [Theory]
        [AutoDomainData]
        public async Task GetNext_WhenMultipleOutboxMessagesExists_ThenReturnTheEarliestAndNotProcessedOutboxMessage(
            OutboxMessageBuilder outboxMessageBuilder)
        {
            // Arrange
            await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
            // The previous test adds data to the database that we need to remove.
            await RemoveAllExistingOutboxMessages(chargesDatabaseWriteContext);
            await AddOutboxMessageToContextAndSaveAsync(chargesDatabaseWriteContext, outboxMessageBuilder, InstantHelper.GetYesterdayAtMidnightUtc(), true);
            await AddOutboxMessageToContextAndSaveAsync(chargesDatabaseWriteContext, outboxMessageBuilder, InstantHelper.GetTomorrowAtMidnightUtc(), false);
            var expected = await AddOutboxMessageToContextAndSaveAsync(chargesDatabaseWriteContext, outboxMessageBuilder, InstantHelper.GetTodayAtMidnightUtc(), false);

            var sut = new OutboxMessageRepository(chargesDatabaseWriteContext);

            // Act
            var actual = sut.GetNext();

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        private static async Task RemoveAllExistingOutboxMessages(ChargesDatabaseContext chargesDatabaseWriteContext)
        {
            var oldOutboxMessages = chargesDatabaseWriteContext.OutboxMessages;
            chargesDatabaseWriteContext.OutboxMessages.RemoveRange(oldOutboxMessages);
            await chargesDatabaseWriteContext.SaveChangesAsync();
        }

        private static async Task<OutboxMessage> AddOutboxMessageToContextAndSaveAsync(
            ChargesDatabaseContext chargesDatabaseWriteContext,
            OutboxMessageBuilder outboxMessageBuilder,
            Instant instant,
            bool isProcessed)
        {
            var outboxMessage = outboxMessageBuilder.WithCreationDate(instant).Build();
            if (isProcessed)
                outboxMessage.SetProcessed(InstantHelper.GetTodayAtMidnightUtc());

            chargesDatabaseWriteContext.OutboxMessages.Add(outboxMessage);
            await chargesDatabaseWriteContext.SaveChangesAsync();
            return outboxMessage;
        }
    }
}
