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
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers;
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers.Actor;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.MarketParticipants.Handlers;
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.GridAreaLinks;
using GreenEnergyHub.Charges.FunctionHost.MarketParticipant;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures;
using GreenEnergyHub.Charges.TestCore.Builders.Testables;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.MarketParticipantEndpoints
{
    [IntegrationTest]
    public class MarketParticipantCreatedEndpointTests
    {
        [Collection(nameof(MarketParticipantEndpointTestCollection))]
        public class RunAsync : IClassFixture<ChargesManagedDependenciesTestFixture>, IAsyncLifetime
        {
            private readonly ChargesManagedDependenciesTestFixture _fixture;
            private static readonly TestGridAccessProvider _gridAccessProvider = new("1234567890");
            private static readonly GridAreaLink _gridAreaLink = new(Guid.NewGuid(), Guid.NewGuid(), null);

            public RunAsync(ChargesManagedDependenciesTestFixture fixture)
            {
                _fixture = fixture;
            }

            public async Task InitializeAsync()
            {
                await using var context = _fixture.ChargesDatabaseManager.CreateDbContext();
                context.GridAreaLinks.Add(_gridAreaLink);
                await context.SaveChangesAsync();
            }

            public async Task DisposeAsync()
            {
                await using var context = _fixture.ChargesDatabaseManager.CreateDbContext();
                context.GridAreaLinks.Remove(_gridAreaLink);
                await context.SaveChangesAsync();
            }

            [Fact]
            public async Task RunAsync_WhenReceivingActorIntegrationUpdatedMessage_MarketParticipantIsSavedToDatabase()
            {
                // Arrange
                var message = CreateMessage();
                var sut = new MarketParticipantCreatedEndpoint(
                    _fixture.GetService<ISharedIntegrationEventParser>(),
                    _fixture.GetService<IMarketParticipantCreatedCommandHandler>(),
                    _fixture.GetService<IUnitOfWork>());

                // Act
                await sut.RunAsync(message);

                // Assert
                await using var context = _fixture.ChargesDatabaseManager.CreateDbContext();
                var marketParticipant =
                    context.MarketParticipants.Single(x => x.ActorId == _gridAccessProvider.ActorId);
                marketParticipant.BusinessProcessRole.Should().Be(MarketParticipantRole.GridAccessProvider);
                marketParticipant.ActorId.Should().Be(_gridAccessProvider.ActorId);
                var gridAreaLink = await context.GridAreaLinks.SingleAsync(gal =>
                    gal.GridAreaId == _gridAreaLink.GridAreaId);
                gridAreaLink.OwnerId.Should().Be(marketParticipant.Id);
            }

            private static byte[] CreateMessage()
            {
                var actorCreatedIntegrationEvent = new ActorCreatedIntegrationEvent(
                    Guid.NewGuid(),
                    _gridAccessProvider.ActorId,
                    Guid.NewGuid(),
                    ActorStatus.New,
                    _gridAccessProvider.MarketParticipantId,
                    _gridAccessProvider.Name,
                    new List<BusinessRoleCode> { BusinessRoleCode.Ddm, },
                    new List<ActorMarketRole>
                    {
                        new(EicFunction.GridAccessProvider, new List<ActorGridArea> { new(_gridAreaLink.GridAreaId, new List<string>()), }),
                    },
                    DateTime.UtcNow);

                var actorCreatedIntegrationEventParser = new ActorCreatedIntegrationEventParser();
                return actorCreatedIntegrationEventParser.ParseToSharedIntegrationEvent(actorCreatedIntegrationEvent);
            }
        }
    }
}
