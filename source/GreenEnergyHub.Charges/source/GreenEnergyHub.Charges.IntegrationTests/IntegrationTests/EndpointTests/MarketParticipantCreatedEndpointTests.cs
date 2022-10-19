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
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers;
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers.Actor;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.MarketParticipants.Handlers;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.FunctionHost.MarketParticipant;
using GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData;
using GreenEnergyHub.Charges.Infrastructure.Persistence;
using GreenEnergyHub.Charges.Infrastructure.Persistence.Repositories;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.FunctionApp;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestCommon;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
using GreenEnergyHub.Charges.TestCore;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.EndpointTests
{
    [IntegrationTest]
    public class MarketParticipantCreatedEndpointTests
    {
        [Collection(nameof(ChargesFunctionAppCollectionFixture))]
        public class RunAsync : FunctionAppTestBase<ChargesFunctionAppFixture>, IAsyncLifetime
        {
            public RunAsync(ChargesFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
                : base(fixture, testOutputHelper)
            {
            }

            public Task InitializeAsync()
            {
                return Task.CompletedTask;
            }

            public Task DisposeAsync()
            {
                Fixture.HostManager.ClearHostLog();
                return Task.CompletedTask;
            }

            [Fact]
            public async Task When_ReceivingActorIntegrationUpdatedMessage_MarketParticipantIsSavedToDatabase()
            {
                // Arrange
                var actorId = Guid.NewGuid();
                var message = CreateServiceBusMessage("9876543210", actorId, SeededData.GridAreaLink.Provider8100000000030.GridAreaId);
                await using var context = Fixture.ChargesDatabaseManager.CreateDbContext();

                // Act
                await MockTelemetryClient.WrappedOperationWithTelemetryDependencyInformationAsync(
                    () => Fixture.IntegrationEventTopic.SenderClient.SendMessageAsync(message), message.CorrelationId);

                await FunctionAsserts.AssertHasExecutedAsync(
                    Fixture.HostManager, nameof(MarketParticipantCreatedEndpoint)).ConfigureAwait(false);

                // Assert
                var marketParticipant = context.MarketParticipants.Single(x => x.ActorId == actorId);
                marketParticipant.BusinessProcessRole.Should().Be(MarketParticipantRole.GridAccessProvider);
                marketParticipant.ActorId.Should().Be(actorId);
            }

            [Fact]
            public async Task RunAsync_WhenActorCreatedEventHandled_MarketParticipantSavedAndGridAreaLinksUpdated()
            {
                // Arrange
                var actorId = Guid.NewGuid();
                var message = CreateMessage("0123456789", actorId, SeededData.GridAreaLink.Provider8500000000013.GridAreaId);
                await using var writeContext = Fixture.ChargesDatabaseManager.CreateDbContext();
                var sharedIntegrationEventParser = new SharedIntegrationEventParser();
                var marketParticipantRepository = new MarketParticipantRepository(writeContext);
                var gridAreaLinkRepository = new GridAreaLinkRepository(writeContext);
                var unitOfWork = new UnitOfWork(writeContext);

                var marketParticipantCreatedCommandHandler = new MarketParticipantCreatedCommandHandler(
                    marketParticipantRepository,
                    gridAreaLinkRepository);

                var sut = new MarketParticipantCreatedEndpoint(
                    sharedIntegrationEventParser,
                    marketParticipantCreatedCommandHandler,
                    unitOfWork);

                // Act
                await sut.RunAsync(message);

                // Assert
                await using var readContext = Fixture.ChargesDatabaseManager.CreateDbContext();
                var marketParticipant = await readContext.MarketParticipants.SingleAsync(mp =>
                    mp.ActorId == actorId);
                var actual = await readContext.GridAreaLinks.SingleAsync(gal =>
                    gal.GridAreaId == SeededData.GridAreaLink.Provider8500000000013.GridAreaId);
                actual.OwnerId.Should().Be(marketParticipant.Id);
            }

            private static ServiceBusMessage CreateServiceBusMessage(string actorNumber, Guid actorId, Guid gridAreaId)
            {
                var message = CreateMessage(actorNumber, actorId, gridAreaId);

                var correlationId = Guid.NewGuid().ToString("N");

                return new ServiceBusMessage(message)
                {
                    CorrelationId = correlationId,
                    ApplicationProperties =
                    {
                        new KeyValuePair<string, object>(MessageMetaDataConstants.CorrelationId, correlationId),
                        new KeyValuePair<string, object>(MessageMetaDataConstants.MessageType, "ActorCreatedIntegrationEvent"),
                    },
                };
            }

            private static byte[] CreateMessage(string actorNumber, Guid actorId, Guid gridAreaId)
            {
                var actorCreatedIntegrationEvent = new ActorCreatedIntegrationEvent(
                    Guid.NewGuid(),
                    actorId,
                    Guid.NewGuid(),
                    ActorStatus.New,
                    actorNumber,
                    "New actor",
                    new List<BusinessRoleCode>
                    {
                        BusinessRoleCode.Ddm,
                    },
                    new List<ActorMarketRole>
                    {
                        new(EicFunction.GridAccessProvider, new List<ActorGridArea>
                        {
                            new(gridAreaId, new List<string>()),
                        }),
                    },
                    DateTime.UtcNow);

                var actorCreatedIntegrationEventParser = new ActorCreatedIntegrationEventParser();
                return actorCreatedIntegrationEventParser.ParseToSharedIntegrationEvent(actorCreatedIntegrationEvent);
            }
        }
    }
}
