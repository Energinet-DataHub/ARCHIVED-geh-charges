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
using AutoFixture.Xunit2;
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers;
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers.Actor;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.MarketParticipants.Handlers;
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.FunctionHost.MarketParticipant;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.TestCore.Builders.Testables;
using GreenEnergyHub.Charges.TestCore.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.MarketParticipantEndpoints
{
    [IntegrationTest]
    public class MarketParticipantB2CActorIdChangedEndpointTests
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

            [Theory]
            [InlineAutoData("664359b9-f6cc-45d4-9c93-ec35248e5f95")]
            [InlineAutoData(null)]
            public async Task When_ReceivingExternalActorIdChangedIntegrationEvent_NewMarketParticipantB2CActorIdSaved(
                Guid newExternalId)
            {
                // Arrange
                var message = CreateMessage(newExternalId);
                await using var context = _fixture.DatabaseManager.CreateDbContext();
                var sut = new MarketParticipantB2CActorIdChangedEndpoint(
                    _fixture.GetService<ISharedIntegrationEventParser>(),
                    _fixture.GetService<IMarketParticipantB2CActorIdChangedCommandHandler>(),
                    _fixture.GetService<IUnitOfWork>());

                // Act
                await sut.RunAsync(message);

                // Assert
                var marketParticipant = context.MarketParticipants
                    .Single(x => x.ActorId == _marketParticipant.ActorId);
                marketParticipant.B2CActorId.Should().Be(newExternalId);
            }

            private static byte[] CreateMessage(Guid externalId)
            {
                var actorExternalIdChangedIntegrationEvent = new ActorExternalIdChangedIntegrationEvent(
                    Guid.NewGuid(),
                    InstantHelper.GetTodayAtMidnightUtc().ToDateTimeUtc(),
                    _marketParticipant.ActorId,
                    Guid.NewGuid(),
                    externalId);

                var actorExternalIdChangedIntegrationEventParser = new ActorExternalIdChangedIntegrationEventParser();
                return actorExternalIdChangedIntegrationEventParser.ParseToSharedIntegrationEvent(
                    actorExternalIdChangedIntegrationEvent);
            }
        }
    }
}
