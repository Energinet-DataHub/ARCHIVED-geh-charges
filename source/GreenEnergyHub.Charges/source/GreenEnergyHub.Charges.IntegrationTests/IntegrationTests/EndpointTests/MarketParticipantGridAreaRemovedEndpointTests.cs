﻿// Copyright 2020 Energinet DataHub A/S
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
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers.Actor;
using FluentAssertions;
using GreenEnergyHub.Charges.FunctionHost.MarketParticipant;
using GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData;
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
    public class MarketParticipantGridAreaRemovedEndpointTests
    {
        [Collection(nameof(ChargesFunctionAppCollectionFixture))]
        public class RunAsync : FunctionAppTestBase<ChargesFunctionAppFixture>
        {
            public RunAsync(ChargesFunctionAppFixture fixture, ITestOutputHelper testOutputHelper)
                : base(fixture, testOutputHelper)
            {
            }

            [Fact]
            public async Task RunAsync_WhenValidGridAreaRemovedFromMarketParticipantEventHandled_GridAreaShouldHaveNoOwner()
            {
                // Arrange
                var serviceBusMessage = CreateServiceBusMessage();

                // Act
                await MockTelemetryClient.WrappedOperationWithTelemetryDependencyInformationAsync(
                    () => Fixture.IntegrationEventTopic.SenderClient.SendMessageAsync(serviceBusMessage), serviceBusMessage.CorrelationId);

                await FunctionAsserts.AssertHasExecutedAsync(
                    Fixture.HostManager, nameof(GridAreaOwnerRemovedEndpoint)).ConfigureAwait(false);

                // Assert
                await using var context = Fixture.ChargesDatabaseManager.CreateDbContext();
                var gridAreaLink = await context.GridAreaLinks.SingleAsync(g =>
                    g.GridAreaId == SeededData.GridAreaLink.Provider8100000000030.GridAreaId);
                gridAreaLink.OwnerId.Should().BeNull();
            }

            private static ServiceBusMessage CreateServiceBusMessage()
            {
                var gridAreaRemovedFromActorIntegrationEvent = new GridAreaRemovedFromActorIntegrationEvent(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    InstantHelper.GetTodayAtMidnightUtc().ToDateTimeUtc(),
                    EicFunction.GridAccessProvider,
                    SeededData.GridAreaLink.Provider8100000000030.GridAreaId,
                    Guid.NewGuid());
                var gridAreaRemovedFromActorIntegrationEventParser = new GridAreaRemovedFromActorIntegrationEventParser();
                var message = gridAreaRemovedFromActorIntegrationEventParser.ParseToSharedIntegrationEvent(
                    gridAreaRemovedFromActorIntegrationEvent);
                var correlationId = Guid.NewGuid().ToString("N");
                var serviceBusMessage = new ServiceBusMessage(message)
                {
                    CorrelationId = correlationId,
                    ApplicationProperties =
                    {
                        new KeyValuePair<string, object>(
                            MessageMetaDataConstants.CorrelationId,
                            correlationId),
                        new KeyValuePair<string, object>(
                            MessageMetaDataConstants.MessageType,
                            "GridAreaRemovedFromActorIntegrationEvent"),
                    },
                };
                return serviceBusMessage;
            }
        }
    }
}
