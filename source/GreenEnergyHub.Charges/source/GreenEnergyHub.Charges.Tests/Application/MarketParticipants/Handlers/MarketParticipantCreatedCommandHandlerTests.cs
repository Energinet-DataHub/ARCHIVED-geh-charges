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
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.MarketParticipants.Handlers;
using GreenEnergyHub.Charges.Domain.Dtos.Events;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.GridAreaLinks;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.MarketParticipants.Handlers
{
    [UnitTest]
    public class MarketParticipantCreatedCommandHandlerTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_ValidEvent_ShouldAddNewMarketParticipant(
                [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
                MarketParticipantCreatedCommandHandler sut)
        {
            // Arrange
            var marketParticipantCreatedEvent = new MarketParticipantCreatedCommand(
                ActorId: Guid.NewGuid(),
                "mp123",
                "name",
                new List<MarketParticipantRole> { MarketParticipantRole.EnergySupplier, },
                MarketParticipantStatus.Active,
                new List<Guid>());
            MarketParticipant marketParticipant = null!;
            marketParticipantRepository.Setup(m => m.AddAsync(It.IsAny<MarketParticipant>()))
                .Callback<MarketParticipant>(m => marketParticipant = m);

            // Act
            await sut.HandleAsync(marketParticipantCreatedEvent);

            // Assert
            marketParticipant.Should().NotBeNull();
            marketParticipant.ActorId.Should().Be(marketParticipantCreatedEvent.ActorId);
            marketParticipant.B2CActorId.Should().BeNull();
            marketParticipant.MarketParticipantId.Should().Be(marketParticipantCreatedEvent.MarketParticipantId);
            marketParticipant.BusinessProcessRole.Should().Be(MarketParticipantRole.EnergySupplier);
            marketParticipant.Status.Should().Be(MarketParticipantStatus.Active);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenGridAccessProvider_ShouldAddMarketParticipantAsGridAreaOwner(
            Guid gridAreaId,
            GridAreaLink gridAreaLink,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IGridAreaLinkRepository> gridAreaLinkRepository,
            MarketParticipantCreatedCommandHandler sut)
        {
            // Arrange
            var marketParticipantCreatedEvent = new MarketParticipantCreatedCommand(
                ActorId: Guid.NewGuid(),
                "mp123",
                "name",
                new List<MarketParticipantRole> { MarketParticipantRole.GridAccessProvider, },
                MarketParticipantStatus.Active,
                new List<Guid> { gridAreaId });

            MarketParticipant marketParticipant = null!;
            marketParticipantRepository
                .Setup(m => m.AddAsync(It.IsAny<MarketParticipant>()))
                .Callback<MarketParticipant>(m => marketParticipant = m);
            gridAreaLinkRepository
                .Setup(g => g.GetGridAreaOrNullAsync(gridAreaId))
                .ReturnsAsync(gridAreaLink);

            await sut.HandleAsync(marketParticipantCreatedEvent);

            gridAreaLink.OwnerId.Should().Be(marketParticipant.Id);
        }
    }
}
