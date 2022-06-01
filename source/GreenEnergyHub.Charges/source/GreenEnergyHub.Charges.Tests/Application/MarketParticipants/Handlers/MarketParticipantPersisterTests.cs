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
    public class MarketParticipantPersisterTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task PersistAsync_WhenCalledWithNonExistentMarketParticipantAndNonExistentGridArea_ShouldPersist(
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IGridAreaRepository> gridAreaRepository,
            [Frozen] Mock<ILoggerFactory> loggerFactory,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            Mock<ILogger> logger)
        {
            // Arrange
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var marketParticipantChangedEvent = GetMarketParticipantChangedEvent(
                new List<MarketParticipantRole> { MarketParticipantRole.EnergySupplier, MarketParticipantRole.GridAccessProvider },
                new List<Guid>());

            SetupRepositories(marketParticipantRepository, null!, gridAreaRepository, null!);

            var sut = new MarketParticipantPersister(
                marketParticipantRepository.Object,
                gridAreaRepository.Object,
                loggerFactory.Object,
                unitOfWork.Object);

            // Act
            await sut.PersistAsync(marketParticipantChangedEvent).ConfigureAwait(false);

            // Assert
            marketParticipantRepository.Verify(v => v.AddAsync(It.IsAny<MarketParticipant>()), Times.Exactly(2));
            logger.VerifyLoggerWasCalled(
                $"Market participant with ID '{marketParticipantChangedEvent.MarketParticipantId}' " +
                $"and role '{MarketParticipantRole.EnergySupplier}' has been persisted",
                LogLevel.Information);
            logger.VerifyLoggerWasCalled(
                $"Market participant with ID '{marketParticipantChangedEvent.MarketParticipantId}' " +
                $"and role '{MarketParticipantRole.GridAccessProvider}' has been persisted",
                LogLevel.Information);
            logger.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task PersistAsync_WhenCalledWithExistingMarketParticipantAndNonExistentGridArea_ShouldUpdateExisting(
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IGridAreaRepository> gridAreaRepository,
            [Frozen] Mock<ILoggerFactory> loggerFactory,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            Mock<ILogger> logger)
        {
            // Arrange
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var marketParticipantChangedEvent = GetMarketParticipantChangedEvent(
                new List<MarketParticipantRole> { MarketParticipantRole.GridAccessProvider },
                new List<Guid>());

            var existingMarketParticipant = GetMarketParticipant(marketParticipantChangedEvent);

            SetupRepositories(marketParticipantRepository, existingMarketParticipant, gridAreaRepository, null!);

            var sut = new MarketParticipantPersister(
                marketParticipantRepository.Object,
                gridAreaRepository.Object,
                loggerFactory.Object,
                unitOfWork.Object);

            // Act
            await sut.PersistAsync(marketParticipantChangedEvent).ConfigureAwait(false);

            // Assert
            marketParticipantRepository
                .Verify(v => v.AddAsync(It.IsAny<MarketParticipant>()), Times.Never());
            logger.VerifyLoggerWasCalled(
                $"Market participant with ID '{existingMarketParticipant.MarketParticipantId}' " +
                $"and role '{existingMarketParticipant.BusinessProcessRole}' has changed state",
                LogLevel.Information);
            logger.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task PersistAsync_WhenCalledWithNonExistentMarketParticipantAndExistentGridArea_ShouldPersist(
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IGridAreaRepository> gridAreaRepository,
            [Frozen] Mock<ILoggerFactory> loggerFactory,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            Mock<ILogger> logger)
        {
            // Arrange
            var gridAreaId = Guid.NewGuid();
            var gridArea = new GridArea(gridAreaId, Guid.NewGuid());

            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var marketParticipantChangedEvent = GetMarketParticipantChangedEvent(
                new List<MarketParticipantRole>
                {
                    MarketParticipantRole.GridAccessProvider,
                    MarketParticipantRole.SystemOperator,
                },
                new List<Guid> { gridArea.Id });

            SetupRepositories(marketParticipantRepository, null!, gridAreaRepository, gridArea);

            var sut = new MarketParticipantPersister(
                marketParticipantRepository.Object,
                gridAreaRepository.Object,
                loggerFactory.Object,
                unitOfWork.Object);

            // Act
            await sut.PersistAsync(marketParticipantChangedEvent).ConfigureAwait(false);

            // Assert
            marketParticipantRepository.Verify(v => v.AddAsync(It.IsAny<MarketParticipant>()), Times.Exactly(2));
            gridAreaRepository.Verify(v => v.GetOrNullAsync(It.IsAny<Guid>()), Times.Exactly(1));
            logger.VerifyLoggerWasCalled(
                $"Market participant with ID '{marketParticipantChangedEvent.MarketParticipantId}' " +
                $"and role '{MarketParticipantRole.GridAccessProvider}' has been persisted",
                LogLevel.Information);
            logger.VerifyLoggerWasCalled(
                $"Market participant with ID '{marketParticipantChangedEvent.MarketParticipantId}' " +
                $"and role '{MarketParticipantRole.SystemOperator}' has been persisted",
                LogLevel.Information);
            logger.VerifyLoggerWasCalled(
                $"GridArea ID '{gridArea.Id}' has changed GridAccessProvider ID to '{gridArea.GridAccessProviderId}'",
                LogLevel.Information);
            logger.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task PersistAsync_WhenCalledWithExistentMarketParticipantAndMultipleExistentGridAreas_ShouldPersist(
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IGridAreaRepository> gridAreaRepository,
            [Frozen] Mock<ILoggerFactory> loggerFactory,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            Mock<ILogger> logger)
        {
            // Arrange
            var gridArea = new GridArea(Guid.NewGuid(), Guid.NewGuid());
            var gridAreas = new List<Guid>() { Guid.NewGuid(), Guid.NewGuid() };

            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var marketParticipantChangedEvent = GetMarketParticipantChangedEvent(
                new List<MarketParticipantRole>
                {
                    MarketParticipantRole.GridAccessProvider,
                    MarketParticipantRole.SystemOperator,
                },
                gridAreas);

            SetupRepositories(marketParticipantRepository, null!, gridAreaRepository, gridArea);

            var sut = new MarketParticipantPersister(
                marketParticipantRepository.Object,
                gridAreaRepository.Object,
                loggerFactory.Object,
                unitOfWork.Object);

            // Act
            await sut.PersistAsync(marketParticipantChangedEvent).ConfigureAwait(false);

            // Assert
            marketParticipantRepository.Verify(v => v.AddAsync(It.IsAny<MarketParticipant>()), Times.Exactly(2));
            gridAreaRepository.Verify(v => v.GetOrNullAsync(It.IsAny<Guid>()), Times.Exactly(2));
            logger.VerifyLoggerWasCalled(
                $"Market participant with ID '{marketParticipantChangedEvent.MarketParticipantId}' " +
                $"and role '{MarketParticipantRole.GridAccessProvider}' has been persisted",
                LogLevel.Information);
            logger.VerifyLoggerWasCalled(
                $"Market participant with ID '{marketParticipantChangedEvent.MarketParticipantId}' " +
                $"and role '{MarketParticipantRole.SystemOperator}' has been persisted",
                LogLevel.Information);
            logger.VerifyLoggerWasCalled(
                $"GridArea ID '{gridArea.Id}' has changed GridAccessProvider ID to '{gridArea.GridAccessProviderId}'",
                LogLevel.Information);
            logger.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task PersistAsync_WhenEventIsNull_ThrowsArgumentNullException(MarketParticipantPersister sut)
        {
            // Arrange
            MarketParticipantUpdatedEvent? marketParticipantChangedEvent = null;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => sut.PersistAsync(marketParticipantChangedEvent!))
                .ConfigureAwait(false);
        }

        private static MarketParticipantUpdatedEvent GetMarketParticipantChangedEvent(
            List<MarketParticipantRole> marketParticipantRoleCodes,
            IEnumerable<Guid> gridAreas)
        {
            return new MarketParticipantUpdatedEvent(
                "mp123",
                marketParticipantRoleCodes,
                true,
                gridAreas);
        }

        private static MarketParticipant GetMarketParticipant(
            MarketParticipantUpdatedEvent marketParticipantChangedEvent)
        {
            return new MarketParticipant(
                Guid.NewGuid(),
                marketParticipantChangedEvent.MarketParticipantId,
                marketParticipantChangedEvent.IsActive,
                marketParticipantChangedEvent.BusinessProcessRoles.Single());
        }

        private static void SetupRepositories(
            Mock<IMarketParticipantRepository> marketParticipantRepository,
            MarketParticipant marketParticipant,
            Mock<IGridAreaRepository> gridAreaRepository,
            GridArea gridArea)
        {
            marketParticipantRepository.Setup(x =>
                    x.SingleOrNullAsync(It.IsAny<MarketParticipantRole>(), It.IsAny<string>()))
                .ReturnsAsync(() => marketParticipant);

            gridAreaRepository.Setup(x =>
                    x.GetOrNullAsync(It.IsAny<Guid>()))
                .ReturnsAsync(() => gridArea);
        }
    }
}
