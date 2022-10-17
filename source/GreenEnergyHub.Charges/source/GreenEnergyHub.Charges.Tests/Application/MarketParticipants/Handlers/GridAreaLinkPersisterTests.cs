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
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Application.MarketParticipants.Handlers;
using GreenEnergyHub.Charges.Domain.Dtos.Events;
using GreenEnergyHub.Charges.Domain.GridAreaLinks;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Builders.Testables;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.MarketParticipants.Handlers
{
    [UnitTest]
    public class GridAreaPersisterTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task PersistAsync_WhenCalledWithNonExistentGridAreaLink_ShouldPersist(
            [Frozen] Mock<IGridAreaLinkRepository> gridAreaLinkRepository,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository)
        {
            // Arrange
            var gridAreaUpdatedEvent = new MarketParticipantGridAreaUpdatedCommand(
                Guid.NewGuid(),
                Guid.NewGuid());

            SetupGridAreaRepositories(
                gridAreaLinkRepository,
                marketParticipantRepository,
                null,
                null);

            var sut = new GridAreaLinkPersister(gridAreaLinkRepository.Object);

            // Act
            await sut.PersistAsync(gridAreaUpdatedEvent).ConfigureAwait(false);

            // Assert
            gridAreaLinkRepository.Verify(v => v.AddAsync(It.IsAny<GridAreaLink>()), Times.Exactly(1));
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task PersistAsync_WhenCalledWithExistingGridAreaLinkAndDifferentGridAreaId_ShouldUpdateGridAreaId(
            [Frozen] Mock<IGridAreaLinkRepository> gridAreaLinkRepository,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository)
        {
            // Arrange
            var gridAreaUpdatedEvent = new MarketParticipantGridAreaUpdatedCommand(Guid.NewGuid(), Guid.NewGuid());

            var existingGridAreaLink = new GridAreaLink(gridAreaUpdatedEvent.GridAreaLinkId, Guid.NewGuid(), Guid.NewGuid());
            var marketParticipant = new TestGridAccessProvider(string.Empty);

            SetupGridAreaRepositories(
                gridAreaLinkRepository,
                marketParticipantRepository,
                existingGridAreaLink,
                marketParticipant);
            var sut = new GridAreaLinkPersister(gridAreaLinkRepository.Object);

            // Act
            await sut.PersistAsync(gridAreaUpdatedEvent).ConfigureAwait(false);

            // Assert
            gridAreaLinkRepository.Verify(v => v.AddAsync(It.IsAny<GridAreaLink>()), Times.Never);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task PersistAsync_WhenCalledWithExistingGridAreaLinkAndSameGridAreaId_ShouldNotUpdateGridAreaId(
            [Frozen] Mock<IGridAreaLinkRepository> gridAreaLinkRepository,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository)
        {
            // Arrange
            var gridAreaUpdatedEvent = new MarketParticipantGridAreaUpdatedCommand(Guid.NewGuid(), Guid.NewGuid());

            var existingGridAreaLink = new GridAreaLink(gridAreaUpdatedEvent.GridAreaLinkId, gridAreaUpdatedEvent.GridAreaId, Guid.NewGuid());
            SetupGridAreaRepositories(
                gridAreaLinkRepository,
                marketParticipantRepository,
                existingGridAreaLink,
                new TestGridAccessProvider(string.Empty));
            var sut = new GridAreaLinkPersister(gridAreaLinkRepository.Object);

            // Act
            await sut.PersistAsync(gridAreaUpdatedEvent).ConfigureAwait(false);

            // Assert
            gridAreaLinkRepository.Verify(v => v.AddAsync(It.IsAny<GridAreaLink>()), Times.Never);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task PersistAsync_WhenEventIsNull_ThrowsArgumentNullException(GridAreaLinkPersister sut)
        {
            // Arrange
            MarketParticipantGridAreaUpdatedCommand? gridAreaChangedEvent = null;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => sut.PersistAsync(gridAreaChangedEvent!))
                .ConfigureAwait(false);
        }

        private static void SetupGridAreaRepositories(
            Mock<IGridAreaLinkRepository> gridAreaLinkRepository,
            Mock<IMarketParticipantRepository> marketParticipantRepository,
            GridAreaLink? gridAreaLink,
            MarketParticipant? marketParticipant)
        {
            gridAreaLinkRepository.Setup(x => x.GetOrNullAsync(It.IsAny<Guid>()))
                .ReturnsAsync(gridAreaLink);
            marketParticipantRepository.Setup(x => x.GetGridAccessProviderAsync(It.IsAny<Guid>()))
                .ReturnsAsync(marketParticipant);
        }
    }
}
