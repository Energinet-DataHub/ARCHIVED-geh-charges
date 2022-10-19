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
using AutoFixture.Xunit2;
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
    public class MarketParticipantB2CActorIdChangedEndpointTests
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

            [Theory]
            [InlineAutoData("664359b9-f6cc-45d4-9c93-ec35248e5f95")]
            [InlineAutoData(null)]
            public async Task When_ReceivingActorCreatedIntegrationEvent_MarketParticipantIsSavedToDatabase(Guid externalId)
            {
                // Arrange
                var actorId = Guid.NewGuid();
                var message = CreateServiceBusMessage(actorId, externalId);
                await using var context = Fixture.ChargesDatabaseManager.CreateDbContext();

                // Act
                await MockTelemetryClient.WrappedOperationWithTelemetryDependencyInformationAsync(
                    () => Fixture.IntegrationEventTopic.SenderClient.SendMessageAsync(message), message.CorrelationId);

                await FunctionAsserts.AssertHasExecutedAsync(
                    Fixture.HostManager, nameof(MarketParticipantB2CActorIdChangedEndpoint)).ConfigureAwait(false);

                // Assert
                var marketParticipant = context.MarketParticipants.Single(x => x.ActorId == actorId);
                marketParticipant.B2CActorId.Should().Be(externalId);
            }

            private static ServiceBusMessage CreateServiceBusMessage(Guid actorId, Guid? externalId)
            {
                var message = CreateMessage(actorId, externalId);

                var correlationId = Guid.NewGuid().ToString("N");

                return new ServiceBusMessage(message)
                {
                    CorrelationId = correlationId,
                    ApplicationProperties =
                    {
                        new KeyValuePair<string, object>(MessageMetaDataConstants.CorrelationId, correlationId),
                        new KeyValuePair<string, object>(MessageMetaDataConstants.MessageType, "ActorExternalIdChangedIntegrationEvent"),
                    },
                };
            }

            private static byte[] CreateMessage(Guid actorId, Guid? externalId)
            {
                var actorExternalIdChangedIntegrationEvent = new ActorExternalIdChangedIntegrationEvent(
                    Guid.NewGuid(),
                    InstantHelper.GetTodayAtMidnightUtc().ToDateTimeUtc(),
                    actorId,
                    Guid.NewGuid(),
                    externalId);

                var actorExternalIdChangedIntegrationEventParser = new ActorExternalIdChangedIntegrationEventParser();
                return actorExternalIdChangedIntegrationEventParser.ParseToSharedIntegrationEvent(actorExternalIdChangedIntegrationEvent);
            }
        }
    }
}
