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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Domain.Dtos.Events;
using GreenEnergyHub.Charges.FunctionHost.MessageHub;
using GreenEnergyHub.Charges.Infrastructure.Outbox;
using GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using GreenEnergyHub.Charges.TestCore.TestHelpers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.ChargeInformation
{
    [IntegrationTest]
    public class OutboxMessageProcessorEndpointTests
    {
        public class RunAsync : IClassFixture<ChargesManagedDependenciesTestFixture>
        {
            private readonly ChargesManagedDependenciesTestFixture _fixture;
            private readonly ICorrelationContext _correlationContext;

            public RunAsync(ChargesManagedDependenciesTestFixture fixture)
            {
                _fixture = fixture;
                _correlationContext = _fixture.CorrelationContext;
                _correlationContext.SetId("testCorrelationId");
            }

            [Theory]
            [AutoMoqData]
            public async Task RunAsync_WhenRejectedOutboxMessageIsRead_ThenDomainEventRaised_AndProcessedDateIsSet(
                [Frozen] Mock<IDomainEventDispatcher> domainEventDispatcher,
                TimerInfo timerInfo,
                ChargePriceOperationsRejectedEventBuilder builder)
            {
                // Arrange
                var domainEvent = builder.Build();
                var outboxMessage = await CreateStoredOutboxMessage(domainEvent);

                var outboxProcessor = new OutboxMessageProcessorEndpoint(
                    _fixture.GetService<IOutboxMessageRepository>(),
                    _fixture.GetService<IOutboxMessageParser>(),
                    _fixture.GetService<IClock>(),
                    _correlationContext,
                    domainEventDispatcher.Object,
                    _fixture.UnitOfWork,
                    _fixture.GetService<ILoggerFactory>());

                // Act
                await outboxProcessor.RunAsync(timerInfo);

                // Assert
                domainEventDispatcher.Verify(d => d.DispatchAsync(It.IsAny<DomainEvent>()), Times.Once());
                var writeContext = _fixture.DatabaseManager.CreateDbContext();
                var actual = await writeContext.OutboxMessages.SingleAsync(om =>
                    om.Id == outboxMessage.Id);
                actual.ProcessedDate.Should().NotBeNull();
            }

            [Theory]
            [InlineAutoMoqData]
            public async Task RunAsync_WhenConfirmedOutboxMessageIsRead_ThenDomainEventRaised_AndProcessedDateIsSet(
                TimerInfo timerInfo,
                Mock<IDomainEventDispatcher> domainEventDispatcher,
                ChargePriceOperationsAcceptedEventBuilder chargePriceOperationsAcceptedEventBuilder)
            {
                // Arrange
                var domainEvent = chargePriceOperationsAcceptedEventBuilder.Build();
                var outboxMessage = await CreateStoredOutboxMessage(domainEvent);

                var sut = new OutboxMessageProcessorEndpoint(
                    _fixture.GetService<IOutboxMessageRepository>(),
                    _fixture.GetService<IOutboxMessageParser>(),
                    _fixture.GetService<IClock>(),
                    _correlationContext,
                    domainEventDispatcher.Object,
                    _fixture.UnitOfWork,
                    _fixture.GetService<ILoggerFactory>());

                // Act
                await sut.RunAsync(timerInfo);

                // Assert
                var readContext = _fixture.DatabaseManager.CreateDbContext();
                var actual = await readContext.OutboxMessages
                    .SingleAsync(x => x.Id == outboxMessage.Id);
                actual.Should().BeEquivalentTo(outboxMessage, om => om.Excluding(p => p.ProcessedDate));
                outboxMessage.ProcessedDate.Should().NotBeNull();
            }

            [Theory]
            [InlineAutoMoqData]
            public async Task GivenOutputProcessorEndpoint_WhenFailsFirstAttempt_ThenRetryNext(
                [Frozen] Mock<IDomainEventDispatcher> dispatcher,
                ChargePriceOperationsRejectedEventBuilder chargePriceOperationsRejectedEventBuilder,
                TimerInfo timerInfo)
            {
                // Arrange
                await using var writeContext = _fixture.DatabaseManager.CreateDbContext();

                var operationsRejectedEvent = chargePriceOperationsRejectedEventBuilder.Build();
                var outboxMessage = await CreateStoredOutboxMessage(operationsRejectedEvent);

                var sut = new OutboxMessageProcessorEndpoint(
                    _fixture.GetService<IOutboxMessageRepository>(),
                    _fixture.GetService<IOutboxMessageParser>(),
                    _fixture.GetService<IClock>(),
                    _correlationContext,
                    dispatcher.Object,
                    _fixture.UnitOfWork,
                    _fixture.GetService<ILoggerFactory>());

                // Act & Assert
                dispatcher
                    .Setup(d => d.DispatchAsync(It.IsAny<DomainEvent>()))
                    .Throws<Exception>();

                await Assert.ThrowsAsync<Exception>(() => sut.RunAsync(timerInfo));
                var actual = await FetchOutboxMessageAsync(outboxMessage.Id);
                actual.ProcessedDate.Should().BeNull();

                dispatcher
                    .Setup(d => d.DispatchAsync(It.IsAny<DomainEvent>()))
                    .Returns(Task.CompletedTask);

                await sut.RunAsync(timerInfo);
                actual = await FetchOutboxMessageAsync(outboxMessage.Id);
                actual.ProcessedDate.Should().NotBeNull();
            }

            private async Task<OutboxMessage> FetchOutboxMessageAsync(Guid outboxMessageId)
            {
                await using var readContext = _fixture.DatabaseManager.CreateDbContext();
                return await readContext.OutboxMessages.SingleAsync(x => x.Id == outboxMessageId);
            }

            private async Task<OutboxMessage> CreateStoredOutboxMessage(DomainEvent domainEvent)
            {
                var outboxMessage = CreateOutboxMessage(domainEvent);
                await using var chargesDatabaseWriteContext = _fixture.DatabaseManager.CreateDbContext();
                await chargesDatabaseWriteContext.OutboxMessages.AddAsync(outboxMessage);
                await chargesDatabaseWriteContext.SaveChangesAsync();
                return outboxMessage;
            }

            private OutboxMessage CreateOutboxMessage<T>([DisallowNull] T domainEvent)
            {
                var jsonSerializer = _fixture.GetService<IJsonSerializer>();
                var data = jsonSerializer.Serialize(domainEvent);

                return OutboxMessage.Create(
                    data,
                    _correlationContext.Id,
                    domainEvent.GetType().ToString(),
                    InstantHelper.GetTodayAtMidnightUtc());
            }
        }
    }
}
