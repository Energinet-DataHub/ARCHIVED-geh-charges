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
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.Charges.Tests.Builders.Testables;
using GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Handlers
{
    [UnitTest]
    public class ChargeInformationEventHandlerTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidationSucceed_StoreAndConfirmCommand(
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IInputValidator<ChargeInformationDto>> inputValidator,
            [Frozen] Mock<IBusinessValidator<ChargeOperation>> businessValidator,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            TestMarketParticipant sender,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IChargeCommandReceiptService> receiptService,
            ChargeBuilder chargeBuilder,
            [Frozen] Mock<IChargeFactory> chargeFactory,
            [Frozen] Mock<IChargePeriodFactory> chargePeriodFactory,
            ChargeInformationDtoBuilder chargeInformationDtoBuilder,
            ChargeCommandBuilder chargeCommandBuilder,
            ChargeInformationEventHandler sut)
        {
            // Arrange
            var chargeInformationDto = chargeInformationDtoBuilder.Build();
            var chargeCommand = chargeCommandBuilder.WithChargeOperation(chargeInformationDto).Build();
            var receivedEvent = new ChargeCommandReceivedEvent(Instant.MinValue, chargeCommand);

            var validationResult = ValidationResult.CreateSuccess();
            SetupValidators(inputValidator, businessValidator, validationResult);

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
                .Setup(s => s.AcceptValidOperationsAsync(It.IsAny<IReadOnlyCollection<ChargeOperation>>(), It.IsAny<DocumentDto>()))
                .Callback<IReadOnlyCollection<ChargeOperation>, DocumentDto>((_, _) => confirmed = true);

            var charge = chargeBuilder.WithPeriods(new List<ChargePeriod> { CreateValidPeriod() }).Build();
            chargeFactory
                .Setup(s => s.CreateFromChargeOperationDtoAsync(It.IsAny<ChargeInformationDto>()))
                .ReturnsAsync(charge);

            chargePeriodFactory
                .Setup(s => s.CreateFromChargeOperationDto(It.IsAny<ChargeInformationDto>()))
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
            [Frozen] Mock<IInputValidator<ChargeInformationDto>> inputValidator,
            [Frozen] Mock<IBusinessValidator<ChargeOperation>> businessValidator,
            [Frozen] Mock<IChargeCommandReceiptService> receiptService,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            TestMarketParticipant sender,
            ChargeBuilder chargeBuilder,
            ChargeInformationDtoBuilder chargeInformationDtoBuilder,
            ChargeCommandBuilder chargeCommandBuilder,
            ChargeInformationEventHandler sut)
        {
            // Arrange
            var chargePriceDto = chargeInformationDtoBuilder.Build();
            var chargeCommand = chargeCommandBuilder.WithChargeOperation(chargePriceDto).Build();
            var receivedEvent = new ChargeCommandReceivedEvent(Instant.MinValue, chargeCommand);

            var validationResult = GetFailedValidationResult();
            SetupValidators(inputValidator, businessValidator, validationResult);
            SetupMarketParticipantRepository(marketParticipantRepository, sender); // TODO: Unnecessary?
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);

            var charge = chargeBuilder.Build();
            SetupChargeRepository(chargeRepository, charge);

            var rejected = false;
            receiptService
                .Setup(s => s.RejectInvalidOperationsAsync(
                    It.IsAny<IReadOnlyCollection<ChargeOperation>>(),
                    It.IsAny<DocumentDto>(),
                    It.IsAny<IList<IValidationRuleContainer>>()))
                .Callback<IReadOnlyCollection<ChargeOperation>, DocumentDto, IList<IValidationRuleContainer>>((_, _, _) => rejected = true);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            rejected.Should().Be(true);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_IfEventIsNull_ThrowsArgumentNullException(
            ChargeInformationEventHandler sut)
        {
            // Arrange
            ChargeCommandReceivedEvent? receivedEvent = null;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.HandleAsync(receivedEvent!));
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_IfValidUpdateEvent_ChargeUpdated(
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IInputValidator<ChargeInformationDto>> inputValidator,
            [Frozen] Mock<IBusinessValidator<ChargeOperation>> businessValidator,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IChargePeriodFactory> chargePeriodFactory,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            TestMarketParticipant sender,
            ChargePeriodBuilder chargePeriodBuilder,
            ChargeInformationDtoBuilder chargeInformationDtoBuilder,
            ChargeCommandBuilder chargeCommandBuilder,
            ChargeInformationEventHandler sut)
        {
            // Arrange
            var chargePriceDto = chargeInformationDtoBuilder.Build();
            var chargeCommand = chargeCommandBuilder.WithChargeOperation(chargePriceDto).Build();
            var receivedEvent = new ChargeCommandReceivedEvent(Instant.MinValue, chargeCommand);

            var validationResult = ValidationResult.CreateSuccess();
            SetupValidators(inputValidator, businessValidator, validationResult);
            SetupMarketParticipantRepository(marketParticipantRepository, sender); //TODO: Unnecessary?
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);

            var periods = CreateValidPeriods(3);
            var charge = CreateValidCharge(periods);
            SetupChargeRepository(chargeRepository, charge);

            var newPeriod = chargePeriodBuilder
                .WithStartDateTime(InstantHelper.GetTomorrowAtMidnightUtc())
                .WithEndDateTime(InstantHelper.GetEndDefault())
                .Build();
            SetupChargePeriodFactory(chargePeriodFactory, newPeriod);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            charge.Periods.Count.Should().Be(2);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_IfValidStopEvent_ChargeStopped(
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IInputValidator<ChargeInformationDto>> inputValidator,
            [Frozen] Mock<IBusinessValidator<ChargeOperation>> businessValidator,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IChargePeriodFactory> chargePeriodFactory,
            [Frozen] Instant stopDate,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            TestMarketParticipant sender,
            ChargePeriodBuilder chargePeriodBuilder,
            ChargeInformationDtoBuilder chargeInformationDtoBuilder,
            ChargeCommandBuilder chargeCommandBuilder,
            ChargeInformationEventHandler sut)
        {
            // Arrange
            var chargeInformationDto = chargeInformationDtoBuilder
                .WithStartDateTime(stopDate)
                .WithEndDateTime(stopDate)
                .Build();
            var chargeCommand = chargeCommandBuilder.WithChargeOperation(chargeInformationDto).Build();
            var receivedEvent = new ChargeCommandReceivedEvent(Instant.MinValue, chargeCommand);

            var validationResult = ValidationResult.CreateSuccess();
            SetupValidators(inputValidator, businessValidator, validationResult);
            SetupMarketParticipantRepository(marketParticipantRepository, sender);
            SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);

            var periods = CreateValidPeriodsFromOffset(stopDate);
            var charge = CreateValidCharge(periods);
            SetupChargeRepository(chargeRepository, charge);

            var newPeriod = chargePeriodBuilder.WithStartDateTime(stopDate).WithEndDateTime(stopDate).Build();
            SetupChargePeriodFactory(chargePeriodFactory, newPeriod);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            charge.Periods.Count.Should().Be(2);
            var actual = charge.Periods.OrderByDescending(p => p.StartDateTime).First();
            actual.EndDateTime.Should().Be(stopDate);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidCancelStop_ThenStopCancelled(
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IInputValidator<ChargeInformationDto>> inputValidator,
            [Frozen] Mock<IBusinessValidator<ChargeOperation>> businessValidator,
            [Frozen] Mock<IChargePeriodFactory> chargePeriodFactory,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            TestMarketParticipant sender,
            ChargePeriodBuilder chargePeriodBuilder,
            ChargeCommandBuilder chargeCommandBuilder,
            ChargeBuilder chargeBuilder,
            ChargeInformationDtoBuilder chargeInformationDtoBuilder,
            ChargeInformationEventHandler sut)
        {
            // Arrange
            var validationResult = ValidationResult.CreateSuccess();
            SetupValidators(inputValidator, businessValidator, validationResult);
            var chargeOperationDto = chargeInformationDtoBuilder
                .WithStartDateTime(InstantHelper.GetTomorrowAtMidnightUtc())
                .WithEndDateTime(InstantHelper.GetEndDefault())
                .Build();
            var chargeCommand = chargeCommandBuilder
                .WithChargeOperation(chargeOperationDto)
                .Build();
            var receivedEvent = new ChargeCommandReceivedEvent(InstantHelper.GetTodayAtMidnightUtc(), chargeCommand);
            var periods = new List<ChargePeriod>
            {
                chargePeriodBuilder.WithEndDateTime(InstantHelper.GetTomorrowAtMidnightUtc()).Build(),
            };
            var charge = chargeBuilder.WithPeriods(periods).Build();
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
        public async Task HandleAsync_WhenValidationFailsInBundleOperation_RejectEventForAllSubsequentOperations(
            TestMarketParticipant sender,
            [Frozen] Mock<IChargeIdentifierFactory> chargeIdentifierFactory,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IChargePeriodFactory> chargePeriodFactory,
            [Frozen] Mock<IDocumentValidator<ChargeCommand>> documentValidator,
            [Frozen] Mock<IInputValidator<ChargeInformationDto>> inputValidator,
            [Frozen] Mock<IBusinessValidator<ChargeOperation>> businessValidator,
            [Frozen] Mock<IChargeCommandReceiptService> receiptService,
            ChargeInformationEventHandler sut)
         {
             // Arrange
             var receivedEvent = CreateReceivedEventWithChargeOperations();
             SetupMarketParticipantRepository(marketParticipantRepository, sender); //TODO: Unnecessary?
             SetupChargeIdentifierFactoryMock(chargeIdentifierFactory);
             SetupChargeRepository(chargeRepository);
             SetupChargePeriodFactory(chargePeriodFactory);

             var invalidValidationResult = ValidationResult.CreateFailure(
                 new List<IValidationRuleContainer>
                 {
                     new DocumentValidationRuleContainer(
                         new TestValidationRule(false, ValidationRuleIdentifier.StartDateValidation)),
                 });

             SetupValidatorsForOperation(documentValidator, inputValidator, businessValidator, invalidValidationResult);

             var accepted = 0;
             receiptService
                 .Setup(s => s.AcceptValidOperationsAsync(
                     It.IsAny<IReadOnlyCollection<ChargeOperation>>(),
                     It.IsAny<DocumentDto>()))
                 .Callback<IReadOnlyCollection<ChargeOperation>, DocumentDto>((_, _) => accepted++);
             var rejectedRules = new List<IValidationRuleContainer>();
             receiptService
                 .Setup(s => s.RejectInvalidOperationsAsync(
                     It.IsAny<IReadOnlyCollection<ChargeOperation>>(),
                     It.IsAny<DocumentDto>(),
                     It.IsAny<IList<IValidationRuleContainer>>()))
                 .Callback<IReadOnlyCollection<ChargeOperation>, DocumentDto, IList<IValidationRuleContainer>>((_, _, s) => rejectedRules.AddRange(s));

             // Act
             await sut.HandleAsync(receivedEvent);

             // Assert
             accepted.Should().Be(1);
             rejectedRules.Count.Should().Be(3);

             var invalid = rejectedRules.Where(vr =>
                 vr.ValidationRule.ValidationRuleIdentifier == ValidationRuleIdentifier.StartDateValidation);
             invalid.Count().Should().Be(1);

             var subsequent = rejectedRules.Where(vr =>
                 vr.ValidationRule.ValidationRuleIdentifier == ValidationRuleIdentifier.SubsequentBundleOperationsFail).ToList();
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
         }

        private static void SetupMarketParticipantRepository(
            Mock<IMarketParticipantRepository> marketParticipantRepository,
            TestMarketParticipant marketParticipant)
        {
            marketParticipantRepository
                .Setup(r => r.SingleAsync(It.IsAny<string>()))
                .ReturnsAsync(marketParticipant);
        }

        private static void SetupChargeIdentifierFactoryMock(Mock<IChargeIdentifierFactory> chargeIdentifierFactory)
        {
            chargeIdentifierFactory
                .Setup(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<ChargeType>(), It.IsAny<string>()))
                .ReturnsAsync(It.IsAny<ChargeIdentifier>());
        }

        private static void SetupChargePeriodFactory(Mock<IChargePeriodFactory> chargePeriodFactory, ChargePeriod? period = null)
        {
            if (period == null)
            {
                period = new ChargePeriodBuilder()
                    .WithStartDateTime(InstantHelper.GetTomorrowAtMidnightUtc())
                    .Build();
            }

            chargePeriodFactory
                .Setup(r => r.CreateFromChargeOperationDto(It.IsAny<ChargeInformationDto>()))
                .Returns(period);
        }

        private static void SetupChargeRepository(Mock<IChargeRepository> chargeRepository, Charge? charge = null)
        {
            if (charge == null)
            {
                charge = new ChargeBuilder()
                    .WithPeriods(
                        new List<ChargePeriod>
                        {
                            new ChargePeriodBuilder()
                                .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                                .Build(),
                        })
                    .Build();
            }

            chargeRepository
                .Setup(r => r.SingleOrNullAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(charge);
    }

        private static ChargeCommandReceivedEvent CreateReceivedEventWithChargeOperations()
        {
            var validChargeOperationDto = new ChargeInformationDtoBuilder()
                .WithChargeOperationId("Operation1")
                .WithDescription("valid")
                .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                .WithEndDateTime(InstantHelper.GetEndDefault())
                .Build();
            var invalidChargeOperationDto = new ChargeInformationDtoBuilder()
                .WithChargeOperationId("Operation2")
                .WithDescription("invalid")
                .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                .WithEndDateTime(InstantHelper.GetEndDefault())
                .Build();
            var failedChargeOperationDto = new ChargeInformationDtoBuilder()
                .WithChargeOperationId("Operation3")
                .WithDescription("failed")
                .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                .WithEndDateTime(InstantHelper.GetEndDefault())
                .Build();
            var anotherFailedChargeOperationDto = new ChargeInformationDtoBuilder()
                .WithChargeOperationId("Operation4")
                .WithDescription("another failed")
                .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                .WithEndDateTime(InstantHelper.GetEndDefault())
                .Build();
            var chargeCommand = new ChargeCommandBuilder()
                .WithChargeOperations(
                    new List<ChargeOperation>
                    {
                        validChargeOperationDto,
                        invalidChargeOperationDto,
                        failedChargeOperationDto,
                        anotherFailedChargeOperationDto,
                    })
                .Build();
            var receivedEvent = new ChargeCommandReceivedEvent(
                InstantHelper.GetTodayAtMidnightUtc(),
                chargeCommand);
            return receivedEvent;
        }

        private static void SetupValidatorsForOperation(
            Mock<IDocumentValidator<ChargeCommand>> documentValidator,
            Mock<IInputValidator<ChargeInformationDto>> inputValidator,
            Mock<IBusinessValidator<ChargeOperation>> businessValidator,
            ValidationResult invalidValidationResult)
        {
            inputValidator
                .Setup(v => v.Validate(It.IsAny<ChargeInformationDto>()))
                .Returns(ValidationResult.CreateSuccess());

            documentValidator
                .Setup(v => v.ValidateAsync(It.IsAny<ChargeCommand>()))
                .ReturnsAsync(ValidationResult.CreateSuccess());

            businessValidator
                .Setup(v => v.ValidateAsync(It.Is<ChargeOperation>(x => x.ChargeId == "valid")))
                .Returns(Task.FromResult(ValidationResult.CreateSuccess()));

            businessValidator
                .Setup(v => v.ValidateAsync(It.Is<ChargeOperation>(x => x.ChargeId == "invalid")))
                .Returns(Task.FromResult(invalidValidationResult));
        }

        private static IEnumerable<ChargePeriod> CreateValidPeriods(int numberOfPeriods = 1)
        {
            for (var i = 0; i < numberOfPeriods; i++)
            {
                var endDate = InstantHelper.GetTodayPlusDaysAtMidnightUtc(i + 1);
                if (i == numberOfPeriods)
                {
                    endDate = InstantHelper.GetEndDefault();
                }

                yield return new ChargePeriodBuilder()
                    .WithStartDateTime(InstantHelper.GetTodayPlusDaysAtMidnightUtc(i))
                    .WithEndDateTime(endDate)
                    .Build();
            }
        }

        private static ChargePeriod CreateValidPeriod(int startDaysFromToday = 1)
        {
            return new ChargePeriodBuilder()
                    .WithStartDateTime(InstantHelper.GetTodayPlusDaysAtMidnightUtc(startDaysFromToday))
                    .WithEndDateTime(InstantHelper.GetEndDefault())
                    .Build();
        }

        private static IEnumerable<ChargePeriod> CreateValidPeriodsFromOffset(Instant offsetDate)
        {
            return new List<ChargePeriod>
            {
                new ChargePeriodBuilder()
                    .WithStartDateTime(offsetDate.Minus(Duration.FromDays(5)))
                    .WithEndDateTime(offsetDate.Minus(Duration.FromDays(1)))
                    .Build(),
                new ChargePeriodBuilder()
                    .WithStartDateTime(offsetDate.Minus(Duration.FromDays(1)))
                    .WithEndDateTime(offsetDate.Plus(Duration.FromDays(5)))
                    .Build(),
                new ChargePeriodBuilder()
                    .WithStartDateTime(offsetDate.Plus(Duration.FromDays(5)))
                    .WithEndDateTime(InstantHelper.GetEndDefault())
                    .Build(),
            };
        }

        private static Charge CreateValidCharge(IEnumerable<ChargePeriod> periods)
        {
            return new ChargeBuilder()
                .WithPeriods(periods)
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
            Mock<IInputValidator<ChargeInformationDto>> inputValidator,
            Mock<IBusinessValidator<ChargeOperation>> businessValidator,
            ValidationResult validationResult)
        {
            inputValidator.Setup(v => v.Validate(It.IsAny<ChargeInformationDto>()))
                .Returns(validationResult);
            businessValidator.Setup(v => v.ValidateAsync(It.IsAny<ChargeOperation>()))
                .Returns(Task.FromResult(validationResult));
        }
    }
}
