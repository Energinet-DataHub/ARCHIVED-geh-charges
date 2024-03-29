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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Charges.Factories;
using GreenEnergyHub.Charges.Application.Charges.Handlers.ChargeInformation;
using GreenEnergyHub.Charges.Application.Common.Services;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Events;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Builders.Command;
using GreenEnergyHub.Charges.TestCore.Builders.Testables;
using GreenEnergyHub.Charges.TestCore.TestHelpers;
using GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Handlers.ChargeInformation
{
    [UnitTest]
    public class ChargeInformationOperationsHandlerTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidationSucceed_ThenConfirmedEventRaised(
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IInputValidator<ChargeInformationOperationDto>> inputValidator,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IChargeFactory> chargeFactory,
            [Frozen] Mock<IChargePeriodFactory> chargePeriodFactory,
            [Frozen] Mock<IDomainEventPublisher> domainEventPublisher,
            [Frozen] Mock<ILoggerFactory> loggerFactory,
            [Frozen] Mock<IChargeInformationOperationsRejectedEventFactory> chargeInformationOperationsRejectedEventFactory,
            [Frozen] Mock<IChargeInformationOperationsAcceptedEventFactory> chargeInformationOperationsAcceptedEventFactory,
            Mock<ILogger> logger,
            TestMarketParticipant sender,
            ChargeBuilder chargeBuilder,
            ChargeInformationCommandBuilder chargeCommandBuilder,
            ChargeInformationOperationDtoBuilder chargeInformationOperationDtoBuilder,
            ChargeInformationOperationsAcceptedEventBuilder chargeInformationOperationsAcceptedEventBuilder)
        {
            // // Arrange
            var chargeInformationOperationsAcceptedEvent = chargeInformationOperationsAcceptedEventBuilder.Build();
            loggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(logger.Object);
            SetupValidators(inputValidator, ValidationResult.CreateSuccess());
            SetupMarketParticipantRepository(marketParticipantRepository, sender);
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);
            chargeRepository
                .Setup(r => r.SingleOrNullAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(null as Charge);
            var charge = chargeBuilder.AddPeriod(CreateValidPeriod()).Build();
            chargeFactory
                .Setup(s => s.CreateFromChargeOperationDtoAsync(It.IsAny<ChargeInformationOperationDto>()))
                .ReturnsAsync(charge);
            chargePeriodFactory
                 .Setup(s => s.CreateFromChargeOperationDto(It.IsAny<ChargeInformationOperationDto>()))
                 .Returns(CreateValidPeriod(30));
            var createOperationDto = chargeInformationOperationDtoBuilder
                .WithStartDateTime(InstantHelper.GetTodayAtMidnightUtc())
                .WithEndDateTime(InstantHelper.GetEndDefault())
                .Build();
            var chargeCommand = chargeCommandBuilder.WithChargeOperation(createOperationDto).Build();
            var chargeInformationCommandReceivedEvent = new ChargeInformationCommandReceivedEvent(
                InstantHelper.GetTodayAtMidnightUtc(),
                chargeCommand);
            chargeInformationOperationsAcceptedEventFactory
                .Setup(c => c.Create(
                    It.IsAny<DocumentDto>(),
                    It.IsAny<IReadOnlyCollection<ChargeInformationOperationDto>>()))
                .Returns(chargeInformationOperationsAcceptedEvent);

            var sut = new ChargeInformationOperationsHandler(
                inputValidator.Object,
                chargeRepository.Object,
                marketParticipantRepository.Object,
                chargeFactory.Object,
                chargePeriodFactory.Object,
                domainEventPublisher.Object,
                loggerFactory.Object,
                chargeInformationOperationsAcceptedEventFactory.Object,
                chargeInformationOperationsRejectedEventFactory.Object);

            // Act
            await sut.HandleAsync(chargeInformationCommandReceivedEvent);

            // Assert
            chargeRepository
                .Verify(x => x.AddAsync(charge), Times.Once);
            domainEventPublisher
                .Verify(x => x.Publish(It.IsAny<ChargeInformationOperationsRejectedEvent>()), Times.Never);
            domainEventPublisher
                .Verify(x => x.Publish(chargeInformationOperationsAcceptedEvent), Times.Once);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidationFails_ThenRejectedEventRaised(
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IInputValidator<ChargeInformationOperationDto>> inputValidator,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IDomainEventPublisher> domainEventPublisher,
            [Frozen] Mock<IChargeInformationOperationsRejectedEventFactory> chargeInformationOperationsRejectedEventFactory,
            [Frozen] Mock<IChargeInformationOperationsAcceptedEventFactory> chargeInformationOperationsAcceptedEventFactory,
            TestMarketParticipant sender,
            ChargeBuilder chargeBuilder,
            ChargeInformationCommandReceivedEvent receivedEvent,
            ChargeInformationOperationsRejectedEvent chargeInformationOperationsRejectedEvent,
            ChargeInformationOperationsHandler sut)
        {
            // Arrange
            var charge = chargeBuilder.Build();
            var validationResult = GetFailedValidationResult();
            SetupValidators(inputValidator, validationResult);
            SetupMarketParticipantRepository(marketParticipantRepository, sender);
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);

            chargeRepository
                .Setup(r => r.SingleOrNullAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(charge);

            chargeInformationOperationsRejectedEventFactory
                .Setup(c => c.Create(
                    It.IsAny<DocumentDto>(),
                    It.IsAny<List<ChargeInformationOperationDto>>(),
                    It.IsAny<ValidationResult>()))
                .Returns(chargeInformationOperationsRejectedEvent);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            domainEventPublisher
                .Verify(x => x.Publish(It.IsAny<ChargeInformationOperationsRejectedEvent>()), Times.Once);
            domainEventPublisher.VerifyNoOtherCalls();
            chargeInformationOperationsAcceptedEventFactory.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_IfEventIsNull_ThrowsArgumentNullException(
            ChargeInformationOperationsHandler sut)
        {
            // Arrange
            ChargeInformationCommandReceivedEvent? receivedEvent = null;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.HandleAsync(receivedEvent!));
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_IfValidUpdateEvent_ChargeUpdated(
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IChargePeriodFactory> chargePeriodFactory,
            [Frozen] Mock<IInputValidator<ChargeInformationOperationDto>> inputValidator,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IChargeInformationOperationsAcceptedEventFactory> chargeInformationOperationsAcceptedEventFactory,
            ChargeInformationOperationsAcceptedEventBuilder chargeInformationOperationsAcceptedEventBuilder,
            ChargeBuilder chargeBuilder,
            ChargePeriodBuilder chargePeriodBuilder,
            ChargeInformationCommandBuilder chargeInformationCommandBuilder,
            ChargeInformationOperationDtoBuilder chargeInformationOperationDtoBuilder,
            TestMarketParticipant sender,
            ChargeInformationOperationsHandler sut)
        {
            // Arrange
            var charge = chargeBuilder.Build();
            var updateOperationDto = chargeInformationOperationDtoBuilder
                .WithStartDateTime(InstantHelper.GetTodayAtMidnightUtc())
                .Build();
            var chargeCommand = chargeInformationCommandBuilder.WithChargeOperation(updateOperationDto).Build();
            var receivedEvent = new ChargeInformationCommandReceivedEvent(InstantHelper.GetTodayAtMidnightUtc(), chargeCommand);
            var validationResult = ValidationResult.CreateSuccess();
            SetupValidators(inputValidator, validationResult);
            var newPeriod = chargePeriodBuilder
                .WithStartDateTime(updateOperationDto.StartDateTime)
                .Build();
            SetupMarketParticipantRepository(marketParticipantRepository, sender);
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);
            chargePeriodFactory.Setup(cpf => cpf.CreateFromChargeOperationDto(updateOperationDto))
                .Returns(newPeriod);
            chargeRepository
                .Setup(r => r.SingleOrNullAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(charge);
            var chargeInformationOperationsAcceptedEvent = chargeInformationOperationsAcceptedEventBuilder.Build();
            chargeInformationOperationsAcceptedEventFactory
                .Setup(c => c.Create(
                    It.IsAny<DocumentDto>(),
                    It.IsAny<IReadOnlyCollection<ChargeInformationOperationDto>>()))
                .Returns(chargeInformationOperationsAcceptedEvent);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            var timeline = charge.Periods.OrderBy(p => p.StartDateTime).ToList();
            var firstPeriod = timeline[0];
            var secondPeriod = timeline[1];

            firstPeriod.StartDateTime.Should().Be(InstantHelper.GetStartDefault());
            firstPeriod.EndDateTime.Should().Be(newPeriod.StartDateTime);
            secondPeriod.StartDateTime.Should().Be(newPeriod.StartDateTime);
            secondPeriod.EndDateTime.Should().Be(newPeriod.EndDateTime);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_IfValidStopEvent_ChargeStopped(
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IInputValidator<ChargeInformationOperationDto>> inputValidator,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IChargeInformationOperationsAcceptedEventFactory> chargeInformationOperationsAcceptedEventFactory,
            ChargeInformationOperationsAcceptedEventBuilder chargeInformationOperationsAcceptedEventBuilder,
            ChargeBuilder chargeBuilder,
            ChargeInformationCommandBuilder chargeInformationCommandBuilder,
            ChargeInformationOperationDtoBuilder chargeInformationOperationDtoBuilder,
            TestMarketParticipant sender,
            ChargeInformationOperationsHandler sut)
        {
            // Arrange
            var stopDate = InstantHelper.GetTodayAtMidnightUtc();
            var stopOperationDto = chargeInformationOperationDtoBuilder.WithStartDateTime(stopDate).WithEndDateTime(stopDate).Build();
            var chargeCommand = chargeInformationCommandBuilder.WithChargeOperation(stopOperationDto).Build();
            var receivedEvent = new ChargeInformationCommandReceivedEvent(InstantHelper.GetTodayAtMidnightUtc(), chargeCommand);
            var charge = chargeBuilder.Build();
            var validationResult = ValidationResult.CreateSuccess();
            SetupValidators(inputValidator, validationResult);
            SetupMarketParticipantRepository(marketParticipantRepository, sender);
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);
            chargeRepository
                .Setup(r => r.SingleOrNullAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(charge);
            var chargeInformationOperationsAcceptedEvent = chargeInformationOperationsAcceptedEventBuilder.Build();
            chargeInformationOperationsAcceptedEventFactory
                .Setup(c => c.Create(
                    It.IsAny<DocumentDto>(),
                    It.IsAny<IReadOnlyCollection<ChargeInformationOperationDto>>()))
                .Returns(chargeInformationOperationsAcceptedEvent);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            charge.Periods.Count.Should().Be(1);
            var actual = charge.Periods.Single();
            actual.EndDateTime.Should().Be(stopDate);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidCancelStop_ThenStopCancelled(
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IInputValidator<ChargeInformationOperationDto>> inputValidator,
            [Frozen] Mock<IChargePeriodFactory> chargePeriodFactory,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IChargeInformationOperationsAcceptedEventFactory> chargeInformationOperationsAcceptedEventFactory,
            ChargeInformationOperationsAcceptedEventBuilder chargeInformationOperationsAcceptedEventBuilder,
            TestMarketParticipant sender,
            ChargeInformationCommandBuilder chargeInformationCommandBuilder,
            ChargeBuilder chargeBuilder,
            ChargeInformationOperationDtoBuilder chargeInformationOperationDtoBuilder,
            ChargeInformationOperationsHandler sut)
        {
            // Arrange
            var validationResult = ValidationResult.CreateSuccess();
            SetupValidators(inputValidator, validationResult);
            var chargeOperationDto = chargeInformationOperationDtoBuilder
                .WithStartDateTime(InstantHelper.GetTomorrowAtMidnightUtc())
                .WithEndDateTime(InstantHelper.GetEndDefault())
                .Build();
            var chargeCommand = chargeInformationCommandBuilder
                .WithChargeOperation(chargeOperationDto)
                .Build();
            var receivedEvent = new ChargeInformationCommandReceivedEvent(InstantHelper.GetTodayAtMidnightUtc(), chargeCommand);
            var charge = chargeBuilder.WithStopDate(InstantHelper.GetTomorrowAtMidnightUtc()).Build();
            SetupMarketParticipantRepository(marketParticipantRepository, sender);
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);
            SetupChargeRepository(chargeRepository, charge);
            SetupChargePeriodFactory(chargePeriodFactory);
            var chargeInformationOperationsAcceptedEvent = chargeInformationOperationsAcceptedEventBuilder.Build();
            chargeInformationOperationsAcceptedEventFactory
                .Setup(c => c.Create(
                    It.IsAny<DocumentDto>(),
                    It.IsAny<IReadOnlyCollection<ChargeInformationOperationDto>>()))
                .Returns(chargeInformationOperationsAcceptedEvent);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            charge.Periods.Count.Should().Be(2);
            var actual = charge.Periods.OrderByDescending(p => p.StartDateTime).First();
            actual.StartDateTime.Should().Be(InstantHelper.GetTomorrowAtMidnightUtc());
            actual.EndDateTime.Should().Be(InstantHelper.GetEndDefault());
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenInputValidationFailsInBundleOperation_RejectEventForAllSubsequentOperations(
            [Frozen] Mock<IInputValidator<ChargeInformationOperationDto>> inputValidator,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IDomainEventPublisher> domainEventPublisher,
            ChargeInformationOperationsAcceptedEventFactory chargeInformationOperationsAcceptedEventFactory,
            ChargeInformationOperationsRejectedEventFactory chargeInformationOperationsRejectedEventFactory,
            ChargeFactory chargeFactory,
            ChargePeriodFactory chargePeriodFactory,
            ChargeInformationOperationsAcceptedEvent acceptedEvent,
            ChargeInformationOperationsRejectedEvent rejectedEvent,
            TestMarketParticipant marketParticipant)
        {
            // Arrange
            var (receivedEvent, invalidOperationId) = CreateReceivedEventWithChargeOperations();
            SetupValidatorsForOperation(inputValidator, invalidOperationId);
            SetupMarketParticipantRepository(marketParticipantRepository, marketParticipant);
            SetupChargeRepository(chargeRepository);
            domainEventPublisher
                .Setup(d => d.Publish(It.IsAny<ChargeInformationOperationsAcceptedEvent>()))
                .Callback<ChargeInformationOperationsAcceptedEvent>(e => acceptedEvent = e);
            domainEventPublisher
                .Setup(d => d.Publish(It.IsAny<ChargeInformationOperationsRejectedEvent>()))
                .Callback<ChargeInformationOperationsRejectedEvent>(e => rejectedEvent = e);

            var sut = new ChargeInformationOperationsHandler(
                inputValidator.Object,
                chargeRepository.Object,
                marketParticipantRepository.Object,
                chargeFactory,
                chargePeriodFactory,
                domainEventPublisher.Object,
                new LoggerFactory(),
                chargeInformationOperationsAcceptedEventFactory,
                chargeInformationOperationsRejectedEventFactory);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            acceptedEvent.Operations.Count().Should().Be(1);
            rejectedEvent.Operations.Count().Should().Be(3);
            rejectedEvent.ValidationErrors
                .First(ve => ve.OperationId == invalidOperationId).ValidationRuleIdentifier
                .Should()
                .Be(ValidationRuleIdentifier.StartDateValidation);
            rejectedEvent.ValidationErrors
                .First(ve => ve.OperationId != invalidOperationId).ValidationRuleIdentifier
                .Should()
                .Be(ValidationRuleIdentifier.SubsequentBundleOperationsFail);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenChargeUpdateFailsInBundleOperation_RejectEventForAllSubsequentOperations(
            [Frozen] Mock<IInputValidator<ChargeInformationOperationDto>> inputValidator,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IDomainEventPublisher> domainEventPublisher,
            ChargeInformationOperationsAcceptedEventFactory chargeInformationOperationsAcceptedEventFactory,
            ChargeInformationOperationsRejectedEventFactory chargeInformationOperationsRejectedEventFactory,
            ChargeFactory chargeFactory,
            ChargePeriodFactory chargePeriodFactory,
            TestMarketParticipant marketParticipant)
        {
            // Arrange
            var (receivedEvent, invalidOperationId) = CreateReceivedEventWithChargeOperations();
            SetupMarketParticipantRepository(marketParticipantRepository, marketParticipant);
            SetupChargeRepository(chargeRepository);
            inputValidator
                .Setup(i => i.Validate(
                    It.IsAny<ChargeInformationOperationDto>(),
                    It.IsAny<DocumentDto>()))
                .Returns(ValidationResult.CreateSuccess());
            ChargeInformationOperationsAcceptedEvent acceptedEvent = null!;
            domainEventPublisher
                .Setup(d => d.Publish(It.IsAny<ChargeInformationOperationsAcceptedEvent>()))
                .Callback<ChargeInformationOperationsAcceptedEvent>(e => acceptedEvent = e);
            ChargeInformationOperationsRejectedEvent rejectedEvent = null!;
            domainEventPublisher
                .Setup(d => d.Publish(It.IsAny<ChargeInformationOperationsRejectedEvent>()))
                .Callback<ChargeInformationOperationsRejectedEvent>(e => rejectedEvent = e);

            var sut = new ChargeInformationOperationsHandler(
                inputValidator.Object,
                chargeRepository.Object,
                marketParticipantRepository.Object,
                chargeFactory,
                chargePeriodFactory,
                domainEventPublisher.Object,
                new LoggerFactory(),
                chargeInformationOperationsAcceptedEventFactory,
                chargeInformationOperationsRejectedEventFactory);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            acceptedEvent.Operations.Count().Should().Be(1);
            rejectedEvent.Operations.Count().Should().Be(3);
            rejectedEvent.ValidationErrors
                .First(ve => ve.OperationId == invalidOperationId).ValidationRuleIdentifier
                .Should()
                .Be(ValidationRuleIdentifier.ChangingTariffTaxValueNotAllowed);
            rejectedEvent.ValidationErrors
                .First(ve => ve.OperationId != invalidOperationId).ValidationRuleIdentifier
                .Should()
                .Be(ValidationRuleIdentifier.SubsequentBundleOperationsFail);
        }

        private static void SetupMarketParticipantRepository(
            Mock<IMarketParticipantRepository> marketParticipantRepository,
            TestMarketParticipant marketParticipant)
        {
            marketParticipantRepository
                .Setup(r => r.GetSystemOperatorOrGridAccessProviderAsync(
                    It.IsAny<string>()))
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

        private static void SetupChargePeriodFactory(Mock<IChargePeriodFactory> chargePeriodFactory, ChargePeriod? period = null)
        {
            if (period is null)
            {
                period = new ChargePeriodBuilder()
                    .WithStartDateTime(InstantHelper.GetTomorrowAtMidnightUtc())
                    .Build();
            }

            chargePeriodFactory
                .Setup(r => r.CreateFromChargeOperationDto(It.IsAny<ChargeInformationOperationDto>()))
                .Returns(period);
        }

        private static void SetupChargeRepository(Mock<IChargeRepository> chargeRepository, Charge? charge = null)
        {
            if (charge == null)
            {
                var period = new ChargePeriodBuilder()
                    .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                    .Build();
                charge = new ChargeBuilder().AddPeriod(period).Build();
            }

            chargeRepository
                .Setup(r => r.SingleOrNullAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(charge);
    }

        private static (ChargeInformationCommandReceivedEvent ReceivedEvent, string InvalidOperationId) CreateReceivedEventWithChargeOperations()
        {
            var validChargeOperationDto = new ChargeInformationOperationDtoBuilder()
                .WithChargeOperationId("Operation1")
                .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                .Build();
            var invalidChargeOperationDto = new ChargeInformationOperationDtoBuilder()
                .WithChargeOperationId("Operation2")
                .WithTaxIndicator(TaxIndicator.NoTax)
                .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                .Build();
            var failedChargeOperationDto = new ChargeInformationOperationDtoBuilder()
                .WithChargeOperationId("Operation3")
                .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                .Build();
            var anotherFailedChargeOperationDto = new ChargeInformationOperationDtoBuilder()
                .WithChargeOperationId("Operation4")
                .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                .Build();
            var chargeCommand = new ChargeInformationCommandBuilder()
                .WithChargeOperations(
                    new List<ChargeInformationOperationDto>
                    {
                        validChargeOperationDto,
                        invalidChargeOperationDto,
                        failedChargeOperationDto,
                        anotherFailedChargeOperationDto,
                    })
                .Build();
            var receivedEvent = new ChargeInformationCommandReceivedEventBuilder()
                .WithCommand(chargeCommand)
                .Build();
            return (ReceivedEvent: receivedEvent, InvalidOperationId: invalidChargeOperationDto.OperationId);
        }

        private static void SetupValidatorsForOperation(
            Mock<IInputValidator<ChargeInformationOperationDto>> inputValidator,
            string invalidOperationId)
        {
            var invalidValidationResult = ValidationResult.CreateFailure(
                new List<IValidationRuleContainer>
                {
                    new OperationValidationRuleContainer(new TestValidationRule(false, ValidationRuleIdentifier.StartDateValidation), invalidOperationId),
                });

            inputValidator
                .Setup(v => v.Validate(
                    It.Is<ChargeInformationOperationDto>(x => x.OperationId == "Operation1"),
                    It.IsAny<DocumentDto>()))
                .Returns(ValidationResult.CreateSuccess());

            inputValidator
                .Setup(v => v.Validate(
                    It.Is<ChargeInformationOperationDto>(x => x.OperationId == "Operation2"),
                    It.IsAny<DocumentDto>()))
                .Returns(invalidValidationResult);
        }

        private static ChargePeriod CreateValidPeriod(int startDaysFromToday = 1)
        {
            return new ChargePeriodBuilder()
                    .WithStartDateTime(InstantHelper.GetTodayPlusDaysAtMidnightUtc(startDaysFromToday))
                    .Build();
        }

        private static ValidationResult GetFailedValidationResult()
        {
            var failedRule = new Mock<IValidationRule>();
            failedRule.Setup(r => r.IsValid).Returns(false);

            return ValidationResult.CreateFailure(
                new List<IValidationRuleContainer> { new DocumentValidationRuleContainer(failedRule.Object) });
        }

        private static void SetupValidators(
            Mock<IInputValidator<ChargeInformationOperationDto>> inputValidator,
            ValidationResult validationResult)
        {
            inputValidator
                .Setup(v => v.Validate(
                    It.IsAny<ChargeInformationOperationDto>(), It.IsAny<DocumentDto>()))
                .Returns(validationResult);
        }
    }
}
