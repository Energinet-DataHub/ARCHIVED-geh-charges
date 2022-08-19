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
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers.GridArea;
using FluentAssertions;
using GreenEnergyHub.Charges.FunctionHost.MarketParticipant;
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
    public class GridAreaLinkPersisterEndPointTests
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
            public async Task When_ReceivingGridAreaIntegrationUpdatedMessage_GridAreaLinkIsSavedToDatabase()
            {
                // Arrange
                await using var context = Fixture.ChargesDatabaseManager.CreateDbContext();
                var id = Guid.NewGuid();
                var (message, parentId) = CreateServiceBusMessage(id);

                // Act
                await MockTelemetryClient.WrappedOperationWithTelemetryDependencyInformationAsync(
                    () => Fixture.MarketParticipantChangedTopic.SenderClient.SendMessageAsync(message), message.CorrelationId, parentId);

                // Assert
                await FunctionAsserts.AssertHasExecutedAsync(Fixture.HostManager, nameof(MarketParticipantPersisterEndpoint)).ConfigureAwait(false);
                var gridAreaLink = context.GridAreaLinks.SingleOrDefault(x => x.Id == id);
                gridAreaLink.Should().NotBeNull();

                // We need to clear host log after each test is done to ensure that we can assert on function executed on each test run because we only check on function name.
                Fixture.HostManager.ClearHostLog();
            }

            private static (ServiceBusMessage ServiceBusMessage, string ParentId)
                CreateServiceBusMessage(Guid id)
            {
                var gridAreaIntegrationEvent = new GridAreaUpdatedIntegrationEvent(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "name",
                    "code",
                    PriceAreaCode.DK1,
                    id);
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
