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
using System.Threading.Tasks;
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers;
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers.Actor;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.MarketParticipants.Handlers;
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.Domain.GridAreaLinks;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.FunctionHost.MarketParticipant;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures;
using GreenEnergyHub.Charges.TestCore.Builders.Testables;
using GreenEnergyHub.Charges.TestCore.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.MarketParticipantEndpoints
{
    [IntegrationTest]
    public class GridAreaOwnerRemovedEndpointTests
    {
        [Collection(nameof(MarketParticipantEndpointTestCollection))]
        public class RunAsync : IClassFixture<ChargesManagedDependenciesTestFixture>
        {
            private readonly ChargesManagedDependenciesTestFixture _fixture;
            private static readonly MarketParticipant _marketParticipant = new TestGridAccessProvider("1234567890");

            private static readonly GridAreaLink _gridAreaLink =
                new(Guid.NewGuid(), Guid.NewGuid(), _marketParticipant.Id);

            public RunAsync(ChargesManagedDependenciesTestFixture fixture)
            {
                _fixture = fixture;
            }

            [Fact]
            public async Task RunAsync_WhenEventHandled_GridAreaShouldHaveNoOwner()
            {
                // Arrange
                await using var setupContext = _fixture.ChargesDatabaseManager.CreateDbContext();
                setupContext.MarketParticipants.Add(_marketParticipant);
                setupContext.GridAreaLinks.Add(_gridAreaLink);
                await setupContext.SaveChangesAsync();
                var message = CreateMessage();
                var sut = new GridAreaOwnerRemovedEndpoint(
                    _fixture.GetService<ISharedIntegrationEventParser>(),
                    _fixture.GetService<IGridAreaOwnerRemovedCommandHandler>(),
                    _fixture.GetService<IChargesUnitOfWork>());

                // Act
                await sut.RunAsync(message);

                // Assert
                await using var context = _fixture.ChargesDatabaseManager.CreateDbContext();
                var gridAreaLink = await context.GridAreaLinks.SingleAsync(g =>
                    g.GridAreaId == _gridAreaLink.GridAreaId);
                gridAreaLink.OwnerId.Should().BeNull();
            }

            private static byte[] CreateMessage()
            {
                var gridAreaRemovedFromActorIntegrationEvent = new GridAreaRemovedFromActorIntegrationEvent(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    InstantHelper.GetTodayAtMidnightUtc().ToDateTimeUtc(),
                    EicFunction.GridAccessProvider,
                    _gridAreaLink.GridAreaId,
                    Guid.NewGuid());
                var parser = new GridAreaRemovedFromActorIntegrationEventParser();
                return parser.ParseToSharedIntegrationEvent(gridAreaRemovedFromActorIntegrationEvent);
            }
        }
    }
}
