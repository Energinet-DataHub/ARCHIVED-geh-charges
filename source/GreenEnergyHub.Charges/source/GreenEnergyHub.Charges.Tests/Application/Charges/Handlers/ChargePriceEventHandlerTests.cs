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
using GreenEnergyHub.Charges.Application.Charges.Events;
using GreenEnergyHub.Charges.Application.Charges.Factories;
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Application.Charges.Services;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.Charges.Tests.Builders.Testables;
using GreenEnergyHub.TestHelpers;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Handlers
{
    [UnitTest]
    public class ChargePriceEventHandlerTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidationSucceed_CallsConfirmServiceAndLogsThatPricesArePersisted(
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IInputValidator<ChargePriceOperationDto>> inputValidator,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            TestMarketParticipant sender,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IChargePriceRejectionService> chargePriceRejectionService,
            [Frozen] Mock<IChargePriceConfirmationService> chargePriceConfirmationService,
            [Frozen] Mock<IChargePriceNotificationService> chargePriceNotificationService,
            [Frozen] Mock<ILoggerFactory> loggerFactory,
            [Frozen] Mock<IChargePriceOperationsRejectedEventFactory> chargePriceOperationsRejectedEventFactory,
            Mock<ILogger> logger,
            ChargeBuilder chargeBuilder)
        {
            // Arrange
            var chargeCommand = CreateChargeCommandWith24Points();
            var receivedEvent = new ChargePriceCommandReceivedEvent(InstantHelper.GetTodayAtMidnightUtc(), chargeCommand);
            var validationResult = ValidationResult.CreateSuccess();
            inputValidator
                .Setup(v => v.Validate(It.IsAny<ChargePriceOperationDto>())).Returns(validationResult);
            var points = new List<Point>
            {
                new(1.00m, InstantHelper.GetTodayPlusDaysAtMidnightUtc(0)),
                new(2.00m, InstantHelper.GetTodayPlusDaysAtMidnightUtc(1)),
            };
            var charge = chargeBuilder.WithPoints(points).Build();
            SetupChargeRepository(chargeRepository, charge);
            SetupMarketParticipantRepository(marketParticipantRepository, sender);
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);

            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

            var sut = new ChargePriceEventHandler(
                chargeRepository.Object,
                marketParticipantRepository.Object,
                inputValidator.Object,
                chargePriceConfirmationService.Object,
                chargePriceRejectionService.Object,
                chargePriceNotificationService.Object,
                loggerFactory.Object,
                chargePriceOperationsRejectedEventFactory.Object);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            chargePriceRejectionService.Verify(
                x => x.SaveRejections(It.IsAny<ChargePriceOperationsRejectedEvent>()),
                Times.Never);
            chargePriceConfirmationService.Verify(
                x =>
                    x.SaveConfirmationsAsync(It.Is<List<ChargePriceOperationDto>>(y => y.Count == 1)),
                Times.Once);

            var expectedMessage = $"At this point, price(s) will be persisted for operation with Id {receivedEvent.Command.Operations.First().Id}";
            logger.VerifyLoggerWasCalled(expectedMessage, LogLevel.Information);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidationFails_RejectsEvent(
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IInputValidator<ChargePriceOperationDto>> inputValidator,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IChargePriceRejectionService> chargePriceRejectionService,
            [Frozen] Mock<IChargePriceOperationsRejectedEventFactory> chargePriceOperationsRejectedEventFactory,
            TestMarketParticipant sender,
            ChargeBuilder chargeBuilder,
            ChargePriceCommandReceivedEvent receivedEvent,
            ChargePriceOperationsRejectedEvent chargePriceRejectedEvent,
            ChargePriceEventHandler sut)
        {
            // Arrange
            var validationResult = GetFailedValidationResult();
            inputValidator.Setup(v => v.Validate(It.IsAny<ChargePriceOperationDto>())).Returns(validationResult);
            var charge = chargeBuilder.Build();

            chargePriceOperationsRejectedEventFactory
                .Setup(c => c.Create(It.IsAny<ChargePriceCommand>(), It.IsAny<ValidationResult>()))
                .Returns(chargePriceRejectedEvent);
            SetupChargeRepository(chargeRepository, charge);
            SetupMarketParticipantRepository(marketParticipantRepository, sender);
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            chargePriceRejectionService.Verify(
                x => x.SaveRejections(It.IsAny<ChargePriceOperationsRejectedEvent>()), Times.Once);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_IfEventIsNull_ThrowsArgumentNullException(
            ChargePriceEventHandler sut)
        {
            // Arrange
            ChargePriceCommandReceivedEvent? receivedEvent = null;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.HandleAsync(receivedEvent!));
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenPriceSeriesWithResolutionPT1H_StorePriceSeries(
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IInputValidator<ChargePriceOperationDto>> inputValidator,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            TestMarketParticipant sender,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            ChargeBuilder chargeBuilder,
            ChargePriceEventHandler sut)
        {
            // Arrange
            var validationResult = ValidationResult.CreateSuccess();
            inputValidator.Setup(v => v.Validate(It.IsAny<ChargePriceOperationDto>())).Returns(validationResult);
            var points = new List<Point>();
            var price = 99.00M;
            for (var i = 0; i <= 23; i++)
            {
                points.Add(new Point(price + i, InstantHelper.GetTodayAtMidnightUtc() + Duration.FromHours(i)));
            }

            var charge = chargeBuilder.WithPoints(points).Build();
            SetupChargeRepository(chargeRepository, charge);
            SetupMarketParticipantRepository(marketParticipantRepository, sender);
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);

            var chargeCommand = CreateChargeCommandWith24Points();
            var receivedEvent = new ChargePriceCommandReceivedEvent(InstantHelper.GetTodayAtMidnightUtc(), chargeCommand);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            // charge.Points.Count.Should().Be(24);
            // TODO: assert that points are updated to those created in CreateChargeCommandWith24Points()
        }

        private static void SetupMarketParticipantRepository(
            Mock<IMarketParticipantRepository> marketParticipantRepository,
            TestMarketParticipant marketParticipant)
        {
            marketParticipantRepository
                .Setup(r => r.GetSystemOperatorOrGridAccessProviderAsync(It.IsAny<string>()))
                .ReturnsAsync(marketParticipant);
        }

        private static void SetupChargeIdentifierFactoryMock(Mock<IChargeIdentifierFactory> chargeIdentifierFactory)
        {
            chargeIdentifierFactory
                .Setup(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<ChargeType>(), It.IsAny<string>()))
                .ReturnsAsync(It.IsAny<ChargeIdentifier>());
        }

        private static void SetupChargeRepository(
            [Frozen] Mock<IChargeRepository> chargeRepository,
            Charge charge)
        {
            chargeRepository
                .Setup(r => r.SingleOrNullAsync(It.IsAny<ChargeIdentifier>()))!
                .ReturnsAsync(charge);
        }

        private static ValidationResult GetFailedValidationResult()
        {
            var failedRule = new Mock<IValidationRule>();
            failedRule.Setup(r => r.IsValid).Returns(false);

            return ValidationResult.CreateFailure(
                new List<IValidationRuleContainer> { new DocumentValidationRuleContainer(failedRule.Object) });
        }

        private static ChargePriceCommand CreateChargeCommandWith24Points()
        {
            var points = new List<Point>();
            var price = 0.00M;
            var startTime = InstantHelper.GetTodayAtMidnightUtc();
            for (var i = 0; i <= 23; i++)
            {
                points.Add(new Point(price + i, startTime + Duration.FromHours(i)));
            }

            var startDate = points.Min(x => x.Time);
            var endDate = points.Max(x => x.Time) + Duration.FromHours(1);

            var operation = new ChargePriceOperationDtoBuilder()
                .WithPointsInterval(startDate, endDate)
                .WithPoints(points)
                .Build();

            return new ChargePriceCommandBuilder()
                .WithChargeOperation(operation)
                .Build();
        }
    }
}
