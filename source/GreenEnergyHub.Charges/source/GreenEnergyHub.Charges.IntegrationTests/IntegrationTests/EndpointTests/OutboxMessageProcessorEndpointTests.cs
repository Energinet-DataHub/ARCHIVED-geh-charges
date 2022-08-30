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
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Charges.Events;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Messages.Events;
using GreenEnergyHub.Charges.FunctionHost.Charges.MessageHub;
using GreenEnergyHub.Charges.FunctionHost.MessageHub;
using GreenEnergyHub.Charges.Infrastructure.Outbox;
using GreenEnergyHub.Charges.Infrastructure.Persistence;
using GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.FunctionApp;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestCommon;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
using GreenEnergyHub.Charges.TestCore.TestHelpers;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using Microsoft.Azure.Functions.Worker;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.EndpointTests
{
    [IntegrationTest]
    public class OutboxMessageProcessorEndpointTests
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
            public async Task RunAsync_WhenRejectedOutboxMessageIsRead_AvailableDataIsPersisted_AndProcessedDateIsSet()
            {
                // Arrange
                await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
                await using var messageHubDatabaseContext = Fixture.MessageHubDatabaseManager.CreateDbContext();
                await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();
                var operationsRejectedEvent = CreateChargePriceOperationsRejectedEvent();
                var outboxMessage = await PersistToOutboxMessage(chargesDatabaseWriteContext, operationsRejectedEvent);

                // Act
                await FunctionAsserts.AssertHasExecutedAsync(
                    Fixture.HostManager, nameof(ChargePriceRejectedDataAvailableNotifierEndpoint));

                // Assert
                var actualAvailableDataReceipt = messageHubDatabaseContext.AvailableChargeReceiptData
                    .Single(x => x.OriginalOperationId == operationsRejectedEvent.Operations.First().OperationId);
                actualAvailableDataReceipt.RecipientId.Should().Be(operationsRejectedEvent.Document.Sender.MarketParticipantId);
                var processedOutboxMessage = chargesDatabaseReadContext.OutboxMessages.Single(x => x.Id == outboxMessage.Id);
                processedOutboxMessage.ProcessedDate.Should().NotBeNull();
            }

            [Fact]
            public async Task RunAsync_WhenConfirmedOutboxMessageIsRead_AvailableDataIsPersisted_AndProcessedDateIsSet()
            {
                // Arrange
                await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
                await using var messageHubDatabaseContext = Fixture.MessageHubDatabaseManager.CreateDbContext();
                await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();
                var operationsConfirmedEvent = CreateChargePriceOperationsConfirmedEvent();
                var outboxMessage = await PersistToOutboxMessage(chargesDatabaseWriteContext, operationsConfirmedEvent);

                // Act
                await FunctionAsserts.AssertHasExecutedAsync(
                    Fixture.HostManager, nameof(ChargePriceConfirmedDataAvailableNotifierEndpoint));

                // Assert
                var actualAvailableDataReceipt = messageHubDatabaseContext.AvailableChargeReceiptData
                    .Single(x => x.OriginalOperationId == operationsConfirmedEvent.Operations.First().OperationId);
                actualAvailableDataReceipt.RecipientId.Should().Be(operationsConfirmedEvent.Document.Sender.MarketParticipantId);
                var processedOutboxMessage = chargesDatabaseReadContext.OutboxMessages.Single(x => x.Id == outboxMessage.Id);
                processedOutboxMessage.ProcessedDate.Should().NotBeNull();
            }

            [Theory]
            [InlineAutoMoqData]
            public async Task GivenOutputProcessorEndpoint_WhenFailsFirstAttempt_ThenRetryNext(
                [Frozen] Mock<IClock> clock,
                Mock<IInternalEventDispatcher<InternalEvent>> dispatcher,
                Mock<IOutboxMessageParser> outboxMessageParser,
                PriceConfirmedEvent confirmedEvent,
                TimerInfo timerInfo,
                CorrelationContext correlationContext,
                Instant now)
            {
                // Arrange
                await using var chargesDatabaseWriteContext = _databaseManager.CreateDbContext();
                await using var chargesDatabaseReadContext = _databaseManager.CreateDbContext();

                clock.Setup(c => c.GetCurrentInstant()).Returns(now);
                outboxMessageParser
                    .Setup(o => o.Parse(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(confirmedEvent);
                var operationsRejectedEvent = CreateChargePriceOperationsRejectedEvent();
                var outboxMessage = await PersistToOutboxMessage(chargesDatabaseWriteContext, operationsRejectedEvent);

                var unitOfWork = new UnitOfWork(chargesDatabaseWriteContext);
                var outboxRepository = new OutboxMessageRepository(chargesDatabaseWriteContext);
                var sut = new OutboxMessageProcessorEndpoint(
                    outboxRepository,
                    outboxMessageParser.Object,
                    clock.Object,
                    correlationContext,
                    dispatcher.Object,
                    unitOfWork);

                // Act & Assert
                dispatcher.Setup(d => d.DispatchAsync(
                        It.IsAny<PriceRejectedEvent>(),
                        It.IsAny<CancellationToken>())).Throws<Exception>();

                await Assert.ThrowsAsync<Exception>(() => sut.RunAsync(timerInfo));

                dispatcher.Setup(d => d.DispatchAsync(
                        It.IsAny<PriceRejectedEvent>(),
                        It.IsAny<CancellationToken>()))
                    .Callback<PriceRejectedEvent, CancellationToken>((_, _) => { });
                await sut.RunAsync(timerInfo);

                outboxMessage = chargesDatabaseReadContext.OutboxMessages.Single(x => x.Id == outboxMessage.Id);
                outboxMessage.ProcessedDate.Should().Be(now);
            }

            private static PriceRejectedEvent CreateChargePriceOperationsRejectedEvent()
            {
                var chargePriceOperation =
                    new ChargePriceOperationDtoBuilder()
                        .WithChargePriceOperationId(Guid.NewGuid().ToString())
                        .Build();
                var operations = new List<ChargePriceOperationDto> { chargePriceOperation };
                var operationsRejectedEvent = new PriceRejectedEventBuilder()
                    .WithOperations(operations)
                    .Build();
                return operationsRejectedEvent;
            }

            private static PriceConfirmedEvent CreateChargePriceOperationsConfirmedEvent()
            {
                var chargePriceOperation =
                    new ChargePriceOperationDtoBuilder()
                        .WithChargePriceOperationId(Guid.NewGuid().ToString())
                        .Build();
                var operations = new List<ChargePriceOperationDto> { chargePriceOperation };
                var operationsConfirmedEvent = new PriceConfirmedEventBuilder()
                    .WithOperations(operations)
                    .Build();
                return operationsConfirmedEvent;
            }

            private static async Task<OutboxMessage> PersistToOutboxMessage<T>(ChargesDatabaseContext context, T domainEvent)
            {
                var correlationContext = CorrelationContextGenerator.Create();
                var jsonSerializer = new JsonSerializer();
                var outboxMessageFactory = new OutboxMessageFactory(jsonSerializer, SystemClock.Instance, correlationContext);
                var outboxMessage = outboxMessageFactory.CreateFrom(domainEvent);
                context.OutboxMessages.Add(outboxMessage);
                await context.SaveChangesAsync();
                return outboxMessage;
            }
        }
    }
}
