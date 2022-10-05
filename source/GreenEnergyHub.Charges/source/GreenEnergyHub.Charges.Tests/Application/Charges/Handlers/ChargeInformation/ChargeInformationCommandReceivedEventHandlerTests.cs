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
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Charges.Factories;
using GreenEnergyHub.Charges.Application.Charges.Handlers.ChargeInformation;
using GreenEnergyHub.Charges.Application.Common.Services;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Events;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Handlers.ChargeInformation
{
    [UnitTest]
    public class ChargeInformationCommandReceivedEventHandlerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenBusinessReasonCodeIsUpdateChargeInformation_ActivateHandler(
            [Frozen] Mock<IChargeInformationOperationsHandler> chargeCommandReceivedEventHandlerMock,
            [Frozen] Mock<IDocumentValidator> documentValidator,
            ChargeInformationCommandReceivedEventHandler sut)
        {
            // Arrange
            documentValidator
                .Setup(v => v.ValidateAsync(It.IsAny<ChargeInformationCommand>()))
                .ReturnsAsync(ValidationResult.CreateSuccess());
            var chargeInformationCommandReceivedEvent = BuildEvent();

            // Act
            await sut.HandleAsync(chargeInformationCommandReceivedEvent);

            // Assert
            chargeCommandReceivedEventHandlerMock.Verify(
                x => x.HandleAsync(chargeInformationCommandReceivedEvent),
                Times.Once);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenDocumentValidationFails_ShouldRaiseRejectedEvent(
            [Frozen] Mock<IDocumentValidator> documentValidator,
            [Frozen] Mock<IDomainEventPublisher> domainEventPublisher,
            [Frozen] Mock<IChargeInformationOperationsRejectedEventFactory> chargeInformationOperationsRejectedEventFactory,
            ChargeInformationOperationsRejectedEvent chargeInformationOperationsRejectedEvent,
            ChargeInformationCommandReceivedEventHandler sut)
        {
            // Arrange
            SetupEventFactory(chargeInformationOperationsRejectedEventFactory, chargeInformationOperationsRejectedEvent);
            SetupDocumentValidator(documentValidator);
            var chargeInformationCommandReceivedEvent = BuildEvent();
            ChargeInformationOperationsRejectedEvent actualRejectedEvent = null!;
            domainEventPublisher
                .Setup(d => d.Publish(It.IsAny<ChargeInformationOperationsRejectedEvent>()))
                .Callback<ChargeInformationOperationsRejectedEvent>(e => actualRejectedEvent = e);

            // Act
            await sut.HandleAsync(chargeInformationCommandReceivedEvent);

            // Assert
            domainEventPublisher.Verify(
                x => x.Publish(actualRejectedEvent),
                Times.Once);
            actualRejectedEvent.Should().BeEquivalentTo(chargeInformationOperationsRejectedEvent);
        }

        private ChargeInformationCommandReceivedEvent BuildEvent()
        {
            var documentDto = new DocumentDtoBuilder()
                .WithBusinessReasonCode(BusinessReasonCode.UpdateChargeInformation)
                .Build();
            var command = new ChargeInformationCommandBuilder()
                .WithDocumentDto(documentDto)
                .Build();
            return new ChargeInformationCommandReceivedEventBuilder()
                .WithCommand(command)
                .Build();
        }

        private static void SetupDocumentValidator(Mock<IDocumentValidator> documentValidator)
        {
            documentValidator.Setup(v =>
                    v.ValidateAsync(It.IsAny<ChargeInformationCommand>()))
                .ReturnsAsync(ValidationResult.CreateFailure(GetFailedValidationResult()));
        }

        private static void SetupEventFactory(
            Mock<IChargeInformationOperationsRejectedEventFactory> chargeInformationOperationsRejectedEventFactory,
            ChargeInformationOperationsRejectedEvent chargeInformationOperationsRejectedEvent)
        {
            chargeInformationOperationsRejectedEventFactory
                .Setup(c => c.Create(
                    It.IsAny<DocumentDto>(),
                    It.IsAny<IReadOnlyCollection<ChargeInformationOperationDto>>(),
                    It.IsAny<ValidationResult>()))
                .Returns(chargeInformationOperationsRejectedEvent);
        }

        private static List<IValidationRuleContainer> GetFailedValidationResult()
        {
            var failedRule = new Mock<IValidationRule>();
            failedRule.Setup(r => r.IsValid).Returns(false);

            return new List<IValidationRuleContainer> { new DocumentValidationRuleContainer(failedRule.Object) };
        }
    }
}
