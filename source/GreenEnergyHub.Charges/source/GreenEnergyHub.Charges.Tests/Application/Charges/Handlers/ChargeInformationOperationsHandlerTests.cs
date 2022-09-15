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
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Charges.Acknowledgement;
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands.Validation.BusinessValidation.ValidationRules;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.Charges.Tests.Builders.Testables;
using GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Handlers
{
    [UnitTest]
    public class ChargeInformationOperationsHandlerTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidationSucceed_StoreAndConfirmCommand(
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IInputValidator<ChargeInformationOperationDto>> inputValidator,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IChargeCommandReceiptService> receiptService,
            [Frozen] Mock<IChargeFactory> chargeFactory,
            [Frozen] Mock<IChargePeriodFactory> chargePeriodFactory,
            TestMarketParticipant sender,
            ChargeBuilder chargeBuilder,
            ChargeInformationCommandBuilder chargeCommandBuilder,
            ChargeInformationOperationDtoBuilder chargeInformationOperationDtoBuilder,
            ChargeInformationOperationsHandler sut)
        {
            // Arrange
            var createOperationDto = chargeInformationOperationDtoBuilder
                .WithStartDateTime(InstantHelper.GetTodayAtMidnightUtc())
                .WithEndDateTime(InstantHelper.GetEndDefault())
                .Build();
            var chargeCommand = chargeCommandBuilder.WithChargeOperation(createOperationDto).Build();
            var receivedEvent = new ChargeInformationCommandReceivedEvent(InstantHelper.GetTodayAtMidnightUtc(), chargeCommand);

            var validationResult = ValidationResult.CreateSuccess();
            SetupValidators(inputValidator, validationResult);

            var stored = false;
            SetupMarketParticipantRepository(marketParticipantRepository, sender);
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);
            chargeRepository
                .Setup(r => r.AddAsync(It.IsAny<Charge>()))
                .Callback<Charge>(_ => stored = true);
            chargeRepository
                .Setup(r => r.SingleOrNullAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(null as Charge);

            var confirmed = false;
            receiptService
                .Setup(s => s.AcceptValidOperationsAsync(It.IsAny<IReadOnlyCollection<ChargeInformationOperationDto>>(), It.IsAny<DocumentDto>()))
                .Callback<IReadOnlyCollection<ChargeInformationOperationDto>, DocumentDto>((_, _) => confirmed = true);

            var charge = chargeBuilder.AddPeriod(CreateValidPeriod()).Build();
            chargeFactory
                .Setup(s => s.CreateFromChargeOperationDtoAsync(It.IsAny<ChargeInformationOperationDto>()))
                .ReturnsAsync(charge);
            chargePeriodFactory
                 .Setup(s => s.CreateFromChargeOperationDto(It.IsAny<ChargeInformationOperationDto>()))
                 .Returns(CreateValidPeriod(30));

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            stored.Should().Be(true);
            confirmed.Should().Be(true);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidationFails_RejectsEvent(
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IInputValidator<ChargeInformationOperationDto>> inputValidator,
            [Frozen] Mock<IChargeCommandReceiptService> receiptService,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            TestMarketParticipant sender,
            ChargeBuilder chargeBuilder,
            ChargeInformationCommandReceivedEvent receivedEvent,
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

            var rejected = false;
            receiptService
                .Setup(s => s.RejectInvalidOperationsAsync(
                    It.IsAny<IReadOnlyCollection<ChargeInformationOperationDto>>(),
                    It.IsAny<DocumentDto>(),
                    It.IsAny<IList<IValidationRuleContainer>>()))
                .Callback<IReadOnlyCollection<ChargeInformationOperationDto>, DocumentDto, IList<IValidationRuleContainer>>((_, _, _) => rejected = true);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            rejected.Should().Be(true);
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
            TestMarketParticipant sender,
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IChargePeriodFactory> chargePeriodFactory,
            [Frozen] Mock<IDocumentValidator> documentValidator,
            [Frozen] Mock<IInputValidator<ChargeInformationOperationDto>> inputValidator,
            [Frozen] Mock<IChargeCommandReceiptService> receiptService,
            ChargeInformationOperationsHandler sut)
        {
         // Arrange
         var (receivedEvent, invalidOperationId) = CreateReceivedEventWithChargeOperations();
         SetupMarketParticipantRepository(marketParticipantRepository, sender);
         SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);
         SetupChargeRepository(chargeRepository);
         SetupChargePeriodFactory(chargePeriodFactory);
         var invalidValidationResult = ValidationResult.CreateFailure(
             new List<IValidationRuleContainer>
             {
                 new OperationValidationRuleContainer(new TestValidationRule(false, ValidationRuleIdentifier.StartDateValidation), invalidOperationId),
             });

         SetupValidatorsForOperation(documentValidator, inputValidator, invalidValidationResult);

         var accepted = 0;
         receiptService
             .Setup(s => s.AcceptValidOperationsAsync(
                 It.IsAny<IReadOnlyCollection<ChargeInformationOperationDto>>(),
                 It.IsAny<DocumentDto>()))
             .Callback<IReadOnlyCollection<ChargeInformationOperationDto>, DocumentDto>((_, _) => accepted++);
         var rejectedRules = new List<IValidationRuleContainer>();
         receiptService
             .Setup(s => s.RejectInvalidOperationsAsync(
                 It.IsAny<IReadOnlyCollection<ChargeInformationOperationDto>>(),
                 It.IsAny<DocumentDto>(),
                 It.IsAny<IList<IValidationRuleContainer>>()))
             .Callback<IReadOnlyCollection<ChargeInformationOperationDto>, DocumentDto, IList<IValidationRuleContainer>>((_, _, s) => rejectedRules.AddRange(s));

         // Act
         await sut.HandleAsync(receivedEvent);

         // Assert
         accepted.Should().Be(1);
         rejectedRules.Count.Should().Be(3);

         var invalid = (OperationValidationRuleContainer)rejectedRules.Single(vr =>
             vr.ValidationRule.ValidationRuleIdentifier == ValidationRuleIdentifier.StartDateValidation);

         var subsequent = rejectedRules
             .Where(vr =>
                 vr.ValidationRule.ValidationRuleIdentifier == ValidationRuleIdentifier.SubsequentBundleOperationsFail)
             .ToList();
         subsequent.Should().HaveCount(2);

         var firstOperationValidationRuleContainer = (IOperationValidationRuleContainer)subsequent.First();
         firstOperationValidationRuleContainer.OperationId.Should().Be("Operation3");
         var firstOperationTriggeredBy =
             ((IValidationRuleWithExtendedData)firstOperationValidationRuleContainer.ValidationRule).TriggeredBy;
         firstOperationTriggeredBy.Should().Be("Operation2");

         var secondOperationValidationRuleContainer = (IOperationValidationRuleContainer)subsequent.Last();
         secondOperationValidationRuleContainer.OperationId.Should().Be("Operation4");
         var secondOperationTriggeredBy =
             ((IValidationRuleWithExtendedData)secondOperationValidationRuleContainer.ValidationRule).TriggeredBy;
         secondOperationTriggeredBy.Should().Be("Operation2");
         invalid.OperationId.Should().Be(invalidOperationId);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenChargeUpdateFailsInBundleOperation_RejectEventForAllSubsequentOperations(
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IChargePeriodFactory> chargePeriodFactory,
            [Frozen] Mock<IDocumentValidator> documentValidator,
            [Frozen] Mock<IInputValidator<ChargeInformationOperationDto>> inputValidator,
            [Frozen] Mock<IChargeCommandReceiptService> receiptService,
            TestMarketParticipant sender,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            ChargeBuilder chargeBuilder,
            ChargeInformationOperationsHandler sut)
        {
            // Arrange
            var charge = chargeBuilder.WithTaxIndicator(TaxIndicator.Tax).Build();
            var (receivedEvent, invalidOperationId) = CreateReceivedEventWithChargeOperations();
            SetupChargeRepository(chargeRepository, charge);
            SetupChargePeriodFactory(chargePeriodFactory, receivedEvent.Command.Operations.ToList());
            SetupMarketParticipantRepository(marketParticipantRepository, sender);
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);
            inputValidator
                .Setup(v => v.Validate(
                    It.IsAny<ChargeInformationOperationDto>(),
                    It.IsAny<DocumentDto>()))
                .Returns(ValidationResult.CreateSuccess());
            documentValidator
                .Setup(v => v.ValidateAsync(It.IsAny<ChargeInformationCommand>()))
                .ReturnsAsync(ValidationResult.CreateSuccess());

            var accepted = 0;
            receiptService
                .Setup(s => s.AcceptValidOperationsAsync(
                    It.IsAny<IReadOnlyCollection<ChargeInformationOperationDto>>(),
                    It.IsAny<DocumentDto>()))
                .Callback<IReadOnlyCollection<ChargeInformationOperationDto>, DocumentDto>((_, _) => accepted++);
            var rejectedRules = new List<IValidationRuleContainer>();
            receiptService
                .Setup(s => s.RejectInvalidOperationsAsync(
                    It.IsAny<IReadOnlyCollection<ChargeInformationOperationDto>>(),
                    It.IsAny<DocumentDto>(),
                    It.IsAny<IList<IValidationRuleContainer>>()))
                .Callback<IReadOnlyCollection<ChargeInformationOperationDto>, DocumentDto, IList<IValidationRuleContainer>>(
                    (_, _, s) => rejectedRules.AddRange(s));

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            accepted.Should().Be(1);
            rejectedRules.Count.Should().Be(3);
            var invalid = (OperationValidationRuleContainer)rejectedRules.Single(vr =>
                vr.ValidationRule.ValidationRuleIdentifier ==
                ValidationRuleIdentifier.ChangingTariffTaxValueNotAllowed);
            invalid.OperationId.Should().Be(invalidOperationId);
            var subsequent = rejectedRules
                .Where(vr =>
                    vr.ValidationRule.ValidationRuleIdentifier ==
                    ValidationRuleIdentifier.SubsequentBundleOperationsFail)
                .ToList();

            rejectedRules.Count.Should().Be(3);
            subsequent.Count.Should().Be(2);
            subsequent.Should().OnlyContain(r =>
                ((PreviousOperationsMustBeValidRule)r.ValidationRule).TriggeredBy == invalidOperationId);
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

        private static void SetupChargePeriodFactory(Mock<IChargePeriodFactory> chargePeriodFactory, List<ChargeInformationOperationDto> operations)
        {
            foreach (var operation in operations)
            {
                var period = new ChargePeriodBuilder()
                    .WithName(operation.ChargeName)
                    .WithStartDateTime(operation.StartDateTime)
                    .Build();
                chargePeriodFactory
                    .Setup(r => r.CreateFromChargeOperationDto(operation))
                    .Returns(period);
            }
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
                .WithDescription("valid")
                .WithTaxIndicator(TaxIndicator.Tax)
                .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                .Build();
            var invalidChargeOperationDto = new ChargeInformationOperationDtoBuilder()
                .WithChargeOperationId("Operation2")
                .WithDescription("invalid")
                .WithTaxIndicator(TaxIndicator.NoTax)
                .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                .Build();
            var failedChargeOperationDto = new ChargeInformationOperationDtoBuilder()
                .WithChargeOperationId("Operation3")
                .WithDescription("failed")
                .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                .Build();
            var anotherFailedChargeOperationDto = new ChargeInformationOperationDtoBuilder()
                .WithChargeOperationId("Operation4")
                .WithDescription("another failed")
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
            var receivedEvent = new ChargeInformationCommandReceivedEvent(
                InstantHelper.GetTodayAtMidnightUtc(),
                chargeCommand);
            return (ReceivedEvent: receivedEvent, InvalidOperationId: invalidChargeOperationDto.OperationId);
        }

        private static void SetupValidatorsForOperation(
            Mock<IDocumentValidator> documentValidator,
            Mock<IInputValidator<ChargeInformationOperationDto>> inputValidator,
            ValidationResult invalidValidationResult)
        {
            documentValidator.Setup(v =>
                v.ValidateAsync(It.IsAny<ChargeInformationCommand>())).ReturnsAsync(ValidationResult.CreateSuccess());

            inputValidator
                .Setup(v => v.Validate(
                    It.Is<ChargeInformationOperationDto>(x => x.ChargeDescription == "valid"),
                    It.IsAny<DocumentDto>()))
                .Returns(ValidationResult.CreateSuccess());

            inputValidator
                .Setup(v => v.Validate(
                    It.Is<ChargeInformationOperationDto>(x => x.ChargeDescription == "invalid"),
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
