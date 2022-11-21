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
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.FunctionHost.MarketParticipant;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures;
using GreenEnergyHub.Charges.TestCore.Builders.Testables;
using GreenEnergyHub.Charges.TestCore.TestHelpers;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.MarketParticipantEndpoints
{
    [IntegrationTest]
    public class MarketParticipantStatusChangedEndpointTests
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
                await using var context = _fixture.ChargesDatabaseManager.CreateDbContext();
                context.MarketParticipants.Add(_marketParticipant);
                await context.SaveChangesAsync();
            }

            public async Task DisposeAsync()
            {
                await using var context = _fixture.ChargesDatabaseManager.CreateDbContext();
                context.MarketParticipants.Remove(_marketParticipant);
                await context.SaveChangesAsync();
            }

            [Theory]
            [InlineAutoData(ActorStatus.New, MarketParticipantStatus.New)]
            [InlineAutoData(ActorStatus.Active, MarketParticipantStatus.Active)]
            [InlineAutoData(ActorStatus.Inactive, MarketParticipantStatus.Inactive)]
            [InlineAutoData(ActorStatus.Passive, MarketParticipantStatus.Passive)]
            [InlineAutoData(ActorStatus.Deleted, MarketParticipantStatus.Deleted)]
            public async Task When_ReceivingActorStatusChangedIntegrationEvent_NewStatusSaved(
                ActorStatus newStatus,
                MarketParticipantStatus expectedStatus)
            {
                // Arrange
                var message = CreateMessage(newStatus);
                await using var context = _fixture.ChargesDatabaseManager.CreateDbContext();
                var sut = new MarketParticipantStatusChangedEndpoint(
                    _fixture.GetService<ISharedIntegrationEventParser>(),
                    _fixture.GetService<IMarketParticipantStatusChangedCommandHandler>(),
                    _fixture.GetService<IChargesUnitOfWork>());

                // Act
                await sut.RunAsync(message);

                // Assert
                var marketParticipant = context.MarketParticipants
                    .Single(x => x.ActorId == _marketParticipant.ActorId);
                marketParticipant.Status.Should().Be(expectedStatus);
            }

            private static byte[] CreateMessage(ActorStatus status)
            {
                var actorStatusChangedIntegrationEvent = new ActorStatusChangedIntegrationEvent(
                    Guid.NewGuid(),
                    InstantHelper.GetTodayAtMidnightUtc().ToDateTimeUtc(),
                    _marketParticipant.ActorId,
                    Guid.NewGuid(),
                    status);

                var actorStatusChangedIntegrationEventParser = new ActorStatusChangedIntegrationEventParser();
                return actorStatusChangedIntegrationEventParser.ParseToSharedIntegrationEvent(
                    actorStatusChangedIntegrationEvent);
            }
        }
    }
}
