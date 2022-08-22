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
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Application.Charges.Events;
using GreenEnergyHub.Charges.Application.Charges.Factories;
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Application.Common.Services;
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargePriceCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.TestHelpers;
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
            [Frozen] Mock<IChargeEventFactory> chargeEventFactory,
            ChargePriceCommandReceivedEventHandler sut,
            PriceRejectedEvent priceRejectedEvent)
        {
            // Arrange
            documentValidator.Setup(v =>
                    v.ValidateAsync(It.IsAny<ChargePriceCommand>()))
                .ReturnsAsync(ValidationResult.CreateFailure(GetFailedValidationResult()));

            chargeEventFactory
                .Setup(c => c.CreatePriceRejectedEvent(
                    It.IsAny<DocumentDto>(),
                    It.IsAny<List<ChargePriceOperationDto>>(),
                    It.IsAny<ValidationResult>()))
                .Returns(priceRejectedEvent);

            // Act
            await sut.HandleAsync(chargePriceCommandReceivedEvent);

            // Assert
            chargePriceRejectionService.Verify(
                x => x.Publish(It.IsAny<PriceRejectedEvent>()), Times.Once);
            chargePriceEventHandler.Verify(x => x.HandleAsync(chargePriceCommandReceivedEvent), Times.Never);
        }

        private static List<IValidationRuleContainer> GetFailedValidationResult()
        {
            var failedRule = new Mock<IValidationRule>();
            failedRule.Setup(r => r.IsValid).Returns(false);

            return new List<IValidationRuleContainer> { new DocumentValidationRuleContainer(failedRule.Object) };
        }
    }
}
