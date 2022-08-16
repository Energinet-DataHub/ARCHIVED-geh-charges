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
using AutoFixture.Xunit2;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using FluentAssertions;
using FluentAssertions.Common;
using GreenEnergyHub.Charges.FunctionHost.MessageHub;
using GreenEnergyHub.Charges.Infrastructure.Outbox;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.FunctionApp;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestCommon;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.Json;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using IClock = NodaTime.IClock;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.EndpointTests
{
    [IntegrationTest]
    public class OutboxProcessorEndpointTests
    {
        [Collection(nameof(ChargesFunctionAppCollectionFixture))]
        public class RunAsync : FunctionAppTestBase<ChargesFunctionAppFixture>, IAsyncLifetime
        {
            public RunAsync(
                ChargesFunctionAppFixture fixture,
                ITestOutputHelper testOutputHelper)
                : base(fixture, testOutputHelper)
            {
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

            [Theory]
            [InlineAutoMoqData]
            public async Task When_OutboxProcessor_ReadsAnOutboxMessage_PersistsAvailableData(
                [Frozen] Mock<ICorrelationContext> correlationContext,
                ICorrelationContext context,
                JsonSerializer jsonSerializer,
                IClock clock,
                OperationsRejectedEventBuilder builder,
                ChargePriceCommandBuilder priceCommandBuilder,
                ChargePriceOperationDtoBuilder operationDtoBuilder)
            {
                // Arrange
                await using var chargesDatabaseContext = Fixture.ChargesDatabaseManager.CreateDbContext();
                await using var messageHubDatabaseContext = Fixture.MessageHubDatabaseManager.CreateDbContext();
                var correlationId = CorrelationIdGenerator.Create();
                correlationContext.Setup(x => x.AsTraceContext()).Returns(correlationId);
                var chargePriceDto = operationDtoBuilder.WithPoint(1.00m).Build();
                var chargePriceCommand = priceCommandBuilder.WithChargeOperation(chargePriceDto).Build();
                var rejectEvent = builder.WithChargeCommand(chargePriceCommand).Build();
                var factory = new OutboxMessageFactory(jsonSerializer, clock, context);
                var outboxMessage = factory.CreateFrom(rejectEvent);
                chargesDatabaseContext.OutboxMessages.Add(outboxMessage);
                await chargesDatabaseContext.SaveChangesAsync();

                // Act
                await FunctionAsserts.AssertHasExecutedAsync(
                    Fixture.HostManager, nameof(OutboxProcessorEndpoint));

                // Assert
                var actualAvailableDataReceipt = await messageHubDatabaseContext.AvailableChargeReceiptData.FirstAsync();
                actualAvailableDataReceipt.RecipientId.Should().Be(rejectEvent.Command.Document.Sender.MarketParticipantId);
            }
        }
    }
}
