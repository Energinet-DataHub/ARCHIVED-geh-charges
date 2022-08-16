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
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Charges.Events;
using GreenEnergyHub.Charges.FunctionHost.MessageHub;
using GreenEnergyHub.Charges.Infrastructure.Outbox;
using GreenEnergyHub.Charges.Infrastructure.Persistence;
using GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.FunctionApp;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestCommon;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
using GreenEnergyHub.Charges.MessageHub.MessageHub;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.EndpointTests
{
    [IntegrationTest]
    public class OutboxProcessorEndpointTests
    {
        [Collection(nameof(ChargesFunctionAppCollectionFixture))]
        public class RunAsync : FunctionAppTestBase<ChargesFunctionAppFixture>, IAsyncLifetime
        {
            private readonly ChargesDatabaseManager _databaseManager;

            public RunAsync(
                ChargesFunctionAppFixture fixture,
                ITestOutputHelper testOutputHelper)
                : base(fixture, testOutputHelper)
            {
                _databaseManager = fixture.ChargesDatabaseManager;
            }

            public Task DisposeAsync()
            {
                Fixture.MessageHubMock.Clear();
                return Task.CompletedTask;
            }

            public Task InitializeAsync()
            {
                return Task.CompletedTask;
            }

            [Fact]
            public async Task When_OutboxProcessor_ReadsAnOutboxMessage_PersistsAvailableData()
            {
                // Arrange
                await using var messageHubDatabaseContext = Fixture.MessageHubDatabaseManager.CreateDbContext();
                var operationsRejectedEvent = await CreateAndPersistOutboxMessage();

                // Act
                await FunctionAsserts.AssertHasExecutedAsync(
                    Fixture.HostManager, nameof(OutboxProcessorEndpoint));

                // Assert
                var actualAvailableDataReceipt = await messageHubDatabaseContext.AvailableChargeReceiptData.FirstAsync();
                actualAvailableDataReceipt.RecipientId.Should().Be(operationsRejectedEvent.Command.Document.Sender.MarketParticipantId);
            }

            [Theory]
            [InlineAutoMoqData]
            public async Task GivenOutputProcessorEndpoint_WhenFailsFirstAttempt_ThenRetryNext(
                Mock<IAvailableDataNotifier<AvailableChargeReceiptData, ChargePriceOperationsRejectedEvent>> availableDataNotifier,
                JsonSerializer jsonSerializer,
                TimerInfo timerInfo,
                Mock<IClock> clock,
                Instant now)
            {
                // Arrange
                await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
                await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();
                var unitOfWork = new UnitOfWork(chargesDatabaseWriteContext);
                clock.Setup(c => c.GetCurrentInstant()).Returns(now);
                var outboxMessage = CreateOutboxMessage(now);
                var outboxRepository = new OutboxMessageRepository(chargesDatabaseWriteContext, clock.Object);
                chargesDatabaseWriteContext.OutboxMessages.Add(outboxMessage);
                await chargesDatabaseWriteContext.SaveChangesAsync();
                var sut = new OutboxProcessorEndpoint(
                    outboxRepository,
                    availableDataNotifier.Object,
                    jsonSerializer,
                    clock.Object,
                    unitOfWork);

                availableDataNotifier
                    .Setup(adn =>
                        adn.NotifyAsync(It.IsAny<ChargePriceOperationsRejectedEvent>()))
                    .ThrowsAsync(new Exception());

                await Assert.ThrowsAsync<Exception>(() => sut.RunAsync(timerInfo));

                availableDataNotifier
                    .Setup(adn =>
                        adn.NotifyAsync(It.IsAny<ChargePriceOperationsRejectedEvent>()))
                    .Returns(Task.CompletedTask);

                await sut.RunAsync(timerInfo);

                outboxMessage = chargesDatabaseReadContext.OutboxMessages.First();
                outboxMessage.ProcessedDate.Should().Be(now);
            }

            private async Task<ChargePriceOperationsRejectedEvent> CreateAndPersistOutboxMessage()
            {
                await using var chargesDatabaseContext = _databaseManager.CreateDbContext();
                var correlationContext = CreateCorrelationContext();
                var jsonSerializer = new JsonSerializer();
                var outboxMessageFactory = new OutboxMessageFactory(jsonSerializer, SystemClock.Instance, correlationContext);
                var chargePriceOperationDto = new ChargePriceOperationDtoBuilder().WithPoint(1.00m).Build();
                var chargePriceCommand = new ChargePriceCommandBuilder().WithChargeOperation(chargePriceOperationDto).Build();
                var operationsRejectedEvent = new OperationsRejectedEventBuilder().WithChargeCommand(chargePriceCommand).Build();

                var outboxMessage = outboxMessageFactory.CreateFrom(operationsRejectedEvent);
                chargesDatabaseContext.OutboxMessages.Add(outboxMessage);
                await chargesDatabaseContext.SaveChangesAsync();

                return operationsRejectedEvent;
            }

            private OutboxMessage CreateOutboxMessage(Instant now)
            {
                var jsonSerializer = new JsonSerializer();
                var rejectedEvent = new OperationsRejectedEventBuilder().Build();
                var data = jsonSerializer.Serialize(rejectedEvent);
                return new OutboxMessageBuilder()
                    .WithData(data)
                    .WithType(rejectedEvent.GetType().ToString())
                    .WithCreationDate(now)
                    .Build();
            }

            private static CorrelationContext CreateCorrelationContext()
            {
                var correlationContext = new CorrelationContext();
                correlationContext.SetId("id");
                correlationContext.SetParentId("parentId");
                return correlationContext;
            }
        }
    }
}
