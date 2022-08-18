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
using AutoFixture.Xunit2;
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
using GreenEnergyHub.Charges.TestCore.TestHelpers;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.Json;
using Microsoft.Azure.Functions.Worker;
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
            public async Task GivenOutboxProcessorEndpoint_WhenOutboxMessageIsRead_AvailableDataIsPersisted()
            {
                // Arrange
                await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
                await using var messageHubDatabaseContext = Fixture.MessageHubDatabaseManager.CreateDbContext();
                await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();
                var operationsRejectedEvent = CreateChargePriceOperationsRejectedEvent();
                var outboxMessage = await PersistToOutboxMessage(chargesDatabaseWriteContext, operationsRejectedEvent);

                // Act
                await FunctionAsserts.AssertHasExecutedAsync(
                    Fixture.HostManager, nameof(OutboxMessageProcessorEndpoint));

                // Assert
                var actualAvailableDataReceipt = messageHubDatabaseContext.AvailableChargeReceiptData
                    .Single(x => x.OriginalOperationId == operationsRejectedEvent.Command.Operations.First().OperationId);
                actualAvailableDataReceipt.RecipientId.Should().Be(operationsRejectedEvent.Command.Document.Sender.MarketParticipantId);
                var processedOutboxMessage = chargesDatabaseReadContext.OutboxMessages.Single(x => x.Id == outboxMessage.Id);
                processedOutboxMessage.ProcessedDate.Should().NotBeNull();
            }

            [Theory]
            [InlineAutoMoqData]
            public async Task GivenOutputProcessorEndpoint_WhenFailsFirstAttempt_ThenRetryNext(
                Mock<IAvailableDataNotifier<AvailableChargeReceiptData, ChargePriceOperationsRejectedEvent>> availableDataNotifier,
                [Frozen] Mock<IClock> clock,
                JsonSerializer jsonSerializer,
                TimerInfo timerInfo,
                CorrelationContext correlationContext,
                Instant now)
            {
                // Arrange
                await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
                await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();
                var unitOfWork = new UnitOfWork(chargesDatabaseWriteContext);
                clock.Setup(c => c.GetCurrentInstant()).Returns(now);
                var outboxMessage = CreateOutboxMessage(now);
                var outboxRepository = new OutboxMessageRepository(chargesDatabaseWriteContext);
                chargesDatabaseWriteContext.OutboxMessages.Add(outboxMessage);
                await chargesDatabaseWriteContext.SaveChangesAsync();
                var sut = new OutboxMessageProcessorEndpoint(
                    outboxRepository,
                    availableDataNotifier.Object,
                    jsonSerializer,
                    clock.Object,
                    correlationContext,
                    unitOfWork);

                // Act & Assert
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

                outboxMessage = chargesDatabaseReadContext.OutboxMessages.Single(x => x.Id == outboxMessage.Id);
                outboxMessage.ProcessedDate.Should().Be(now);
            }

            private static ChargePriceOperationsRejectedEvent CreateChargePriceOperationsRejectedEvent()
            {
                var chargePriceOperation =
                    new ChargePriceOperationDtoBuilder().WithChargePriceOperationId(Guid.NewGuid().ToString()).Build();
                var chargeCommand = new ChargePriceCommandBuilder().WithChargeOperation(chargePriceOperation).Build();
                var operationsRejectedEvent = new ChargePriceOperationsRejectedEventBuilder().WithChargeCommand(chargeCommand).Build();
                return operationsRejectedEvent;
            }

            private static async Task<OutboxMessage> PersistToOutboxMessage(ChargesDatabaseContext context, ChargePriceOperationsRejectedEvent operationsRejectedEvent)
            {
                var correlationContext = CorrelationContextGenerator.Create();
                var jsonSerializer = new JsonSerializer();
                var outboxMessageFactory = new OutboxMessageFactory(jsonSerializer, SystemClock.Instance, correlationContext);
                var outboxMessage = outboxMessageFactory.CreateFrom(operationsRejectedEvent);
                context.OutboxMessages.Add(outboxMessage);
                await context.SaveChangesAsync();
                return outboxMessage;
            }

            private static OutboxMessage CreateOutboxMessage(Instant now)
            {
                var jsonSerializer = new JsonSerializer();
                var rejectedEvent = new ChargePriceOperationsRejectedEventBuilder().Build();
                var data = jsonSerializer.Serialize(rejectedEvent);
                return new OutboxMessageBuilder()
                    .WithData(data)
                    .WithType(rejectedEvent.GetType().ToString())
                    .WithCreationDate(now)
                    .Build();
            }
        }
    }
}
