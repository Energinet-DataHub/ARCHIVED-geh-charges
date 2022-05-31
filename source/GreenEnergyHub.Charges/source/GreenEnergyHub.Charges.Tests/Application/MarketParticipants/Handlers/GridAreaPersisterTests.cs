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
using GreenEnergyHub.Charges.Domain.Dtos.MarketParticipantsChangedEvents;
using GreenEnergyHub.Charges.Domain.GridAreas;
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
        public async Task PersistAsync_WhenCalledWithNonExistentGridArea_ShouldPersist(
            [Frozen] Mock<IGridAreaRepository> gridAreaRepository,
            [Frozen] Mock<IGridAreaLinkRepository> gridAreaLinkRepository,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<ILoggerFactory> loggerFactory,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            Mock<ILogger> logger)
        {
            // Arrange
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var gridAreaChangedEvent = new GridAreaChangedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            SetupGridAreaRepositories(
                gridAreaRepository,
                gridAreaLinkRepository,
                marketParticipantRepository,
                null,
                null,
                null);

            var sut = new GridAreaPersister(
                gridAreaRepository.Object,
                gridAreaLinkRepository.Object,
                loggerFactory.Object,
                unitOfWork.Object);

            // Act
            await sut.PersistAsync(gridAreaChangedEvent).ConfigureAwait(false);

            // Assert
            gridAreaRepository.Verify(v => v.AddAsync(It.IsAny<GridArea>()), Times.Exactly(1));
            gridAreaLinkRepository.Verify(v => v.AddAsync(It.IsAny<GridAreaLink>()), Times.Exactly(1));
            logger.VerifyLoggerWasCalled(
                $"GridArea ID {gridAreaChangedEvent.GridAreaId} has been persisted",
                LogLevel.Information);
            logger.VerifyLoggerWasCalled(
                $"GridAreaLink ID {gridAreaChangedEvent.GridAreaLinkId} for GridArea ID {gridAreaChangedEvent.GridAreaId} has been persisted",
                LogLevel.Information);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task PersistAsync_WhenCalledWithExistingGridArea_ShouldUpdateGridAreaLink(
            [Frozen] Mock<IGridAreaRepository> gridAreaRepository,
            [Frozen] Mock<IGridAreaLinkRepository> gridAreaLinkRepository,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<ILoggerFactory> loggerFactory,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            Mock<ILogger> logger)
        {
            // Arrange
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var gridAreaChangedEvent = new GridAreaChangedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            var existingGridArea = new GridArea(gridAreaChangedEvent.GridAreaId, null);
            var existingGridAreaLink = new GridAreaLink(gridAreaChangedEvent.GridAreaLinkId, existingGridArea.Id);
            SetupGridAreaRepositories(
                gridAreaRepository,
                gridAreaLinkRepository,
                marketParticipantRepository,
                existingGridArea,
                existingGridAreaLink,
                null);
            var sut = new GridAreaPersister(
                gridAreaRepository.Object,
                gridAreaLinkRepository.Object,
                loggerFactory.Object,
                unitOfWork.Object);

            // Act
            await sut.PersistAsync(gridAreaChangedEvent).ConfigureAwait(false);

            // Assert
            gridAreaRepository.Verify(v => v.AddAsync(It.IsAny<GridArea>()), Times.Never);
            gridAreaLinkRepository.Verify(v => v.AddAsync(It.IsAny<GridAreaLink>()), Times.Never);
            logger.VerifyLoggerWasCalled(
                $"GridAreaLink ID {gridAreaChangedEvent.GridAreaLinkId} has changed GridArea ID to {gridAreaChangedEvent.GridAreaId}",
                LogLevel.Information);
            logger.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task PersistAsync_WhenCalledWithExistingGridArea_ShouldUpdateExistingGridAreaLinkGridAreaId(
            [Frozen] Mock<IGridAreaRepository> gridAreaRepository,
            [Frozen] Mock<IGridAreaLinkRepository> gridAreaLinkRepository,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<ILoggerFactory> loggerFactory,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            Mock<ILogger> logger)
        {
            // Arrange
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var gridAreaChangedEvent = new GridAreaChangedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            var existingGridArea = new GridArea(gridAreaChangedEvent.GridAreaId, Guid.NewGuid());
            var existingGridAreaLink = new GridAreaLink(gridAreaChangedEvent.GridAreaLinkId, Guid.NewGuid());
            var gridAccessProviderId = existingGridArea.GridAccessProviderId ?? Guid.Empty;
            var marketParticipantId = existingGridArea.GridAccessProviderId.ToString() ?? string.Empty;
            var existingMarketParticipant = new MarketParticipant(
                gridAccessProviderId,
                marketParticipantId,
                true,
                MarketParticipantRole.GridAccessProvider);

            SetupGridAreaRepositories(
                gridAreaRepository,
                gridAreaLinkRepository,
                marketParticipantRepository,
                existingGridArea,
                existingGridAreaLink,
                existingMarketParticipant);
            var sut = new GridAreaPersister(
                gridAreaRepository.Object,
                gridAreaLinkRepository.Object,
                loggerFactory.Object,
                unitOfWork.Object);

            // Act
            await sut.PersistAsync(gridAreaChangedEvent).ConfigureAwait(false);

            // Assert
            gridAreaRepository.Verify(v => v.AddAsync(It.IsAny<GridArea>()), Times.Never);
            gridAreaLinkRepository.Verify(v => v.AddAsync(It.IsAny<GridAreaLink>()), Times.Never);
            logger.VerifyLoggerWasCalled(
                $"GridAreaLink ID {gridAreaChangedEvent.GridAreaLinkId} has changed GridArea ID to {gridAreaChangedEvent.GridAreaId}",
                LogLevel.Information);
            logger.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task PersistAsync_WhenEventIsNull_ThrowsArgumentNullException(GridAreaPersister sut)
        {
            // Arrange
            GridAreaChangedEvent? gridAreaChangedEvent = null;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => sut.PersistAsync(gridAreaChangedEvent!))
                .ConfigureAwait(false);
        }

        private static void SetupGridAreaRepositories(
            Mock<IGridAreaRepository> gridAreaRepository,
            Mock<IGridAreaLinkRepository> gridAreaLinkRepository,
            Mock<IMarketParticipantRepository> marketParticipantRepository,
            GridArea? gridArea,
            GridAreaLink? gridAreaLink,
            MarketParticipant? marketParticipant)
        {
            gridAreaRepository.Setup(x => x.GetOrNullAsync(It.IsAny<Guid>()))
                .ReturnsAsync(gridArea);
            gridAreaLinkRepository.Setup(x => x.GetOrNullAsync(It.IsAny<Guid>()))
                .ReturnsAsync(gridAreaLink);
            marketParticipantRepository.Setup(x => x.GetGridAccessProviderAsync(It.IsAny<Guid>()))
                .ReturnsAsync(marketParticipant);
        }
    }
}
