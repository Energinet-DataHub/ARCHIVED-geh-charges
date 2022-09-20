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
using GreenEnergyHub.Charges.Application.Charges.Factories;
using GreenEnergyHub.Charges.Application.Charges.Handlers.ChargePrice;
using GreenEnergyHub.Charges.Application.Common.Services;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Events;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using GreenEnergyHub.Charges.TestCore.Builders.Testables;
using GreenEnergyHub.Charges.TestCore.TestHelpers;
using GreenEnergyHub.TestHelpers;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Handlers.ChargePrice
{
    [UnitTest]
    public class ChargePriceOperationsHandlerTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidationSucceed_ThenAcceptedEventPublished(
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IInputValidator<ChargePriceOperationDto>> inputValidator,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IDomainEventPublisher> domainEventPublisher,
            [Frozen] Mock<ILoggerFactory> loggerFactory,
            [Frozen] Mock<IChargePriceOperationsRejectedEventFactory> chargePriceOperationsRejectedEventFactory,
            [Frozen] Mock<IChargePriceOperationsAcceptedEventFactory> chargePriceOperationsAcceptedEventFactory,
            Mock<ILogger> logger,
            TestMarketParticipant sender,
            ChargeBuilder chargeBuilder,
            ChargePriceOperationsAcceptedEventBuilder chargePriceOperationsAcceptedEventBuilder)
        {
            // Arrange
            var chargePriceOperationsAcceptedEvent = chargePriceOperationsAcceptedEventBuilder.Build();
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
            var charge = chargeBuilder
                .WithPoints(points)
                .WithTaxIndicator(TaxIndicator.NoTax)
                .WithMarketParticipantRole(MarketParticipantRole.SystemOperator)
                .Build();
            SetupChargeRepository(chargeRepository, charge);
            SetupMarketParticipantRepository(marketParticipantRepository, sender);
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);

            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            chargePriceOperationsConfirmedEventFactory
                .Setup(c => c.Create(
                    It.IsAny<DocumentDto>(),
                    It.IsAny<IReadOnlyCollection<ChargePriceOperationDto>>()))
                .Returns(chargePriceOperationsAcceptedEvent);
            var sut = new ChargePriceOperationsHandler(
                chargeRepository.Object,
                marketParticipantRepository.Object,
                inputValidator.Object,
                domainEventPublisher.Object,
                loggerFactory.Object,
                chargePriceOperationsConfirmedEventFactory.Object,
                chargePriceOperationsRejectedEventFactory.Object);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            domainEventPublisher
                .Verify(x => x.Publish(It.IsAny<ChargePriceOperationsRejectedEvent>()), Times.Never);
            domainEventPublisher
                .Verify(x => x.Publish(chargePriceOperationsAcceptedEvent), Times.Once);
            domainEventPublisher.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidationFails_ThenRejectedEventRaised(
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IInputValidator<ChargePriceOperationDto>> inputValidator,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IDomainEventPublisher> domainEventPublisher,
            [Frozen] Mock<IChargePriceOperationsRejectedEventFactory> chargePriceOperationsRejectedEventFactory,
            TestMarketParticipant sender,
            ChargeBuilder chargeBuilder,
            ChargePriceCommandReceivedEvent receivedEvent,
            ChargePriceOperationsRejectedEvent chargePriceOperationsRejectedEvent,
            ChargePriceOperationsHandler sut)
        {
            // Arrange
            var validationResult = GetFailedValidationResult(It.IsAny<ValidationRuleIdentifier>());
            inputValidator.Setup(v => v.Validate(It.IsAny<ChargePriceOperationDto>(), It.IsAny<DocumentDto>())).Returns(validationResult);
            var charge = chargeBuilder.Build();

            chargePriceOperationsRejectedEventFactory
                .Setup(c => c.Create(
                    It.IsAny<DocumentDto>(),
                    It.IsAny<List<ChargePriceOperationDto>>(),
                    It.IsAny<ValidationResult>()))
                .Returns(chargePriceOperationsRejectedEvent);
            SetupChargeRepository(chargeRepository, charge);
            SetupMarketParticipantRepository(marketParticipantRepository, sender);
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            domainEventPublisher.Verify(
                x => x.Publish(It.IsAny<ChargePriceOperationsRejectedEvent>()), Times.Once);
            domainEventPublisher.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidationFails_ThenValidationErrorsAreLogged(
            Mock<ILoggerFactory> loggerFactory,
            Mock<IInputValidator<ChargePriceOperationDto>> inputValidator,
            Mock<IDomainEventPublisher> domainEventPublisher,
            Mock<IChargePriceOperationsAcceptedEventFactory> chargePriceOperationsAcceptedEventFactory,
            Mock<IChargePriceOperationsRejectedEventFactory> chargePriceOperationsRejectedEventFactory,
            Mock<ILogger> logger,
            ChargePriceOperationsRejectedEvent operationsRejectedEvent,
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
            chargePriceOperationsRejectedEventFactory
                .Setup(c => c.Create(
                    It.IsAny<DocumentDto>(),
                    It.IsAny<IReadOnlyCollection<ChargePriceOperationDto>>(),
                    It.IsAny<ValidationResult>()))
                .Returns(operationsRejectedEvent);
            var sut = new ChargePriceOperationsHandler(
                It.IsAny<IChargeRepository>(),
                It.IsAny<IMarketParticipantRepository>(),
                inputValidator.Object,
                domainEventPublisher.Object,
                loggerFactory.Object,
                chargePriceOperationsAcceptedEventFactory.Object,
                chargePriceOperationsRejectedEventFactory.Object);

            // Act
            await sut.HandleAsync(commandReceivedEvent);

            // Assert
            logger.VerifyLoggerWasCalled(expectedMessage, LogLevel.Error);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_IfEventIsNull_ThrowsArgumentNullException(
            ChargePriceOperationsHandler sut)
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
