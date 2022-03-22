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
using GreenEnergyHub.Charges.Application.MeteringPoints.Handlers;
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.Domain.Dtos.MeteringPointCreatedEvents;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.Infrastructure.Core.Persistence;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.TestHelpers;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.MeteringPoints.Handlers
{
    [UnitTest]
    public class MeteringPointPersisterTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task PersistAsync_WhenCalledWithNonExistentMeteringPoint_ShouldPersist(
            [Frozen] Mock<IMeteringPointRepository> meteringPointRepository,
            [Frozen] Mock<ILoggerFactory> loggerFactory,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            Mock<ILogger> logger)
        {
            // Arrange
            var meteringPointCreatedEvent = GetMeteringPointCreatedEvent();
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

            meteringPointRepository.Setup(x => x.GetOrNullAsync(It.IsAny<string>())).ReturnsAsync(() => null);

            var sut = new MeteringPointPersister(meteringPointRepository.Object, loggerFactory.Object, unitOfWork.Object);

            // Act
            await sut.PersistAsync(meteringPointCreatedEvent).ConfigureAwait(false);

            // Assert
            meteringPointRepository.Verify(v => v.AddAsync(It.IsAny<MeteringPoint>()), Times.Exactly(1));
            logger.VerifyLoggerWasCalled(
                $"Metering Point ID '{meteringPointCreatedEvent.MeteringPointId}' has been persisted",
                LogLevel.Information);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task PersistAsync_WhenCalledWithExistingMeteringPoint_ShouldNotPersist(
            [Frozen] Mock<IMeteringPointRepository> meteringPointRepository,
            [Frozen] Mock<ILoggerFactory> loggerFactory,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            Mock<ILogger> logger)
        {
            // Arrange
            var meteringPointCreatedEvent = GetMeteringPointCreatedEvent();

            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

            var existingMeteringPoint = GetMeteringPoint(meteringPointCreatedEvent);

            meteringPointRepository.Setup(x => x.GetOrNullAsync(It.IsAny<string>())).ReturnsAsync(existingMeteringPoint);

            var sut = new MeteringPointPersister(meteringPointRepository.Object, loggerFactory.Object, unitOfWork.Object);

            // Act
            await sut.PersistAsync(meteringPointCreatedEvent).ConfigureAwait(false);

            // Assert
            meteringPointRepository
                .Verify(v => v.AddAsync(It.IsAny<MeteringPoint>()), Times.Never());
            logger.VerifyLoggerWasCalled(
                $"Metering Point ID '{meteringPointCreatedEvent.MeteringPointId}' already exists in storage",
                LogLevel.Information);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task PersistAsync_WhenNewMeteringPointDeviatesExisting_ShouldLogDifferences(
            [Frozen] Mock<IMeteringPointRepository> meteringPointRepository,
            [Frozen] Mock<ILoggerFactory> loggerFactory,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            Mock<ILogger> logger)
        {
            // Arrange
            var meteringPointCreatedEvent = GetMeteringPointCreatedEvent();
            var meteringPoint = GetMeteringPoint(meteringPointCreatedEvent);

            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

            var existingMeteringPoint = MeteringPoint.Create(
                meteringPointCreatedEvent.MeteringPointId,
                MeteringPointType.Production,
                Guid.NewGuid(),
                meteringPointCreatedEvent.EffectiveDate,
                meteringPointCreatedEvent.ConnectionState,
                SettlementMethod.NonProfiled);

            meteringPointRepository.Setup(x => x.GetOrNullAsync(It.IsAny<string>())).ReturnsAsync(existingMeteringPoint);

            var sut = new MeteringPointPersister(meteringPointRepository.Object, loggerFactory.Object, unitOfWork.Object);

            // Act
            await sut.PersistAsync(meteringPointCreatedEvent).ConfigureAwait(false);

            // Assert
            logger.VerifyLoggerWasCalled(
                $"Received 'metering point type' event data '{meteringPoint.MeteringPointType}' was not equal to " +
                $"the already persisted value '{existingMeteringPoint.MeteringPointType}' for Metering Point ID " +
                $"'{meteringPoint.MeteringPointId}'",
                LogLevel.Error);
            logger.VerifyLoggerWasCalled(
                $"Received 'settlement method' event data '{meteringPoint.SettlementMethod}' was not equal to the " +
                $"already persisted value '{existingMeteringPoint.SettlementMethod}' for Metering Point ID " +
                $"'{meteringPoint.MeteringPointId}'",
                LogLevel.Error);
            logger.VerifyLoggerWasCalled(
                $"Received 'grid area link id' event data '{meteringPoint.GridAreaLinkId}' was not equal to the " +
                $"already persisted value '{existingMeteringPoint.GridAreaLinkId}' for Metering Point ID " +
                $"'{meteringPoint.MeteringPointId}'",
                LogLevel.Error);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task PersistAsync_WhenEventIsNull_ThrowsArgumentNullException(MeteringPointPersister sut)
        {
            // Arrange
            MeteringPointCreatedEvent? meteringPointCreatedEvent = null;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => sut.PersistAsync(meteringPointCreatedEvent!))
                .ConfigureAwait(false);
        }

        private static MeteringPointCreatedEvent GetMeteringPointCreatedEvent()
        {
            return new MeteringPointCreatedEvent(
                "123",
                Guid.NewGuid(),
                SettlementMethod.Flex,
                ConnectionState.New,
                SystemClock.Instance.GetCurrentInstant(),
                MeteringPointType.Consumption);
        }

        private static MeteringPoint GetMeteringPoint(MeteringPointCreatedEvent meteringPointCreatedEvent)
        {
            var meteringPoint = MeteringPoint.Create(
                meteringPointCreatedEvent.MeteringPointId,
                meteringPointCreatedEvent.MeteringPointType,
                meteringPointCreatedEvent.GridAreaLinkId,
                meteringPointCreatedEvent.EffectiveDate,
                meteringPointCreatedEvent.ConnectionState,
                meteringPointCreatedEvent.SettlementMethod);

            return meteringPoint;
        }
    }
}
