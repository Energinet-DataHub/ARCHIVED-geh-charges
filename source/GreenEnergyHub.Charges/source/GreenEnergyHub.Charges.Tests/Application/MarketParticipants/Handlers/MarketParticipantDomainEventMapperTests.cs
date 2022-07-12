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
using GreenEnergyHub.TestHelpers;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.Application.MarketParticipants.Handlers
{
    public class MarketParticipantDomainEventMapperTests
    {
        [Theory]
        [AutoDomainData]
        public void MapFromActorIntegrationEvent_ShouldReturnMarketParticipantUpdatedEvent(
            Guid actorId, Guid b2CActorId, string actorNumber)
        {
            // Arrange
            var businessRoles = new List<BusinessRoleCode> { BusinessRoleCode.Ddm };

            var actorUpdatedIntegrationEvent = new ActorUpdatedIntegrationEvent(
                Guid.Empty,
                actorId,
                Guid.Empty,
                b2CActorId,
                actorNumber,
                ActorStatus.Active,
                businessRoles,
                CreateActorMarketRoles());

            // Act
            var actualMarketParticipantUpdatedEvent =
                MarketParticipantDomainEventMapper.MapFromActorUpdatedIntegrationEvent(actorUpdatedIntegrationEvent);

            // Assert
            actualMarketParticipantUpdatedEvent.ActorId.Should().Be(actorId);
            actualMarketParticipantUpdatedEvent.B2CActorId.Should().Be(b2CActorId);
            actualMarketParticipantUpdatedEvent.MarketParticipantId.Should().Be(actorNumber);
            actualMarketParticipantUpdatedEvent.IsActive.Should().Be(true);
            actualMarketParticipantUpdatedEvent.BusinessProcessRoles.Count.Should().Be(1);
            actualMarketParticipantUpdatedEvent.GridAreas.Count().Should().Be(3);
        }

        [Theory]
        [AutoDomainData]
        public void MapFromActorIntegrationEvent_WhenSeveralRolesAssociated_ShouldThrowInvalidOperationException(
            Guid actorId, Guid b2CActorId, string gln)
        {
            // Arrange
            var businessRoles = new List<BusinessRoleCode> { BusinessRoleCode.Ddm, BusinessRoleCode.Ez };
            var actorUpdatedIntegrationEvent = new ActorUpdatedIntegrationEvent(
                Guid.Empty,
                actorId,
                Guid.Empty,
                b2CActorId,
                gln,
                ActorStatus.Active,
                businessRoles,
                CreateActorMarketRoles());

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
                Guid.NewGuid(),
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
