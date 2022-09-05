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

using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Application.Charges.Events;
using GreenEnergyHub.Charges.Application.Charges.Factories;
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Application.Common.Services;
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Messages.Command;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.TestCore.TestHelpers;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Handlers
{
    [UnitTest]
    public class ChargePriceCommandReceivedEventHandlerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenUpdatingValidChargePrice_ShouldActivateHandler(
            [Frozen] Mock<IDocumentValidator> documentValidator,
            [Frozen] Mock<IChargePriceEventHandler> chargePriceEventHandler,
            [Frozen] Mock<IUnitOfWork> unitOfWork,
            ChargePriceCommandReceivedEvent chargeCommandReceivedEvent,
            ChargePriceCommandReceivedEventHandler sut)
        {
            // Arrange
            documentValidator
                .Setup(v => v.ValidateAsync(It.IsAny<ChargePriceCommand>()))
                .ReturnsAsync(ValidationResult.CreateSuccess());

            // Act
            await sut.HandleAsync(chargeCommandReceivedEvent);

            // Assert
            chargePriceEventHandler.Verify(x => x.HandleAsync(chargeCommandReceivedEvent), Times.Once);
            unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenDocumentValidationFails_ShouldCallReject(
            ChargePriceCommandReceivedEvent chargePriceCommandReceivedEvent,
            [Frozen] Mock<IDocumentValidator> documentValidator,
            [Frozen] Mock<IChargePriceEventHandler> chargePriceEventHandler,
            [Frozen] Mock<IDomainEventPublisher> chargePriceRejectionService,
            [Frozen] Mock<IPriceRejectedEventFactory> priceRejectedEventFactory,
            ChargePriceCommandReceivedEventHandler sut,
            PriceRejectedEventBuilder priceRejectedEventBuilder)
        {
            // Arrange
            var priceRejectedEvent = priceRejectedEventBuilder.Build();
            documentValidator.Setup(v =>
                    v.ValidateAsync(It.IsAny<ChargePriceCommand>()))
                .ReturnsAsync(ValidationResult.CreateFailure(GetFailedValidationResult(
                    ValidationRuleIdentifier.BusinessReasonCodeMustBeUpdateChargeInformationOrChargePrices)));

            priceRejectedEventFactory
                .Setup(c => c.Create(
                    It.IsAny<DocumentDto>(),
                    It.IsAny<IReadOnlyCollection<ChargePriceOperationDto>>(),
                    It.IsAny<ValidationResult>()))
                .Returns(priceRejectedEvent);

            // Act
            await sut.HandleAsync(chargePriceCommandReceivedEvent);

            // Assert
            chargePriceRejectionService.Verify(
                x => x.Publish(priceRejectedEvent), Times.Once);
            chargePriceEventHandler.Verify(x => x.HandleAsync(chargePriceCommandReceivedEvent), Times.Never);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task GivenHandleAsync_WhenValidationFails_ShouldLogValidationErrors(
            Mock<ILoggerFactory> loggerFactory,
            Mock<ILogger> logger,
            Mock<IDocumentValidator> documentValidator,
            Mock<IPriceRejectedEventFactory> priceRejectedEventFactory,
            Mock<IChargePriceEventHandler> chargePriceEventHandler,
            Mock<IDomainEventPublisher> domainEventPublisher,
            PriceRejectedEvent rejectedEvent,
            ChargePriceCommandReceivedEvent chargePriceCommandReceivedEvent)
        {
            // Arrange
            var document = chargePriceCommandReceivedEvent.Command.Document;
            var expectedMessage = ErrorTextGenerator.CreateExpectedErrorMessage(
                document.Id,
                document.Type.ToString(),
                document.Sender.MarketParticipantId,
                ValidationRuleIdentifier.BusinessReasonCodeMustBeUpdateChargeInformationOrChargePrices.ToString(),
                0);

            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            priceRejectedEventFactory
                .Setup(c => c.Create(
                    It.IsAny<DocumentDto>(),
                    It.IsAny<IReadOnlyCollection<ChargePriceOperationDto>>(),
                    It.IsAny<ValidationResult>()))
                .Returns(rejectedEvent);
            documentValidator
                .Setup(d => d.ValidateAsync(It.IsAny<ChargeCommand>()))
                .ReturnsAsync(
                    ValidationResult.CreateFailure(GetFailedValidationResult(
                        ValidationRuleIdentifier.BusinessReasonCodeMustBeUpdateChargeInformationOrChargePrices)));

            var sut = new ChargePriceCommandReceivedEventHandler(
                loggerFactory.Object,
                chargePriceEventHandler.Object,
                documentValidator.Object,
                domainEventPublisher.Object,
                priceRejectedEventFactory.Object);

            // Act
            await sut.HandleAsync(chargePriceCommandReceivedEvent);

            // Assert
            logger.VerifyLoggerWasCalled(expectedMessage, LogLevel.Error);
        }

        private static List<IValidationRuleContainer> GetFailedValidationResult(ValidationRuleIdentifier validationRuleIdentifier)
        {
            var failedRule = new Mock<IValidationRule>();
            failedRule.Setup(r => r.IsValid).Returns(false);
            failedRule.Setup(r => r.ValidationRuleIdentifier).Returns(validationRuleIdentifier);

            return new List<IValidationRuleContainer> { new DocumentValidationRuleContainer(failedRule.Object) };
        }
    }
}
