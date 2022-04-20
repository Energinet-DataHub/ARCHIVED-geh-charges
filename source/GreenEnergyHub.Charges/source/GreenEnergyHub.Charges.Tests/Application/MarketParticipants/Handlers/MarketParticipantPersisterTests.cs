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
using GreenEnergyHub.Charges.Domain.Dtos.MarketParticipantsChangedEvents;
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
        public async Task PersistAsync_WhenCalledWithNonExistentMarketParticipant_ShouldPersist(
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<ILoggerFactory> loggerFactory,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            Mock<ILogger> logger)
        {
            // Arrange
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var marketParticipantChangedEvent = GetMarketParticipantChangedEvent(new List<MarketParticipantRole>
                { MarketParticipantRole.EnergySupplier, MarketParticipantRole.GridAccessProvider });

            marketParticipantRepository.Setup(x =>
                x.GetOrNullAsync(It.IsAny<string>(), It.IsAny<MarketParticipantRole>()))
                .ReturnsAsync(() => null);
            var sut = new MarketParticipantPersister(marketParticipantRepository.Object, loggerFactory.Object, unitOfWork.Object);

            // Act
            await sut.PersistAsync(marketParticipantChangedEvent).ConfigureAwait(false);

            // Assert
            marketParticipantRepository.Verify(v => v.AddAsync(It.IsAny<MarketParticipant>()), Times.Exactly(2));
            logger.VerifyLoggerWasCalled(
                $"Marketparticipant ID '{marketParticipantChangedEvent.MarketParticipantId}' " +
                $"with role '{MarketParticipantRole.EnergySupplier}' has been persisted",
                LogLevel.Information);
            logger.VerifyLoggerWasCalled(
                $"Marketparticipant ID '{marketParticipantChangedEvent.MarketParticipantId}' " +
                $"with role '{MarketParticipantRole.GridAccessProvider}' has been persisted",
                LogLevel.Information);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task PersistAsync_WhenCalledWithExistingMarketParticipant_ShouldUpdateExisting(
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<ILoggerFactory> loggerFactory,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            Mock<ILogger> logger)
        {
            // Arrange
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var marketParticipantChangedEvent = GetMarketParticipantChangedEvent(
                new List<MarketParticipantRole> { MarketParticipantRole.EnergySupplier });

            var existingMarketParticipant = GetMarketParticipant(marketParticipantChangedEvent);

            marketParticipantRepository.Setup(x => x.GetOrNullAsync(
                It.IsAny<string>(), It.IsAny<MarketParticipantRole>()))
                .ReturnsAsync(existingMarketParticipant);

            var sut = new MarketParticipantPersister(marketParticipantRepository.Object, loggerFactory.Object, unitOfWork.Object);

            // Act
            await sut.PersistAsync(marketParticipantChangedEvent).ConfigureAwait(false);

            // Assert
            marketParticipantRepository
                .Verify(v => v.AddAsync(It.IsAny<MarketParticipant>()), Times.Never());
            logger.VerifyLoggerWasCalled(
                $"Marketparticipant ID '{existingMarketParticipant.MarketParticipantId}' " +
                $"with role '{existingMarketParticipant.BusinessProcessRole}' has changed state",
                LogLevel.Information);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task PersistAsync_WhenEventIsNull_ThrowsArgumentNullException(MarketParticipantPersister sut)
        {
            // Arrange
            MarketParticipantChangedEvent? marketParticipantChangedEvent = null;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => sut.PersistAsync(marketParticipantChangedEvent!))
                .ConfigureAwait(false);
        }

        private static MarketParticipantChangedEvent GetMarketParticipantChangedEvent(List<MarketParticipantRole> marketParticipantRoleCodes)
        {
            return new MarketParticipantChangedEvent(
                "mp123",
                marketParticipantRoleCodes,
                true);
        }

        private static MarketParticipant GetMarketParticipant(
            MarketParticipantChangedEvent marketParticipantChangedEvent)
        {
            return new MarketParticipant(
                Guid.NewGuid(),
                marketParticipantChangedEvent.MarketParticipantId,
                marketParticipantChangedEvent.IsActive,
                marketParticipantChangedEvent.BusinessProcessRoles.Single());
        }
    }
}
