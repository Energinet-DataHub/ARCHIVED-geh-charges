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
using GreenEnergyHub.Charges.TestCore.Data;
using GreenEnergyHub.Charges.TestCore.TestHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.EndpointTests.MarketParticipant
{
    [IntegrationTest]
    public class MarketParticipantStatusChangedEndpointTests
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
            public async Task When_ReceivingActorStatusChangedIntegrationEvent_NewStatusSaved()
            {
                // Arrange
                var actorId = SeededData.MarketParticipants.Provider8100000000030.Id;
                var message = CreateServiceBusMessage(actorId);
                await using var context = Fixture.ChargesDatabaseManager.CreateDbContext();

                // Act
                await MockTelemetryClient.WrappedOperationWithTelemetryDependencyInformationAsync(
                    () => Fixture.IntegrationEventTopic.SenderClient.SendMessageAsync(message), message.CorrelationId);

                await FunctionAsserts.AssertHasExecutedAsync(
                    Fixture.HostManager, nameof(MarketParticipantStatusChangedEndpoint)).ConfigureAwait(false);

                // Assert
                var marketParticipant = context.MarketParticipants.Single(x => x.ActorId == actorId);
                marketParticipant.Status.Should().Be(MarketParticipantStatus.Passive);
            }

            private static ServiceBusMessage CreateServiceBusMessage(Guid actorId)
            {
                var message = CreateMessage(actorId);

                var correlationId = Guid.NewGuid().ToString("N");

                return new ServiceBusMessage(message)
                {
                    CorrelationId = correlationId,
                    ApplicationProperties =
                    {
                        new KeyValuePair<string, object>(MessageMetaDataConstants.CorrelationId, correlationId),
                        new KeyValuePair<string, object>(MessageMetaDataConstants.MessageType, "ActorStatusChangedIntegrationEvent"),
                    },
                };
            }

            private static byte[] CreateMessage(Guid actorId)
            {
                var actorStatusChangedIntegrationEvent = new ActorStatusChangedIntegrationEvent(
                    Guid.NewGuid(),
                    InstantHelper.GetTodayAtMidnightUtc().ToDateTimeUtc(),
                    actorId,
                    Guid.NewGuid(),
                    ActorStatus.Passive);

                var actorStatusChangedIntegrationEventParser = new ActorStatusChangedIntegrationEventParser();
                return actorStatusChangedIntegrationEventParser.ParseToSharedIntegrationEvent(actorStatusChangedIntegrationEvent);
            }
        }
    }
}
