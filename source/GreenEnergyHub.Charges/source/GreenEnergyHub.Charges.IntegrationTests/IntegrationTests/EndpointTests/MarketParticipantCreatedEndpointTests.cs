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
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers.Actor;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.FunctionHost.MarketParticipant;
using GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.FunctionApp;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestCommon;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
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
                var message = CreateServiceBusMessage();

                // Act
                await MockTelemetryClient.WrappedOperationWithTelemetryDependencyInformationAsync(
                    () => Fixture.IntegrationEventTopic.SenderClient.SendMessageAsync(message), message.CorrelationId);

                await FunctionAsserts.AssertHasExecutedAsync(
                    Fixture.HostManager, nameof(MarketParticipantCreatedEndpoint)).ConfigureAwait(false);

                // Assert
                await using var readContext = Fixture.ChargesDatabaseManager.CreateDbContext();
                var marketParticipant = readContext.MarketParticipants.SingleOrDefault(x =>
                    x.MarketParticipantId == "1234567890123" && x.BusinessProcessRole == MarketParticipantRole.EnergySupplier);
                marketParticipant.Should().NotBeNull();
            }

            private static ServiceBusMessage CreateServiceBusMessage()
            {
                var actorCreatedIntegrationEvent = new ActorCreatedIntegrationEvent(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    ActorStatus.New,
                    "1234567890123",
                    "New actor",
                    new List<BusinessRoleCode>
                    {
                        BusinessRoleCode.Ddq,
                    },
                    new List<ActorMarketRole>
                    {
                        new(EicFunction.GridAccessProvider, new List<ActorGridArea>
                        {
                            new(Guid.NewGuid(), new[] { string.Empty }),
                        }),
                    },
                    DateTime.UtcNow);

                var correlationId = Guid.NewGuid().ToString("N");

                var actorCreatedIntegrationEventParser = new ActorCreatedIntegrationEventParser();
                var message = actorCreatedIntegrationEventParser.ParseToSharedIntegrationEvent(actorCreatedIntegrationEvent);

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
        }
    }
}
