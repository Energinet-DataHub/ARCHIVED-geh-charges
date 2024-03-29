﻿// Copyright 2020 Energinet DataHub A/S
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
using AutoFixture.Xunit2;
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.MarketParticipants.Handlers.Mappers;
using GreenEnergyHub.Charges.Domain.Dtos.Events;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.TestCore.TestHelpers;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.Application.MarketParticipants.Handlers
{
    public class MarketParticipantIntegrationEventMapperTests
    {
        [Fact]
        public void MapFromActorCreated_ShouldReturnMarketParticipantCreatedCommand()
        {
            var actorCreatedIntegrationEvent = new ActorCreatedIntegrationEvent(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                ActorStatus.Active,
                "actorNumber",
                "name",
                new List<BusinessRoleCode> { BusinessRoleCode.Ddq },
                new List<ActorMarketRole> { new(EicFunction.EnergySupplier, new List<ActorGridArea>()) },
                InstantHelper.GetTodayAtMidnightUtc().ToDateTimeUtc());

            // Act
            var actual = MarketParticipantIntegrationEventMapper.Map(actorCreatedIntegrationEvent);

            // Assert
            actual.ActorId.Should().Be(actorCreatedIntegrationEvent.ActorId);
            actual.MarketParticipantId.Should().Be(actorCreatedIntegrationEvent.ActorNumber);
            actual.Status.Should().Be(MarketParticipantStatus.Active);
            actual.GridAreas.Should().BeEmpty();
            actual.BusinessProcessRoles.Should().ContainSingle(bpr => bpr == MarketParticipantRole.EnergySupplier);
        }

        [Theory]
        [InlineData(ActorStatus.Active, MarketParticipantStatus.Active)]
        [InlineData(ActorStatus.Inactive, MarketParticipantStatus.Inactive)]
        [InlineData(ActorStatus.Deleted, MarketParticipantStatus.Deleted)]
        [InlineData(ActorStatus.New, MarketParticipantStatus.New)]
        [InlineData(ActorStatus.Passive, MarketParticipantStatus.Passive)]
        public void MapFromActorStatusChanged_ShouldReturnMarketParticipantStatusChangedCommand(
            ActorStatus actorStatus,
            MarketParticipantStatus expectedStatus)
        {
            // Arrange
            var actorStatusChanged = new ActorStatusChangedIntegrationEvent(
                Guid.NewGuid(),
                InstantHelper.GetTodayAtMidnightUtc().ToDateTimeUtc(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                actorStatus);

            // Act
            var actual = MarketParticipantIntegrationEventMapper.Map(actorStatusChanged);

            // Assert
            actual.ActorId.Should().Be(actorStatusChanged.ActorId);
            actual.Status.Should().Be(expectedStatus);
        }

        [Theory]
        [InlineAutoData("664359b9-f6cc-45d4-9c93-ec35248e5f95")]
        [InlineAutoData(null)]
        public void MapFromActorExternalIdChanged_ShouldReturnMarketParticipantB2CActorIdChangedCommand(Guid externalId)
        {
            // Arrange
            var externalIdChanged = new ActorExternalIdChangedIntegrationEvent(
                Guid.NewGuid(),
                InstantHelper.GetTodayAtMidnightUtc().ToDateTimeUtc(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                externalId);

            // Act
            var actual = MarketParticipantIntegrationEventMapper.Map(externalIdChanged);

            // Assert
            actual.ActorId.Should().Be(actual.ActorId);
            actual.B2CActorId.Should().Be(externalId);
        }

        [Theory]
        [AutoData]
        public void Map_GridAreaAddedToActorIntegrationEvent_ShouldReturnGridAreaOwnerAddedCommand(
            Guid actorId,
            Guid gridAreaId)
        {
            // Arrange
            var gridAreaAddedEvent = new GridAreaAddedToActorIntegrationEvent(
                Guid.NewGuid(),
                actorId,
                Guid.NewGuid(),
                InstantHelper.GetTodayAtMidnightUtc().ToDateTimeUtc(),
                EicFunction.GridAccessProvider,
                gridAreaId,
                Guid.NewGuid());

            // Act
            var actual = MarketParticipantIntegrationEventMapper.Map(gridAreaAddedEvent);

            // Assert
            actual.Should().BeOfType(typeof(GridAreaOwnerAddedCommand));
            actual.ActorId.Should().Be(actorId);
            actual.GridAreaId.Should().Be(gridAreaId);
        }

        [Theory]
        [AutoData]
        public void Map_GridAreaRemovedFromActorIntegrationEvent_ShouldReturnGridAreaOwnerRemovedCommand(
            Guid gridAreaId)
        {
            // Arrange
            var gridAreaRemovedEvent = new GridAreaRemovedFromActorIntegrationEvent(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                InstantHelper.GetTodayAtMidnightUtc().ToDateTimeUtc(),
                EicFunction.GridAccessProvider,
                gridAreaId,
                Guid.NewGuid());

            // Act
            var actual = MarketParticipantIntegrationEventMapper.Map(gridAreaRemovedEvent);

            // Assert
            actual.Should().BeOfType(typeof(GridAreaOwnerRemovedCommand));
            actual.GridAreaId.Should().Be(gridAreaId);
        }

        [Fact]
        public void MapFromActorNameChangedIntegrationEvent_ShouldReturnMarketParticipantNameChangedCommand()
        {
            // Arrange
            var actorId = Guid.NewGuid();
            var name = "some name";
            var actorNameChangedIntegrationEvent =
                new ActorNameChangedIntegrationEvent(Guid.NewGuid(), DateTime.Now, actorId, Guid.NewGuid(), name);

            // Act
            var actual = MarketParticipantIntegrationEventMapper.Map(actorNameChangedIntegrationEvent);

            // Assert
            actual.ActorId.Should().Be(actorId);
            actual.Name.Should().Be(name);
        }
    }
}
