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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Application.Charges.Acknowledgement;
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Handlers
{
    [UnitTest]
    public class ChargeHandlerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenBusinessReasonCodeIsUpdateChargePrices_SendConfirmation(
            ChargeCommandReceivedEvent chargeCommandReceivedEvent,
            ChargeCommandAcceptedEvent chargeCommandAcceptedEvent,
            [Frozen] Mock<IMessageDispatcher<ChargeCommandAcceptedEvent>> messageDispatcherMock,
            [Frozen] Mock<IChargeCommandAcceptedEventFactory> chargeCommandAcceptedEventFactoryMock,
            [Frozen] Mock<IDocumentValidator<ChargeCommand>> documentValidator,
            ChargeHandler sut)
        {
            // Arrange
            chargeCommandReceivedEvent.Command.Document.BusinessReasonCode = BusinessReasonCode.UpdateChargePrices;
            chargeCommandAcceptedEventFactoryMock
                .Setup(x => x.CreateEvent(chargeCommandReceivedEvent.Command))
                .Returns(chargeCommandAcceptedEvent);
            documentValidator.Setup(v =>
                v.ValidateAsync(It.IsAny<ChargeCommand>())).ReturnsAsync(ValidationResult.CreateSuccess());

            // Act
            await sut.HandleAsync(chargeCommandReceivedEvent);

            // Assert
            messageDispatcherMock.Verify(
                x => x.DispatchAsync(
                    It.IsAny<ChargeCommandAcceptedEvent>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenBusinessReasonCodeIsUpdateChargeInformation_ActivateHandler(
            ChargeCommandReceivedEvent chargeCommandReceivedEvent,
            [Frozen] Mock<IChargeCommandReceivedEventHandler> chargeCommandReceivedEventHandlerMock,
            [Frozen] Mock<IDocumentValidator<ChargeCommand>> documentValidator,
            ChargeHandler sut)
        {
            // Arrange
            documentValidator
                .Setup(v => v.ValidateAsync(It.IsAny<ChargeCommand>()))
                .ReturnsAsync(ValidationResult.CreateSuccess());
            chargeCommandReceivedEvent.Command.Document.BusinessReasonCode = BusinessReasonCode.UpdateChargeInformation;

            // Act
            await sut.HandleAsync(chargeCommandReceivedEvent);

            // Assert
            chargeCommandReceivedEventHandlerMock.Verify(
                x => x.HandleAsync(chargeCommandReceivedEvent),
                Times.Once);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenDocumentValidationFails_ShouldCallReject(
            ChargeCommandReceivedEvent chargeCommandReceivedEvent,
            [Frozen] Mock<IDocumentValidator<ChargeCommand>> documentValidator,
            [Frozen] Mock<IChargeCommandReceiptService> chargeCommandReceiptService,
            ChargeHandler sut)
        {
            // Arrange
            documentValidator.Setup(v =>
                    v.ValidateAsync(It.IsAny<ChargeCommand>()))
                .ReturnsAsync(ValidationResult.CreateFailure(GetFailedValidationResult()));
            chargeCommandReceivedEvent.Command.Document.BusinessReasonCode = BusinessReasonCode.UpdateChargeInformation;

            // Act
            await sut.HandleAsync(chargeCommandReceivedEvent);

            // Assert
            chargeCommandReceiptService.Verify(
                x => x.RejectAsync(chargeCommandReceivedEvent.Command, It.IsAny<ValidationResult>()),
                Times.Once);
        }

        private static List<IValidationRule> GetFailedValidationResult()
        {
            var failedRule = new Mock<IValidationRule>();
            failedRule.Setup(r => r.IsValid).Returns(false);

            return new List<IValidationRule> { failedRule.Object };
        }
    }
}
