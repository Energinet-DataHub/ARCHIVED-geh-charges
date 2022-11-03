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
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers;
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers.Actor;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.MarketParticipants.Handlers;
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.FunctionHost.MarketParticipant;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures;
using GreenEnergyHub.Charges.TestCore.Builders.Testables;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.MarketParticipantEndpoints
{
    [IntegrationTest]
    public class MarketParticipantNameChangedEndpointTests
    {
        [Collection(nameof(MarketParticipantEndpointTestCollection))]
        public class RunAsync : IClassFixture<ChargesManagedDependenciesTestFixture>, IAsyncLifetime
        {
            private readonly ChargesManagedDependenciesTestFixture _fixture;
            private static readonly TestEnergySupplier _marketParticipant = new("1234567890");

            public RunAsync(ChargesManagedDependenciesTestFixture fixture)
            {
                _fixture = fixture;
            }

            public async Task InitializeAsync()
            {
                await using var context = _fixture.DatabaseManager.CreateDbContext();
                context.MarketParticipants.Add(_marketParticipant);
                await context.SaveChangesAsync();
            }

            public async Task DisposeAsync()
            {
                await using var context = _fixture.DatabaseManager.CreateDbContext();
                context.MarketParticipants.Remove(_marketParticipant);
                await context.SaveChangesAsync();
            }

            [Fact]
            public async Task When_ReceivingMarketParticipantNameChangedEvent_NameIsUpdatedInTheDatabase()
            {
                // Arrange
                const string marketParticipantName = "super new name";
                var message = CreateMessage(marketParticipantName);
                var sut = new MarketParticipantNameChangedEndpoint(
                    _fixture.GetService<ISharedIntegrationEventParser>(),
                    _fixture.GetService<IMarketParticipantNameChangedCommandHandler>(),
                    _fixture.GetService<IUnitOfWork>());

                // Act
                await sut.RunAsync(message);

                // Assert
                await using var context = _fixture.DatabaseManager.CreateDbContext();
                var marketParticipant = context.MarketParticipants
                    .Single(x => x.ActorId == _marketParticipant.ActorId);
                marketParticipant.Name.Should().Be(marketParticipantName);
            }

            private static byte[] CreateMessage(string name)
            {
                var actorNameChangedIntegrationEvent = new ActorNameChangedIntegrationEvent(
                    Guid.NewGuid(),
                    new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    _marketParticipant.ActorId,
                    Guid.NewGuid(),
                    name);

                var actorNameChangedIntegrationEventParser = new ActorNameChangedIntegrationEventParser();
                return actorNameChangedIntegrationEventParser.ParseToSharedIntegrationEvent(
                    actorNameChangedIntegrationEvent);
            }
        }
    }
}
