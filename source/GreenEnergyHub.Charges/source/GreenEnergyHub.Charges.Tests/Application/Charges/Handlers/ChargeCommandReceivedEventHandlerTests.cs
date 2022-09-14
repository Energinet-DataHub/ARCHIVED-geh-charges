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
using GreenEnergyHub.Charges.Application.Charges.Acknowledgement;
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Handlers
{
    [UnitTest]
    public class ChargeCommandReceivedEventHandlerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenBusinessReasonCodeIsUpdateChargeInformation_ActivateHandler(
            ChargeInformationCommandReceivedEvent chargeInformationCommandReceivedEvent,
            [Frozen] Mock<IChargeInformationEventHandler> chargeCommandReceivedEventHandlerMock,
            [Frozen] Mock<IDocumentValidator> documentValidator,
            ChargeInformationCommandReceivedEventHandler sut)
        {
            // Arrange
            documentValidator
                .Setup(v => v.ValidateAsync(It.IsAny<ChargeInformationCommand>()))
                .ReturnsAsync(ValidationResult.CreateSuccess());
            chargeInformationCommandReceivedEvent.Command.Document.BusinessReasonCode = BusinessReasonCode.UpdateChargeInformation;

            // Act
            await sut.HandleAsync(chargeInformationCommandReceivedEvent);

            // Assert
            chargeCommandReceivedEventHandlerMock.Verify(
                x => x.HandleAsync(chargeInformationCommandReceivedEvent),
                Times.Once);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenDocumentValidationFails_ShouldCallReject(
            ChargeInformationCommandReceivedEvent chargeInformationCommandReceivedEvent,
            [Frozen] Mock<IDocumentValidator> documentValidator,
            [Frozen] Mock<IChargeCommandReceiptService> chargeCommandReceiptService,
            ChargeInformationCommandReceivedEventHandler sut)
        {
            // Arrange
            documentValidator.Setup(v =>
                    v.ValidateAsync(It.IsAny<ChargeInformationCommand>()))
                .ReturnsAsync(ValidationResult.CreateFailure(GetFailedValidationResult()));
            chargeInformationCommandReceivedEvent.Command.Document.BusinessReasonCode = BusinessReasonCode.UpdateChargeInformation;

            // Act
            await sut.HandleAsync(chargeInformationCommandReceivedEvent);

            // Assert
            chargeCommandReceiptService.Verify(
                x => x.RejectAsync(chargeInformationCommandReceivedEvent.Command, It.IsAny<ValidationResult>()),
                Times.Once);
        }

        private static List<IValidationRuleContainer> GetFailedValidationResult()
        {
            var failedRule = new Mock<IValidationRule>();
            failedRule.Setup(r => r.IsValid).Returns(false);

            return new List<IValidationRuleContainer> { new DocumentValidationRuleContainer(failedRule.Object) };
        }
    }
}
