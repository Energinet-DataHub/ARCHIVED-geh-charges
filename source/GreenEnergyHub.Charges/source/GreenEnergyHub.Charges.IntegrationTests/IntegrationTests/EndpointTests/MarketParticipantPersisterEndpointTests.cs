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
using GreenEnergyHub.Charges.Application.MarketParticipants.Handlers;
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
    public class MarketParticipantPersisterEndpointTests
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

            [Theory]
            [InlineData(BusinessRoleCode.Ddm, ActorStatus.Inactive)]
            public async Task When_ReceivingActorIntegrationUpdatedMessage_MarketParticipantIsSavedToDatabase(
                BusinessRoleCode businessRoleCode, ActorStatus actorStatus)
            {
                // Arrange
                const string gln = "1234567890123";
                var role = MarketParticipantRoleMapper.Map(businessRoleCode);
                var (message, parentId) = CreateServiceBusMessage(gln, actorStatus, new List<BusinessRoleCode> { businessRoleCode });

                await using var context = Fixture.ChargesDatabaseManager.CreateDbContext();

                // Act
                await MockTelemetryClient.WrappedOperationWithTelemetryDependencyInformationAsync(
                    () => Fixture.MarketParticipantChangedTopic.SenderClient.SendMessageAsync(message), message.CorrelationId, parentId);

                // Assert
                await FunctionAsserts.AssertHasExecutedAsync(
                    Fixture.HostManager, nameof(MarketParticipantPersisterEndpoint)).ConfigureAwait(false);
                var marketParticipant = context.MarketParticipants.SingleOrDefault(x =>
                    x.MarketParticipantId == gln && x.BusinessProcessRole == role);
                marketParticipant.Should().NotBeNull();

                // We need to clear host log after each test is done to ensure that we can assert on function executed on each test run because we only check on function name
                Fixture.HostManager.ClearHostLog();
            }

            private static (ServiceBusMessage ServiceBusMessage, string ParentId) CreateServiceBusMessage(
                string actorNumber,
                ActorStatus status,
                IEnumerable<BusinessRoleCode> businessRoles)
            {
                var actorUpdatedIntegrationEvent = new ActorUpdatedIntegrationEvent(
                    Guid.NewGuid(),
                    DateTime.UtcNow,
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    actorNumber,
                    status,
                    businessRoles,
                    CreateActorMarketRoles());

                var actorUpdatedIntegrationEventParser = new ActorUpdatedIntegrationEventParser();
                var message = actorUpdatedIntegrationEventParser.Parse(actorUpdatedIntegrationEvent);

                var correlationId = CorrelationIdGenerator.Create();
                var parentId = $"00-{correlationId}-b7ad6b7169203332-01";
                var serviceBusMessage = new ServiceBusMessage(message)
                {
                    CorrelationId = correlationId,
                };
                return (serviceBusMessage, parentId);
            }

            private static IEnumerable<ActorMarketRole> CreateActorMarketRoles()
            {
                return new List<ActorMarketRole>
                {
                    new(EicFunction.GridAccessProvider, new List<ActorGridArea>
                    {
                        new(Guid.NewGuid(), new[] { string.Empty }),
                    }),
                };
            }
        }
    }
}
