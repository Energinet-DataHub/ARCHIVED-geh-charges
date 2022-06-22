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
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Application.MarketParticipants.Handlers;
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.Domain.Dtos.MarketParticipantsUpdatedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.GridAreaLinks;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.TestHelpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Categories;
using GridAreaLink = GreenEnergyHub.Charges.Domain.GridAreaLinks.GridAreaLink;
using MarketParticipant = GreenEnergyHub.Charges.Domain.MarketParticipants.MarketParticipant;

namespace GreenEnergyHub.Charges.Tests.Application.MarketParticipants.Handlers
{
    [UnitTest]
    public class MarketParticipantPersisterTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task PersistAsync_WhenCalledWithNonExistentMarketParticipantAndNonExistentGridArea_ShouldPersist(
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IGridAreaLinkRepository> gridAreaLinkRepository,
            [Frozen] Mock<ILoggerFactory> loggerFactory,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            Mock<ILogger> logger)
        {
            // Arrange
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var marketParticipantUpdatedEvent = GetMarketParticipantUpdatedEvent(
                new List<MarketParticipantRole> { MarketParticipantRole.GridAccessProvider },
                new List<Guid>());

            SetupRepositories(marketParticipantRepository, null!, gridAreaLinkRepository, null!);

            var sut = new MarketParticipantPersister(
                marketParticipantRepository.Object,
                gridAreaLinkRepository.Object,
                loggerFactory.Object,
                unitOfWork.Object);

            // Act
            await sut.PersistAsync(marketParticipantUpdatedEvent).ConfigureAwait(false);

            // Assert
            marketParticipantRepository.Verify(v => v.AddAsync(It.IsAny<MarketParticipant>()), Times.Once());
            logger.VerifyLoggerWasCalled(
                $"Market participant with ID '{marketParticipantUpdatedEvent.MarketParticipantId}' " +
                $"and role '{MarketParticipantRole.GridAccessProvider}' has been persisted",
                LogLevel.Information);
            logger.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task PersistAsync_WhenCalledWithExistingMarketParticipantAndNonExistentGridArea_ShouldUpdateExisting(
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IGridAreaLinkRepository> gridAreaLinkRepository,
            [Frozen] Mock<ILoggerFactory> loggerFactory,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            Mock<ILogger> logger)
        {
            // Arrange
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var marketParticipantUpdatedEvent = GetMarketParticipantUpdatedEvent(
                new List<MarketParticipantRole> { MarketParticipantRole.GridAccessProvider },
                new List<Guid>());

            var existingMarketParticipant = GetMarketParticipant(marketParticipantUpdatedEvent);

            SetupRepositories(marketParticipantRepository, existingMarketParticipant, gridAreaLinkRepository, null!);

            var sut = new MarketParticipantPersister(
                marketParticipantRepository.Object,
                gridAreaLinkRepository.Object,
                loggerFactory.Object,
                unitOfWork.Object);

            // Act
            await sut.PersistAsync(marketParticipantUpdatedEvent).ConfigureAwait(false);

            // Assert
            marketParticipantRepository.Verify(v => v.AddAsync(It.IsAny<MarketParticipant>()), Times.Never());
            logger.VerifyLoggerWasCalled(
                $"Market participant with ID '{existingMarketParticipant.MarketParticipantId}' " +
                $"and role '{existingMarketParticipant.BusinessProcessRole}' has changed state",
                LogLevel.Information);
            logger.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task PersistAsync_WhenCalledWithNonExistentMarketParticipantAndExistentGridAreaLink_ShouldPersist(
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IGridAreaLinkRepository> gridAreaLinkRepository,
            [Frozen] Mock<ILoggerFactory> loggerFactory,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            Mock<ILogger> logger)
        {
            // Arrange
            var gridAreaId = Guid.NewGuid();
            var gridAreaLink = new GridAreaLink(Guid.NewGuid(), gridAreaId, Guid.NewGuid());

            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var marketParticipantUpdatedEvent = GetMarketParticipantUpdatedEvent(
                new List<MarketParticipantRole>
                {
                    MarketParticipantRole.GridAccessProvider,
                },
                new List<Guid> { gridAreaLink.GridAreaId });

            SetupRepositories(marketParticipantRepository, null!, gridAreaLinkRepository, gridAreaLink);

            var sut = new MarketParticipantPersister(
                marketParticipantRepository.Object,
                gridAreaLinkRepository.Object,
                loggerFactory.Object,
                unitOfWork.Object);

            // Act
            await sut.PersistAsync(marketParticipantUpdatedEvent).ConfigureAwait(false);

            // Assert
            marketParticipantRepository.Verify(v => v.AddAsync(It.IsAny<MarketParticipant>()), Times.Once);
            gridAreaLinkRepository.Verify(v => v.GetGridAreaOrNullAsync(It.IsAny<Guid>()), Times.Once);
            logger.VerifyLoggerWasCalled(
                $"Market participant with ID '{marketParticipantUpdatedEvent.MarketParticipantId}' " +
                $"and role '{MarketParticipantRole.GridAccessProvider}' has been persisted",
                LogLevel.Information);
            logger.VerifyLoggerWasCalled(
                $"GridAreaLink ID '{gridAreaLink.Id}' has changed Owner ID to '{gridAreaLink.OwnerId}'",
                LogLevel.Information);
            logger.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task PersistAsync_WhenCalledWithNonExistingMarketParticipantAndMultipleExistingGridAreas_ShouldPersist(
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IGridAreaLinkRepository> gridAreaLinkRepository,
            [Frozen] Mock<ILoggerFactory> loggerFactory,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            Mock<ILogger> logger)
        {
            // Arrange
            var gridAreaLink = new GridAreaLink(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            var gridAreas = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var marketParticipantUpdatedEvent = GetMarketParticipantUpdatedEvent(
                new List<MarketParticipantRole> { MarketParticipantRole.GridAccessProvider },
                gridAreas);

            SetupRepositories(marketParticipantRepository, null!, gridAreaLinkRepository, gridAreaLink);

            var sut = new MarketParticipantPersister(
                marketParticipantRepository.Object,
                gridAreaLinkRepository.Object,
                loggerFactory.Object,
                unitOfWork.Object);

            // Act
            await sut.PersistAsync(marketParticipantUpdatedEvent).ConfigureAwait(false);

            // Assert
            marketParticipantRepository.Verify(v => v.AddAsync(It.IsAny<MarketParticipant>()), Times.Once);
            gridAreaLinkRepository.Verify(v => v.GetGridAreaOrNullAsync(It.IsAny<Guid>()), Times.Exactly(2));
            logger.VerifyLoggerWasCalled(
                $"Market participant with ID '{marketParticipantUpdatedEvent.MarketParticipantId}' " +
                $"and role '{MarketParticipantRole.GridAccessProvider}' has been persisted",
                LogLevel.Information);
            logger.VerifyLoggerWasCalled(
                $"GridAreaLink ID '{gridAreaLink.Id}' has changed Owner ID to '{gridAreaLink.OwnerId}'",
                LogLevel.Information);
            logger.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task PersistAsync_WhenCalledWithExistingMarketParticipantWithDifferentRole_ShouldThrowInvalidOperation(
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IGridAreaLinkRepository> gridAreaLinkRepository,
            [Frozen] Mock<ILoggerFactory> loggerFactory,
            [Frozen] Mock<IUnitOfWork> unitOfWork)
        {
            // Arrange
            var existingMarketParticipant = new MarketParticipant(
                id: Guid.NewGuid(),
                b2CActorId: Guid.NewGuid(),
                "mp123",
                true,
                MarketParticipantRole.GridAccessProvider);

            var marketParticipantUpdatedEvent = GetMarketParticipantUpdatedEvent(
                new List<MarketParticipantRole> { MarketParticipantRole.EnergySupplier },
                new List<Guid>());

            SetupRepositories(marketParticipantRepository, null!, gridAreaLinkRepository, null!);
            marketParticipantRepository
                .Setup(x => x.SingleOrNullAsync(It.IsAny<string>()))
                .ReturnsAsync(() => existingMarketParticipant);

            var sut = new MarketParticipantPersister(
                marketParticipantRepository.Object,
                gridAreaLinkRepository.Object,
                loggerFactory.Object,
                unitOfWork.Object);

            // Act and Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                    () => sut.PersistAsync(marketParticipantUpdatedEvent))
                .ConfigureAwait(false);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task PersistAsync_WhenEventIsNull_ThrowsArgumentNullException(MarketParticipantPersister sut)
        {
            // Arrange
            MarketParticipantUpdatedEvent? marketParticipantUpdatedEvent = null;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => sut.PersistAsync(marketParticipantUpdatedEvent!))
                .ConfigureAwait(false);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task PersistAsync_WhenCalledWithRoleOtherThanGridAccessProvider_ShouldNotConnectToGrid(
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IGridAreaLinkRepository> gridAreaLinkRepository,
            [Frozen] Mock<ILoggerFactory> loggerFactory,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            Mock<ILogger> logger)
        {
            // Arrange
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var marketParticipantUpdatedEvent = GetMarketParticipantUpdatedEvent(
                new List<MarketParticipantRole> { MarketParticipantRole.EnergySupplier },
                new List<Guid>());

            var existingMarketParticipant = GetMarketParticipant(marketParticipantUpdatedEvent);
            var gridAreaLink = new GridAreaLink(Guid.NewGuid(), Guid.NewGuid(), existingMarketParticipant.Id);
            SetupRepositories(marketParticipantRepository, existingMarketParticipant, gridAreaLinkRepository, gridAreaLink);

            var sut = new MarketParticipantPersister(
                marketParticipantRepository.Object,
                gridAreaLinkRepository.Object,
                loggerFactory.Object,
                unitOfWork.Object);

            // Act
            await sut.PersistAsync(marketParticipantUpdatedEvent).ConfigureAwait(false);

            // Assert
            gridAreaLinkRepository.Verify(v => v.GetGridAreaOrNullAsync(It.IsAny<Guid>()), Times.Never());
            logger.VerifyLoggerWasCalled(
                $"Market participant with ID '{existingMarketParticipant.MarketParticipantId}' " +
                $"and role '{existingMarketParticipant.BusinessProcessRole}' has changed state",
                LogLevel.Information);
            logger.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task PersistAsync_WhenCalledWithoutGridAreaLink_ShouldNotConnectToGrid(
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IGridAreaLinkRepository> gridAreaLinkRepository,
            [Frozen] Mock<ILoggerFactory> loggerFactory,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            Mock<ILogger> logger)
        {
            // Arrange
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var marketParticipantUpdatedEvent = GetMarketParticipantUpdatedEvent(
                new List<MarketParticipantRole> { MarketParticipantRole.GridAccessProvider },
                new List<Guid>() { Guid.NewGuid() });

            var existingMarketParticipant = GetMarketParticipant(marketParticipantUpdatedEvent);
            SetupRepositories(marketParticipantRepository, existingMarketParticipant, gridAreaLinkRepository, null!);

            var sut = new MarketParticipantPersister(
                marketParticipantRepository.Object,
                gridAreaLinkRepository.Object,
                loggerFactory.Object,
                unitOfWork.Object);

            // Act
            await sut.PersistAsync(marketParticipantUpdatedEvent).ConfigureAwait(false);

            // Assert
            gridAreaLinkRepository.Verify(v => v.GetGridAreaOrNullAsync(It.IsAny<Guid>()), Times.Once);
            logger.VerifyLoggerWasCalled(
                $"Market participant with ID '{existingMarketParticipant.MarketParticipantId}' " +
                $"and role '{existingMarketParticipant.BusinessProcessRole}' has changed state",
                LogLevel.Information);
            logger.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task PersistAsync_WhenCalledWithExistingGridAreaLink_ShouldNotConnectToGrid(
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IGridAreaLinkRepository> gridAreaLinkRepository,
            [Frozen] Mock<ILoggerFactory> loggerFactory,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            Mock<ILogger> logger)
        {
            // Arrange
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var gridAreaId = Guid.NewGuid();
            var marketParticipantUpdatedEvent = GetMarketParticipantUpdatedEvent(
                new List<MarketParticipantRole> { MarketParticipantRole.GridAccessProvider },
                new List<Guid>() { gridAreaId });

            var existingMarketParticipant = GetMarketParticipant(marketParticipantUpdatedEvent);
            var gridAreaLink = new GridAreaLink(Guid.NewGuid(), gridAreaId, existingMarketParticipant.Id);
            SetupRepositories(marketParticipantRepository, existingMarketParticipant, gridAreaLinkRepository, gridAreaLink);

            var sut = new MarketParticipantPersister(
                marketParticipantRepository.Object,
                gridAreaLinkRepository.Object,
                loggerFactory.Object,
                unitOfWork.Object);

            // Act
            await sut.PersistAsync(marketParticipantUpdatedEvent).ConfigureAwait(false);

            // Assert
            gridAreaLinkRepository.Verify(v => v.GetGridAreaOrNullAsync(It.IsAny<Guid>()), Times.Once());
            logger.VerifyLoggerWasCalled(
                $"Market participant with ID '{existingMarketParticipant.MarketParticipantId}' " +
                $"and role '{existingMarketParticipant.BusinessProcessRole}' has changed state",
                LogLevel.Information);
            logger.VerifyLoggerWasCalled(
                $"GridAreaLink ID '{gridAreaLink.Id}' has changed Owner ID to '{gridAreaLink.OwnerId}'",
                LogLevel.Information); // TODO: This verification should be removed when MarketParticipant.Id can no longer be changed
            logger.VerifyNoOtherCalls();
        }

        private static MarketParticipantUpdatedEvent GetMarketParticipantUpdatedEvent(
            List<MarketParticipantRole> marketParticipantRoleCodes,
            IEnumerable<Guid> gridAreas)
        {
            return new MarketParticipantUpdatedEvent(
                actorId: Guid.NewGuid(),
                b2CActorId: Guid.NewGuid(),
                "mp123",
                marketParticipantRoleCodes,
                true,
                gridAreas);
        }

        private static MarketParticipant GetMarketParticipant(
            MarketParticipantUpdatedEvent marketParticipantUpdatedEvent)
        {
            return new MarketParticipant(
                id: Guid.NewGuid(),
                b2CActorId: Guid.NewGuid(),
                marketParticipantUpdatedEvent.MarketParticipantId,
                marketParticipantUpdatedEvent.IsActive,
                marketParticipantUpdatedEvent.BusinessProcessRoles.Single());
        }

        private static void SetupRepositories(
            Mock<IMarketParticipantRepository> marketParticipantRepository,
            MarketParticipant marketParticipant,
            Mock<IGridAreaLinkRepository> gridAreaLinkRepository,
            GridAreaLink gridAreaLink)
        {
            marketParticipantRepository
                .Setup(x => x.SingleOrNullAsync(It.IsAny<MarketParticipantRole>(), It.IsAny<string>()))
                .ReturnsAsync(() => marketParticipant);

            marketParticipantRepository
                .Setup(x => x.SingleOrNullAsync(It.IsAny<string>()))
                .ReturnsAsync(() => marketParticipant);

            gridAreaLinkRepository
                .Setup(x => x.GetOrNullAsync(It.IsAny<Guid>()))
                .ReturnsAsync(() => gridAreaLink);

            gridAreaLinkRepository
                .Setup(x => x.GetGridAreaOrNullAsync(It.IsAny<Guid>()))
                .ReturnsAsync(() => gridAreaLink);
        }
    }
}
