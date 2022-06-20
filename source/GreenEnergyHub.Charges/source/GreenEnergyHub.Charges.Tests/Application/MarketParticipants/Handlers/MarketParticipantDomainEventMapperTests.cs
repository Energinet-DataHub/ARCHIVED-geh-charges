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
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.MarketParticipants.Handlers;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.Application.MarketParticipants.Handlers
{
    public class MarketParticipantDomainEventMapperTests
    {
        [Fact]
        public void MapFromActorIntegrationEvent_ShouldReturnMarketParticipantUpdatedEvent()
        {
            // Arrange
            var actorId = Guid.NewGuid();
            var gln = "AnyGln";
            var roles = new List<BusinessRoleCode>() { BusinessRoleCode.Ddm };
            var grids = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var actorUpdatedIntegrationEvent = new ActorUpdatedIntegrationEvent(
                Guid.Empty,
                actorId,
                Guid.Empty,
                Guid.Empty,
                gln,
                ActorStatus.Active,
                roles,
                new List<EicFunction>(),
                grids,
                new List<string>());

            // Act
            var actualMarketParticipantUpdatedEvent =
                MarketParticipantDomainEventMapper.MapFromActorUpdatedIntegrationEvent(actorUpdatedIntegrationEvent);

            // Assert
            actualMarketParticipantUpdatedEvent.ActorId.Should().Be(actorId);
            actualMarketParticipantUpdatedEvent.MarketParticipantId.Should().Be(gln);
            actualMarketParticipantUpdatedEvent.IsActive.Should().Be(true);
            actualMarketParticipantUpdatedEvent.BusinessProcessRoles.Count.Should().Be(1);
            actualMarketParticipantUpdatedEvent.GridAreas.Count().Should().Be(3);
        }

        [Fact]
        public void MapFromActorIntegrationEvent_WhenSeveralRolesAssociated_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var actorId = Guid.NewGuid();
            var gln = "AnyGln";
            var roles = new List<BusinessRoleCode>() { BusinessRoleCode.Ddm, BusinessRoleCode.Ez };
            var grids = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var actorUpdatedIntegrationEvent = new ActorUpdatedIntegrationEvent(
                Guid.Empty,
                actorId,
                Guid.Empty,
                Guid.Empty,
                gln,
                ActorStatus.Active,
                roles,
                new List<EicFunction>(),
                grids,
                new List<string>());

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => MarketParticipantDomainEventMapper.MapFromActorUpdatedIntegrationEvent(actorUpdatedIntegrationEvent));
        }

        [Fact]
        public void MapFromGridAreaUpdatedIntegrationEvent_ShouldReturnGridAreaUpdatedEvent()
        {
            // Arrange
            var gridAreaId = Guid.NewGuid();
            var gridAreaLinkId = Guid.NewGuid();

            var gridAreaUpdatedIntegrationEvent = new GridAreaUpdatedIntegrationEvent(
                System.Guid.NewGuid(),
                gridAreaId,
                string.Empty,
                string.Empty,
                PriceAreaCode.DK1,
                gridAreaLinkId);

            // Act
            var actualGridAreaUpdatedEvent =
                MarketParticipantDomainEventMapper.MapFromGridAreaUpdatedIntegrationEvent(gridAreaUpdatedIntegrationEvent);

            // Assert
            actualGridAreaUpdatedEvent.GridAreaId.Should().Be(gridAreaId);
            actualGridAreaUpdatedEvent.GridAreaLinkId.Should().Be(gridAreaLinkId);
        }
    }
}
