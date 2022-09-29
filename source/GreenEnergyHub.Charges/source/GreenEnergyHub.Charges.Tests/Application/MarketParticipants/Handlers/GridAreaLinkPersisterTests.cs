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
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.Domain.Dtos.GridAreas;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.GridAreaLinks;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.TestHelpers;
using Microsoft.Extensions.Logging;
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
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<ILoggerFactory> loggerFactory,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            Mock<ILogger> logger)
        {
            // Arrange
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var gridAreaUpdatedEvent = new GridAreaUpdatedEvent(Guid.NewGuid(), Guid.NewGuid());

            SetupGridAreaRepositories(
                gridAreaLinkRepository,
                marketParticipantRepository,
                null,
                null);

            var sut = new GridAreaLinkPersister(
                gridAreaLinkRepository.Object,
                loggerFactory.Object,
                unitOfWork.Object);

            // Act
            await sut.PersistAsync(gridAreaUpdatedEvent).ConfigureAwait(false);

            // Assert
            gridAreaLinkRepository.Verify(v => v.AddAsync(It.IsAny<GridAreaLink>()), Times.Exactly(1));
            logger.VerifyLoggerWasCalled(
                $"GridAreaLink ID {gridAreaUpdatedEvent.GridAreaLinkId} for GridArea ID {gridAreaUpdatedEvent.GridAreaId} has been persisted",
                LogLevel.Information);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task PersistAsync_WhenCalledWithExistingGridAreaLinkAndDifferentGridAreaId_ShouldUpdateGridAreaId(
            [Frozen] Mock<IGridAreaLinkRepository> gridAreaLinkRepository,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<ILoggerFactory> loggerFactory,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            Mock<ILogger> logger)
        {
            // Arrange
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var gridAreaUpdatedEvent = new GridAreaUpdatedEvent(Guid.NewGuid(), Guid.NewGuid());

            var existingGridAreaLink = new GridAreaLink(gridAreaUpdatedEvent.GridAreaLinkId, Guid.NewGuid(), Guid.NewGuid());
            var marketParticipant = new MarketParticipant(
                id: Guid.NewGuid(),
                actorId: Guid.NewGuid(),
                b2CActorId: Guid.NewGuid(),
                string.Empty,
                MarketParticipantStatus.Active,
                MarketParticipantRole.GridAccessProvider);

            SetupGridAreaRepositories(
                gridAreaLinkRepository,
                marketParticipantRepository,
                existingGridAreaLink,
                marketParticipant);
            var sut = new GridAreaLinkPersister(
                gridAreaLinkRepository.Object,
                loggerFactory.Object,
                unitOfWork.Object);

            // Act
            await sut.PersistAsync(gridAreaUpdatedEvent).ConfigureAwait(false);

            // Assert
            gridAreaLinkRepository.Verify(v => v.AddAsync(It.IsAny<GridAreaLink>()), Times.Never);
            logger.VerifyLoggerWasCalled(
                $"GridAreaLink ID {gridAreaUpdatedEvent.GridAreaLinkId} with OwnerId {existingGridAreaLink.OwnerId} " +
                $"has changed GridArea ID to {gridAreaUpdatedEvent.GridAreaId}",
                LogLevel.Information);
            logger.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task PersistAsync_WhenCalledWithExistingGridAreaLinkAndSameGridAreaId_ShouldNotUpdateGridAreaId(
            [Frozen] Mock<IGridAreaLinkRepository> gridAreaLinkRepository,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<ILoggerFactory> loggerFactory,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            Mock<ILogger> logger)
        {
            // Arrange
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var gridAreaUpdatedEvent = new GridAreaUpdatedEvent(Guid.NewGuid(), Guid.NewGuid());

            var existingGridAreaLink = new GridAreaLink(gridAreaUpdatedEvent.GridAreaLinkId, gridAreaUpdatedEvent.GridAreaId, Guid.NewGuid());
            SetupGridAreaRepositories(
                gridAreaLinkRepository,
                marketParticipantRepository,
                existingGridAreaLink,
                new MarketParticipant(
                    id: Guid.NewGuid(),
                    actorId: Guid.NewGuid(),
                    b2CActorId: Guid.NewGuid(),
                    string.Empty,
                    MarketParticipantStatus.Active,
                    MarketParticipantRole.GridAccessProvider));
            var sut = new GridAreaLinkPersister(
                gridAreaLinkRepository.Object,
                loggerFactory.Object,
                unitOfWork.Object);

            // Act
            await sut.PersistAsync(gridAreaUpdatedEvent).ConfigureAwait(false);

            // Assert
            gridAreaLinkRepository.Verify(v => v.AddAsync(It.IsAny<GridAreaLink>()), Times.Never);
            logger.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task PersistAsync_WhenEventIsNull_ThrowsArgumentNullException(GridAreaLinkPersister sut)
        {
            // Arrange
            GridAreaUpdatedEvent? gridAreaChangedEvent = null;

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
