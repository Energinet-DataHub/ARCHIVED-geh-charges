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
using AutoFixture.Xunit2;
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers.Actor;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.MarketParticipants.Handlers.Mappers;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.TestHelpers;
using NodaTime;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.Application.MarketParticipants.Handlers
{
    public class ActorIntegrationEventMapperTests
    {
        [Theory]
        [AutoDomainData]
        public void MapFromActorIntegrationEvent_ShouldReturnMarketParticipantUpdatedCommand(
            Guid actorId, Guid b2CActorId, string actorNumber)
        {
            // Arrange
            var businessRoles = new List<BusinessRoleCode> { BusinessRoleCode.Ddm };

            var actorUpdatedIntegrationEvent = new ActorUpdatedIntegrationEvent(
                Guid.Empty,
                DateTime.UtcNow,
                actorId,
                Guid.Empty,
                b2CActorId,
                actorNumber,
                ActorStatus.Active,
                businessRoles,
                CreateActorMarketRoles());

            // Act
            var actualMarketParticipantUpdatedEvent =
                ActorIntegrationEventMapper.MapFromActorUpdated(actorUpdatedIntegrationEvent);

            // Assert
            actualMarketParticipantUpdatedEvent.ActorId.Should().Be(actorId);
            actualMarketParticipantUpdatedEvent.B2CActorId.Should().Be(b2CActorId);
            actualMarketParticipantUpdatedEvent.MarketParticipantId.Should().Be(actorNumber);
            actualMarketParticipantUpdatedEvent.Status.Should().Be(MarketParticipantStatus.Active);
            actualMarketParticipantUpdatedEvent.BusinessProcessRoles.Count().Should().Be(1);
            actualMarketParticipantUpdatedEvent.GridAreas.Count().Should().Be(2);
        }

        [Fact]
        public void MapFromGridAreaUpdatedIntegrationEvent_ShouldReturnMarketParticipantGridAreaUpdatedCommand()
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
                ActorIntegrationEventMapper.MapFromGridAreaUpdated(gridAreaUpdatedIntegrationEvent);

            // Assert
            actualGridAreaUpdatedEvent.GridAreaId.Should().Be(gridAreaId);
            actualGridAreaUpdatedEvent.GridAreaLinkId.Should().Be(gridAreaLinkId);
        }

        [Theory]
        [AutoData]
        public void MapFromActorCreated_ShouldReturnMarketParticipantCreatedCommand(
            Guid eventId,
            Guid actorId,
            Guid orgId,
            string actorNumber,
            string name,
            DateTime eventCreated,
            ActorCreatedIntegrationEventParser parser,
            ActorIntegrationEventMapper sut)
        {
            var actorCreatedIntegrationEvent = new ActorCreatedIntegrationEvent(
                eventId,
                actorId,
                orgId,
                ActorStatus.Active,
                actorNumber,
                name,
                new List<BusinessRoleCode> { BusinessRoleCode.Ddq },
                new List<ActorMarketRole> { new(EicFunction.EnergySupplier, new List<ActorGridArea>()) },
                eventCreated);
            var bytes = parser.ParseToSharedIntegrationEvent(actorCreatedIntegrationEvent);

            // Act
            var actual = sut.MapFromActorCreated(bytes);

            // Assert
            actual.ActorId.Should().Be(actorId);
            actual.Status.Should().Be(MarketParticipantStatus.Active);
            actual.BusinessProcessRoles.Should().Contain(bpr => bpr == MarketParticipantRole.EnergySupplier);
            actual.BusinessProcessRoles.Should().Contain(bpr => bpr == MarketParticipantRole.EnergySupplier);
        }

        private static IEnumerable<ActorMarketRole> CreateActorMarketRoles()
        {
            var gridAreaIdOne = Guid.NewGuid();
            var gridAreaIdTwo = Guid.NewGuid();

            return new List<ActorMarketRole>
            {
                new(EicFunction.GridAccessProvider, new List<ActorGridArea>
                {
                    CreateActorGridArea(gridAreaIdOne),
                    CreateActorGridArea(gridAreaIdTwo),
                }),
                new(EicFunction.MeteredDataAdministrator, new List<ActorGridArea>
                {
                    CreateActorGridArea(gridAreaIdTwo),
                }),
            };
        }

        private static ActorGridArea CreateActorGridArea(Guid id)
        {
            return new ActorGridArea(id, new List<string>());
        }
    }
}
