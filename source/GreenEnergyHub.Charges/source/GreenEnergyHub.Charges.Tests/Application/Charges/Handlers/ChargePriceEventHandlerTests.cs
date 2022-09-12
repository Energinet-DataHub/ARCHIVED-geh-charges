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
using GreenEnergyHub.Charges.Application.Common.Services;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.TestHelpers;
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
        public async Task HandleAsync_WhenValidationSucceed_ThenConfirmedEventPublished(
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IInputValidator<ChargePriceOperationDto>> inputValidator,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            TestMarketParticipant sender,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IInternalEventPublisher> domainEventPublisher,
            [Frozen] Mock<ILoggerFactory> loggerFactory,
            [Frozen] Mock<IPriceRejectedEventFactory> priceRejectedEventFactory,
            [Frozen] Mock<IPriceConfirmedEventFactory> priceConfirmedEventFactory,
            Mock<ILogger> logger,
            ChargeBuilder chargeBuilder,
            PriceConfirmedEventBuilder confirmedEventBuilder)
        {
            // Arrange
            var priceConfirmedEvent = confirmedEventBuilder.Build();
            var chargeCommand = CreateChargeCommandWith24Points();
            var receivedEvent = new ChargePriceCommandReceivedEvent(InstantHelper.GetTodayAtMidnightUtc(), chargeCommand);
            var validationResult = ValidationResult.CreateSuccess();
            inputValidator
                .Setup(v => v.Validate(It.IsAny<ChargePriceOperationDto>(), It.IsAny<DocumentDto>())).Returns(validationResult);
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
            priceConfirmedEventFactory
                .Setup(c => c.Create(
                    It.IsAny<DocumentDto>(),
                    It.IsAny<IReadOnlyCollection<ChargePriceOperationDto>>()))
                .Returns(priceConfirmedEvent);

            var sut = new ChargePriceEventHandler(
                chargeRepository.Object,
                marketParticipantRepository.Object,
                inputValidator.Object,
                domainEventPublisher.Object,
                loggerFactory.Object,
                priceConfirmedEventFactory.Object,
                priceRejectedEventFactory.Object);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            domainEventPublisher
                .Verify(x => x.Publish(It.IsAny<PriceRejectedEvent>()), Times.Never);
            domainEventPublisher
                .Verify(x => x.Publish(priceConfirmedEvent), Times.Once);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidationFails_ThenRejectedEventRaised(
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IInputValidator<ChargePriceOperationDto>> inputValidator,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IInternalEventPublisher> chargePriceRejectionService,
            [Frozen] Mock<IPriceRejectedEventFactory> priceRejectedEventFactory,
            TestMarketParticipant sender,
            ChargeBuilder chargeBuilder,
            ChargePriceCommandReceivedEvent receivedEvent,
            PriceRejectedEvent priceRejectedEvent,
            ChargePriceEventHandler sut)
        {
            // Arrange
            var validationResult = GetFailedValidationResult(It.IsAny<ValidationRuleIdentifier>());
            inputValidator.Setup(v => v.Validate(It.IsAny<ChargePriceOperationDto>(), It.IsAny<DocumentDto>())).Returns(validationResult);
            var charge = chargeBuilder.Build();

            priceRejectedEventFactory
                .Setup(c => c.Create(
                    It.IsAny<DocumentDto>(),
                    It.IsAny<List<ChargePriceOperationDto>>(),
                    It.IsAny<ValidationResult>()))
                .Returns(priceRejectedEvent);
            SetupChargeRepository(chargeRepository, charge);
            SetupMarketParticipantRepository(marketParticipantRepository, sender);
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            chargePriceRejectionService.Verify(
                x => x.Publish(It.IsAny<PriceRejectedEvent>()), Times.Once);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidationFails_ThenValidationErrorsAreLogged(
            Mock<ILoggerFactory> loggerFactory,
            Mock<IInputValidator<ChargePriceOperationDto>> inputValidator,
            Mock<IInternalEventPublisher> domainEventPublisher,
            Mock<IPriceConfirmedEventFactory> priceConfirmedEventFactory,
            Mock<IPriceRejectedEventFactory> priceRejectedEventFactory,
            Mock<ILogger> logger,
            PriceRejectedEvent rejectedEvent,
            ChargePriceCommandReceivedEvent commandReceivedEvent)
        {
            // Arrange
            loggerFactory.Setup(l => l.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            var document = commandReceivedEvent.Command.Document;
            var validationResult = GetFailedValidationResult(
                ValidationRuleIdentifier.BusinessReasonCodeMustBeUpdateChargeInformationOrChargePrices);
            var expectedMessage = ErrorTextGenerator.CreateExpectedErrorMessage(
                document.Id,
                document.Type.ToString(),
                document.Sender.MarketParticipantId,
                ValidationRuleIdentifier.BusinessReasonCodeMustBeUpdateChargeInformationOrChargePrices.ToString(),
                commandReceivedEvent.Command.Operations.Count - 1);
            inputValidator
                .Setup(v => v.Validate(
                    It.IsAny<ChargePriceOperationDto>(),
                    It.IsAny<DocumentDto>()))
                .Returns(validationResult);
            priceRejectedEventFactory
                .Setup(c => c.Create(
                    It.IsAny<DocumentDto>(),
                    It.IsAny<IReadOnlyCollection<ChargePriceOperationDto>>(),
                    It.IsAny<ValidationResult>()))
                .Returns(rejectedEvent);
            var sut = new ChargePriceEventHandler(
                It.IsAny<IChargeRepository>(),
                It.IsAny<IMarketParticipantRepository>(),
                inputValidator.Object,
                domainEventPublisher.Object,
                loggerFactory.Object,
                priceConfirmedEventFactory.Object,
                priceRejectedEventFactory.Object);

            // Act
            await sut.HandleAsync(commandReceivedEvent);

            // Assert
            logger.VerifyLoggerWasCalled(expectedMessage, LogLevel.Error);
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
                .Setup(x => x.CreateAsync(
                    It.IsAny<string>(),
                    It.IsAny<ChargeType>(),
                    It.IsAny<string>()))
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

        private static ValidationResult GetFailedValidationResult(ValidationRuleIdentifier validationRuleIdentifier)
        {
            var failedRule = new Mock<IValidationRule>();
            failedRule.Setup(r => r.IsValid).Returns(false);
            failedRule.Setup(r => r.ValidationRuleIdentifier).Returns(validationRuleIdentifier);

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
