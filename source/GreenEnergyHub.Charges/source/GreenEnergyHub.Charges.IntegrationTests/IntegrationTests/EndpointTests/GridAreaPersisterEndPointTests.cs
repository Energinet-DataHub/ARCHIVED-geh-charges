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
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.FunctionHost.MarketParticipant;
using GreenEnergyHub.Charges.Infrastructure.Persistence;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.FunctionApp;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestCommon;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Nito.Disposables.Internals;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.EndpointTests
{
    [IntegrationTest]
    public class GridAreaPersisterEndPointTests
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
                return Task.CompletedTask;
            }

            [Fact]
            public async Task When_ReceivingGridAreaIntegrationUpdatedMessage_GridAreaIsSavedToDatabase()
            {
                // Arrange
                await using var context = Fixture.DatabaseManager.CreateDbContext();
                var id = Guid.NewGuid();
                var gridAccessProviderId = await CreateMarketParticipantInRepository(context);
                var (message, parentId) = CreateServiceBusMessage(id, gridAccessProviderId);

                // Act
                await MockTelemetryClient.WrappedOperationWithTelemetryDependencyInformationAsync(
                    () => Fixture.MarketParticipantChangedTopic.SenderClient.SendMessageAsync(message), message.CorrelationId, parentId);

                // Assert
                await FunctionAsserts.AssertHasExecutedAsync(Fixture.HostManager, nameof(MarketParticipantEndpoint)).ConfigureAwait(false);
                var gridArea = context.GridAreas.SingleOrDefault(x =>
                    x.Id == id && x.GridAccessProviderId == gridAccessProviderId);
                gridArea.Should().NotBeNull();

                // We need to clear host log after each test is done to ensure that we can assert on function executed on each test run because we only check on function name.
                Fixture.HostManager.ClearHostLog();
            }

            private static async Task<Guid> CreateMarketParticipantInRepository(ChargesDatabaseContext context)
            {
                var markedParticipant = new MarketParticipantBuilder().Build();
                context.MarketParticipants.Add(markedParticipant);
                await context.SaveChangesAsync().ConfigureAwait(false);
                return markedParticipant.Id;
            }

            private static (ServiceBusMessage ServiceBusMessage, string ParentId)
                CreateServiceBusMessage(Guid id, Guid gridAreaId)
            {
                var gridAreaIntegrationEvent = new GridAreaUpdatedIntegrationEvent(
                    id,
                    gridAreaId,
                    "name",
                    "code",
                    PriceAreaCode.DK1,
                    Guid.NewGuid());
                var gridAreaUpdatedIntegrationEventParser = new GridAreaUpdatedIntegrationEventParser();
                var message = gridAreaUpdatedIntegrationEventParser.Parse(gridAreaIntegrationEvent);

                var correlationId = CorrelationIdGenerator.Create();
                var serviceBusMessage = new ServiceBusMessage(message)
                {
                    CorrelationId = correlationId,
                };
                var parentId = $"00-{correlationId}-b7ad6b7169203333-01";
                return (serviceBusMessage, parentId);
            }
        }
    }
}
