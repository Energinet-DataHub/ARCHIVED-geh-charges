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
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Application.ChargeLinks.Services;
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeLinksReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.TestHelpers;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation;
using GreenEnergyHub.TestHelpers;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.ChargeLinks.Handlers
{
    [UnitTest]
    public class ChargeLinksReceivedEventHandlerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_IfEventIsNull_ThrowsArgumentNullException(ChargeLinksReceivedEventHandler sut)
        {
            // Arrange
            ChargeLinksReceivedEvent? receivedEvent = null;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.HandleAsync(receivedEvent!));
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalledWithValidChargeLinksCommand_ShouldDispatchAcceptedEvent(
            [Frozen] Mock<IChargeLinksReceiptService> chargeLinksReceiptService,
            [Frozen] Mock<IChargeLinkFactory> chargeLinkFactory,
            [Frozen] Mock<IChargeLinksAcceptedEventFactory> chargeLinkCommandAcceptedEventFactory,
            [Frozen] Mock<IBusinessValidator<ChargeLinkOperationDto>> businessValidator,
            ChargeLinkDtoBuilder linksDtoBuilder,
            ChargeLinksCommandBuilder linksCommandBuilder,
            ChargeLinksAcceptedEvent chargeLinksAcceptedEvent,
            ChargeLinksReceivedEventHandler sut)
        {
            // Arrange
            var chargeLink = CreateChargeLink();
            var links = new List<ChargeLinkOperationDto> { linksDtoBuilder.Build() };
            var chargeLinksCommand = linksCommandBuilder.WithChargeLinks(links).Build();
            var chargeLinksReceivedEvent = new ChargeLinksReceivedEvent(Instant.MinValue, chargeLinksCommand);

            SetupSuccessfulValidator(businessValidator);
            SetupFactories(chargeLinkFactory, chargeLinkCommandAcceptedEventFactory, chargeLinksAcceptedEvent, chargeLink);

            // Act
            await sut.HandleAsync(chargeLinksReceivedEvent);

            // Assert
            chargeLinksReceiptService.Verify(x => x.AcceptAsync(It.IsAny<ChargeLinksCommand>()));
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalledWithInvalidChargeLinksCommand_ShouldDispatchRejectedEvent(
            [Frozen] Mock<IChargeLinksReceiptService> chargeLinksReceiptService,
            [Frozen] Mock<IChargeLinkFactory> chargeLinkFactory,
            [Frozen] Mock<IBusinessValidator<ChargeLinkOperationDto>> businessValidator,
            ChargeLinkDtoBuilder linksDtoBuilder,
            ChargeLinksCommandBuilder linksCommandBuilder,
            ChargeLinksReceivedEventHandler sut)
        {
            // Arrange
            var chargeLink = CreateChargeLink();
            var links = new List<ChargeLinkOperationDto> { linksDtoBuilder.Build() };
            var chargeLinksCommand = linksCommandBuilder.WithChargeLinks(links).Build();
            var chargeLinksReceivedEvent = new ChargeLinksReceivedEvent(Instant.MinValue, chargeLinksCommand);
            chargeLinkFactory
                .Setup(x => x.CreateAsync(It.IsAny<ChargeLinkOperationDto>()))
                .ReturnsAsync(chargeLink);

            SetupErroneousValidator(businessValidator);

            // Act
            await sut.HandleAsync(chargeLinksReceivedEvent);

            // Assert
            chargeLinksReceiptService.Verify(x => x.RejectAsync(
                It.IsAny<ChargeLinksCommand>(),
                It.IsAny<ValidationResult>()));
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalledWithInvalidChargeLinksCommand_ShouldRejectedAllSubsequentOperations(
            [Frozen] Mock<IChargeLinksReceiptService> chargeLinksReceiptService,
            [Frozen] Mock<IChargeLinkFactory> chargeLinkFactory,
            [Frozen] Mock<IBusinessValidator<ChargeLinkOperationDto>> businessValidator,
            List<ChargeLinkOperationDto> chargeLinkDtos,
            ChargeLinksCommandBuilder linksCommandBuilder,
            ChargeLinksReceivedEventHandler sut)
        {
            // Arrange
            var chargeLink = CreateChargeLink();
            var chargeLinksCommand = linksCommandBuilder.WithChargeLinks(chargeLinkDtos).Build();
            var chargeLinksReceivedEvent = new ChargeLinksReceivedEvent(Instant.MinValue, chargeLinksCommand);
            chargeLinkFactory
                .Setup(x => x.CreateAsync(It.IsAny<ChargeLinkOperationDto>()))
                .ReturnsAsync(chargeLink);
            SetupErroneousValidator(businessValidator);

            var validationResultsArgs = new List<ValidationResult>();
            chargeLinksReceiptService
                .Setup(s => s.RejectAsync(It.IsAny<ChargeLinksCommand>(), It.IsAny<ValidationResult>()))
                .Callback<ChargeLinksCommand, ValidationResult>((_, s) => validationResultsArgs.Add(s));

            // Act
            await sut.HandleAsync(chargeLinksReceivedEvent);

            // Assert
            var validationRules = validationResultsArgs.Single().InvalidRules.ToList();
            var invalid = validationRules.Where(vr =>
                vr.ValidationRule.ValidationRuleIdentifier == ValidationRuleIdentifier.MeteringPointDoesNotExist);
            var subsequent = validationRules.Where(vr =>
                vr.ValidationRule.ValidationRuleIdentifier == ValidationRuleIdentifier.SubsequentBundleOperationsFail);

            validationRules.Count.Should().Be(3);
            invalid.Count().Should().Be(1);
            subsequent.Count().Should().Be(2);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task GivenHandleAsync_WhenValidationFails_ShouldLogValidationErrors(
            Mock<ILoggerFactory> loggerFactory,
            Mock<ILogger> logger,
            Mock<IChargeLinksReceiptService> chargeLinksReceiptService,
            Mock<IChargeLinkFactory> chargeLinkFactory,
            Mock<IChargeLinksRepository> chargeLinksRepository,
            Mock<IBusinessValidator<ChargeLinkOperationDto>> businessValidator,
            Mock<IUnitOfWork> unitOfWork,
            ChargeLinksReceivedEvent receivedEvent)
        {
            // Arrange
            var document = receivedEvent.Command.Document;
            var expectedMessage = TestStringGenerator.CreateExpectedErrorMessage(
                document.Id,
                document.Type.ToString(),
                document.Sender.MarketParticipantId,
                ValidationRuleIdentifier.StartDateValidation.ToString(),
                receivedEvent.Command.Operations.Count - 1);

            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            businessValidator
                .Setup(b => b.ValidateAsync(It.IsAny<ChargeLinkOperationDto>()))
                .ReturnsAsync(GetFailedValidationResult(ValidationRuleIdentifier.StartDateValidation));

            var sut = new ChargeLinksReceivedEventHandler(
                loggerFactory.Object,
                chargeLinksReceiptService.Object,
                chargeLinkFactory.Object,
                chargeLinksRepository.Object,
                businessValidator.Object,
                unitOfWork.Object);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            logger.VerifyLoggerWasCalled(expectedMessage, LogLevel.Error);
        }

        private static ChargeLink CreateChargeLink()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            fixture.Customizations.Add(new StringGenerator(() => Guid.NewGuid().ToString()[..16]));
            return fixture.Create<ChargeLink>();
        }

        private static void SetupSuccessfulValidator(Mock<IBusinessValidator<ChargeLinkOperationDto>> businessValidator)
        {
            businessValidator.Setup(x => x.ValidateAsync(It.IsAny<ChargeLinkOperationDto>()))
                .ReturnsAsync(ValidationResult.CreateSuccess());
        }

        private static void SetupErroneousValidator(Mock<IBusinessValidator<ChargeLinkOperationDto>> businessValidator)
        {
            businessValidator.Setup(x => x.ValidateAsync(It.IsAny<ChargeLinkOperationDto>()))
                .ReturnsAsync(ValidationResult.CreateFailure(new List<IValidationRuleContainer>
                {
                    new OperationValidationRuleContainer(
                        new TestValidationRule(false, ValidationRuleIdentifier.MeteringPointDoesNotExist),
                        "testOperationId"),
                }));
        }

        private static void SetupFactories(
            Mock<IChargeLinkFactory> chargeLinkFactory,
            Mock<IChargeLinksAcceptedEventFactory> chargeLinkCommandAcceptedEventFactory,
            ChargeLinksAcceptedEvent chargeLinksAcceptedEvent,
            ChargeLink chargeLink)
        {
            chargeLinkFactory
                .Setup(x => x.CreateAsync(It.IsAny<ChargeLinkOperationDto>()))
                .ReturnsAsync(chargeLink);

            chargeLinkCommandAcceptedEventFactory
                .Setup(x => x.Create(It.IsAny<ChargeLinksCommand>()))
                .Returns(chargeLinksAcceptedEvent);
        }

        private static ValidationResult GetFailedValidationResult(ValidationRuleIdentifier validationRuleIdentifier)
        {
            var failedRule = new Mock<IValidationRule>();
            failedRule.Setup(r => r.IsValid).Returns(false);
            failedRule.Setup(r => r.ValidationRuleIdentifier).Returns(validationRuleIdentifier);

            return ValidationResult.CreateFailure(
                new List<IValidationRuleContainer> { new DocumentValidationRuleContainer(failedRule.Object) });
        }
    }
}
