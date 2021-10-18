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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Application.MeteringPoints.Handlers;
using GreenEnergyHub.Charges.Domain.MeteringPointCreatedEvents;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.TestHelpers;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using NodaTime.Extensions;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.MeteringPoints.Handlers
{
    [UnitTest]
    public class ConsumptionMeteringPointCreatedPersisterTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task PersistAsync_WhenCalledWithNonExistentMeteringPoint_ShouldPersist(
            [NotNull][Frozen] Mock<IMeteringPointRepository> meteringPointRepository,
            [NotNull][Frozen] Mock<ILoggerFactory> loggerFactory,
            [NotNull] Mock<ILogger> logger)
        {
            // Arrange
            var meteringPointCreatedEvent = GetMeteringPointCreatedEvent();
            loggerFactory.Setup(
                x => x.CreateLogger(
                        It.IsAny<string>()))
                    .Returns(logger.Object);

            meteringPointRepository.Setup(
                x => x.GetOrNullAsync(
                    It.IsAny<string>()))
                .ReturnsAsync(() => null);

            var sut = new ConsumptionMeteringPointPersister(meteringPointRepository.Object, loggerFactory.Object);

            // Act
            await sut.PersistAsync(meteringPointCreatedEvent).ConfigureAwait(false);

            // Assert
            meteringPointRepository
                .Verify(v => v.StoreMeteringPointAsync(It.IsAny<MeteringPoint>()), Times.Exactly(1));
            logger.VerifyLoggerWasCalled(
                $"Consumption Metering Point ID '{meteringPointCreatedEvent.MeteringPointId}' has been persisted",
                LogLevel.Information);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task PersistAsync_WhenCalledWithExistingMeteringPoint_ShouldNotPersist(
            [NotNull][Frozen] Mock<IMeteringPointRepository> meteringPointRepository,
            [NotNull][Frozen] Mock<ILoggerFactory> loggerFactory,
            [NotNull] Mock<ILogger> logger)
        {
            // Arrange
            var meteringPointCreatedEvent = GetMeteringPointCreatedEvent();

            loggerFactory.Setup(
                x => x.CreateLogger(
                        It.IsAny<string>()))
                    .Returns(logger.Object);

            var existingMeteringPoint = GetConsumptionMeteringPoint(meteringPointCreatedEvent);

            meteringPointRepository.Setup(
                x => x.GetOrNullAsync(
                    It.IsAny<string>()))
                .ReturnsAsync(existingMeteringPoint);

            var sut = new ConsumptionMeteringPointPersister(meteringPointRepository.Object, loggerFactory.Object);

            // Act
            await sut.PersistAsync(meteringPointCreatedEvent).ConfigureAwait(false);

            // Assert
            meteringPointRepository
                .Verify(v => v.StoreMeteringPointAsync(It.IsAny<MeteringPoint>()), Times.Never());
            logger.VerifyLoggerWasCalled(
                $"Metering Point ID '{meteringPointCreatedEvent.MeteringPointId}' already exists in storage",
                LogLevel.Information);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task PersistAsync_WhenNewMeteringPointDeviatesExisting_ShouldLogDifferences(
            [NotNull][Frozen] Mock<IMeteringPointRepository> meteringPointRepository,
            [NotNull][Frozen] Mock<ILoggerFactory> loggerFactory,
            [NotNull] Mock<ILogger> logger)
        {
            // Arrange
            var meteringPointCreatedEvent = GetMeteringPointCreatedEvent();
            var meteringPoint = GetConsumptionMeteringPoint(meteringPointCreatedEvent);

            loggerFactory.Setup(
                x => x.CreateLogger(
                        It.IsAny<string>()))
                    .Returns(logger.Object);

            var existingMeteringPoint = MeteringPoint.Create(
                meteringPointCreatedEvent.MeteringPointId,
                MeteringPointType.Production,
                "DiffGridArea",
                meteringPointCreatedEvent.EffectiveDate,
                meteringPointCreatedEvent.ConnectionState,
                SettlementMethod.NonProfiled);

            meteringPointRepository.Setup(
                x => x.GetOrNullAsync(
                    It.IsAny<string>()))
                .ReturnsAsync(existingMeteringPoint);

            var sut = new ConsumptionMeteringPointPersister(meteringPointRepository.Object, loggerFactory.Object);

            // Act
            await sut.PersistAsync(meteringPointCreatedEvent).ConfigureAwait(false);

            // Assert
            logger.VerifyLoggerWasCalled(
                $"Received 'metering point type' event data '{meteringPoint.MeteringPointType}' was not equal to the already persisted value '{existingMeteringPoint.MeteringPointType}' for Metering Point ID '{meteringPoint.MeteringPointId}'",
                LogLevel.Error);
            logger.VerifyLoggerWasCalled(
                $"Received 'settlement method' event data '{meteringPoint.SettlementMethod}' was not equal to the already persisted value '{existingMeteringPoint.SettlementMethod}' for Metering Point ID '{meteringPoint.MeteringPointId}'",
                LogLevel.Error);
            logger.VerifyLoggerWasCalled(
                $"Received 'grid area id' event data '{meteringPoint.GridAreaId}' was not equal to the already persisted value '{existingMeteringPoint.GridAreaId}' for Metering Point ID '{meteringPoint.MeteringPointId}'",
                LogLevel.Error);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task PersistAsync_WhenEventIsNull_ThrowsArgumentNullException(
            [NotNull] ConsumptionMeteringPointPersister sut)
        {
            // Arrange
            ConsumptionMeteringPointCreatedEvent? meteringPointCreatedEvent = null;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => sut.PersistAsync(meteringPointCreatedEvent!))
                .ConfigureAwait(false);
        }

        private static ConsumptionMeteringPointCreatedEvent GetMeteringPointCreatedEvent()
        {
            return new ConsumptionMeteringPointCreatedEvent(
                "123",
                "2",
                SettlementMethod.Flex,
                ConnectionState.New,
                SystemClock.Instance.GetCurrentInstant());
        }

        private static MeteringPoint GetConsumptionMeteringPoint(ConsumptionMeteringPointCreatedEvent meteringPointCreatedEvent)
        {
            var meteringPoint = MeteringPoint.Create(
                meteringPointCreatedEvent.MeteringPointId,
                MeteringPointType.Consumption,
                meteringPointCreatedEvent.GridAreaId,
                meteringPointCreatedEvent.EffectiveDate,
                meteringPointCreatedEvent.ConnectionState,
                meteringPointCreatedEvent.SettlementMethod);

            return meteringPoint;
        }
    }
}
