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
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Handlers
{
    [UnitTest]
    public class ChargeCommandReceivedEventHandlerTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidationSucceed_StoreAndConfirmCommand(
            [Frozen] Mock<IDocumentValidator<ChargeCommand>> documentValidator,
            [Frozen] Mock<IInputValidator<ChargeCommand>> inputValidator,
            [Frozen] Mock<IBusinessValidator<ChargeCommand>> businessValidator,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IChargeCommandReceiptService> receiptService,
            ChargeBuilder chargeBuilder,
            [Frozen] Mock<IChargeFactory> chargeFactory,
            [Frozen] Mock<IChargePeriodFactory> chargePeriodFactory,
            ChargeCommandReceivedEvent receivedEvent,
            ChargeCommandReceivedEventHandler sut)
        {
            // Arrange
            var validationResult = ValidationResult.CreateSuccess();
            SetupValidators(documentValidator, inputValidator, businessValidator, validationResult);

            var stored = false;
            chargeRepository
                .Setup(r => r.AddAsync(It.IsAny<Charge>()))
                .Callback<Charge>(_ => stored = true);
            chargeRepository
                .Setup(r => r.GetOrNullAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(null as Charge);

            var confirmed = false;
            receiptService
                .Setup(s => s.AcceptAsync(It.IsAny<ChargeCommand>()))
                .Callback<ChargeCommand>(_ => confirmed = true);

            var charge = chargeBuilder.WithPeriods(new List<ChargePeriod> { CreateValidPeriod() }).Build();
            chargeFactory
                .Setup(s => s.CreateFromChargeOperationDtoAsync(
                    It.IsAny<MarketParticipantRole>(),
                    It.IsAny<ChargeOperationDto>()))
                .ReturnsAsync(charge);

            chargePeriodFactory
                .Setup(s => s.CreateFromChargeOperationDto(It.IsAny<ChargeOperationDto>()))
                .Returns(CreateValidPeriod(30));

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            Assert.True(stored);
            Assert.True(confirmed);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidationFails_RejectsEvent(
            [Frozen] Mock<IDocumentValidator<ChargeCommand>> documentValidator,
            [Frozen] Mock<IInputValidator<ChargeCommand>> inputValidator,
            [Frozen] Mock<IBusinessValidator<ChargeCommand>> businessValidator,
            [Frozen] Mock<IChargeCommandReceiptService> receiptService,
            ChargeCommandReceivedEvent receivedEvent,
            ChargeCommandReceivedEventHandler sut)
        {
            // Arrange
            var validationResult = GetFailedValidationResult();
            SetupValidators(documentValidator, inputValidator, businessValidator, validationResult);

            var rejected = false;
            receiptService.Setup(s => s.RejectAsync(It.IsAny<ChargeCommand>(), validationResult))
                .Callback<ChargeCommand, ValidationResult>((_, _) => rejected = true);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            Assert.True(rejected);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_IfEventIsNull_ThrowsArgumentNullException(
            ChargeCommandReceivedEventHandler sut)
        {
            // Arrange
            ChargeCommandReceivedEvent? receivedEvent = null;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.HandleAsync(receivedEvent!));
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_IfValidUpdateEvent_ChargeUpdated(
            [Frozen] Mock<IDocumentValidator<ChargeCommand>> documentValidator,
            [Frozen] Mock<IInputValidator<ChargeCommand>> inputValidator,
            [Frozen] Mock<IBusinessValidator<ChargeCommand>> businessValidator,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IChargePeriodFactory> chargePeriodFactory,
            ChargeCommandReceivedEvent receivedEvent,
            ChargeCommandReceivedEventHandler sut)
        {
            // Arrange
            var validationResult = ValidationResult.CreateSuccess();
            SetupValidators(documentValidator, inputValidator, businessValidator, validationResult);
            var periods = CreateValidPeriods(3);
            var charge = CreateValidCharge(periods);
            var newPeriod = new ChargePeriodBuilder()
                .WithStartDateTime(InstantHelper.GetTomorrowAtMidnightUtc())
                .WithEndDateTime(InstantHelper.GetEndDefault())
                .Build();

            chargeRepository
                .Setup(r => r.GetOrNullAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(charge);
            chargePeriodFactory
                .Setup(r => r.CreateFromChargeOperationDto(It.IsAny<ChargeOperationDto>()))
                .Returns(newPeriod);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            charge.Periods.Count.Should().Be(2);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_IfValidStopEvent_ChargeStopped(
            [Frozen] Mock<IDocumentValidator<ChargeCommand>> documentValidator,
            [Frozen] Mock<IInputValidator<ChargeCommand>> inputValidator,
            [Frozen] Mock<IBusinessValidator<ChargeCommand>> businessValidator,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IChargePeriodFactory> chargePeriodFactory,
            [Frozen] Instant stopDate,
            ChargeCommandReceivedEvent receivedEvent,
            ChargeCommandReceivedEventHandler sut)
        {
            // Arrange
            var validationResult = ValidationResult.CreateSuccess();
            SetupValidators(documentValidator, inputValidator, businessValidator, validationResult);
            var periods = CreateValidPeriodsFromOffset(stopDate);
            var charge = CreateValidCharge(periods);
            var newPeriod = new ChargePeriodBuilder().WithStartDateTime(stopDate).WithEndDateTime(stopDate).Build();

            chargeRepository
                .Setup(r => r.GetOrNullAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(charge);
            chargePeriodFactory
                .Setup(r => r.CreateFromChargeOperationDto(It.IsAny<ChargeOperationDto>()))
                .Returns(newPeriod);

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
            [Frozen] Mock<IDocumentValidator<ChargeCommand>> documentValidator,
            [Frozen] Mock<IInputValidator<ChargeCommand>> inputValidator,
            [Frozen] Mock<IBusinessValidator<ChargeCommand>> businessValidator,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IChargePeriodFactory> chargePeriodFactory,
            ChargeCommandReceivedEventHandler sut)
        {
            // Arrange
            var validationResult = ValidationResult.CreateSuccess();
            SetupValidators(documentValidator, inputValidator, businessValidator, validationResult);
            var chargeOperationDto = new ChargeOperationDtoBuilder()
                .WithStartDateTime(InstantHelper.GetTomorrowAtMidnightUtc())
                .WithEndDateTime(InstantHelper.GetEndDefault())
                .Build();
            var chargeCommand = new ChargeCommandBuilder()
                .WithChargeOperation(chargeOperationDto)
                .Build();
            var receivedEvent = new ChargeCommandReceivedEvent(InstantHelper.GetTodayAtMidnightUtc(), chargeCommand);
            var periods = new List<ChargePeriod>
            {
                new ChargePeriodBuilder().WithEndDateTime(InstantHelper.GetTomorrowAtMidnightUtc()).Build(),
            };
            var charge = new ChargeBuilder().WithPeriods(periods).Build();
            var newPeriod = new ChargePeriodBuilder()
                .WithStartDateTime(InstantHelper.GetTomorrowAtMidnightUtc())
                .Build();
            chargeRepository
                .Setup(r => r.GetOrNullAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(charge);
            chargePeriodFactory
                .Setup(r => r.CreateFromChargeOperationDto(It.IsAny<ChargeOperationDto>()))
                .Returns(newPeriod);

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
             [Frozen] Mock<IChargeRepository> chargeRepository,
             [Frozen] Mock<IChargePeriodFactory> chargePeriodFactory,
             [Frozen] Mock<IDocumentValidator<ChargeCommand>> documentValidator,
             [Frozen] Mock<IInputValidator<ChargeCommand>> inputValidator,
             [Frozen] Mock<IBusinessValidator<ChargeCommand>> businessValidator,
             [Frozen] Mock<IChargeCommandReceiptService> receiptService,
             ChargeCommandReceivedEventHandler sut)
         {
             // Arrange
             var receivedEvent = CreateReceivedEventWithChargeOperations();
             SetupChargeRepository(chargeRepository);
             SetupChargePeriodFactory(chargePeriodFactory);

             var invalidValidationResult = ValidationResult.CreateFailure(new List<IValidationRule>
                 { new TestValidationRule(false, ValidationRuleIdentifier.StartDateValidation) });
             SetupValidatorsForOperation(documentValidator, inputValidator, businessValidator, invalidValidationResult);

             var accepted = 0;
             receiptService.Setup(s => s.AcceptAsync(It.IsAny<ChargeCommand>()))
                 .Callback<ChargeCommand>((_) => accepted++);

             var rejected = 0;
             receiptService.Setup(s =>
                      s.RejectAsync(
                              It.IsAny<ChargeCommand>(), invalidValidationResult))
                  .Callback<ChargeCommand, ValidationResult>((_, _) => rejected++);

             var autoRejected = 0;
             receiptService.Setup(s => s.RejectAsync(
                     It.IsAny<ChargeCommand>(), It.Is<ValidationResult>(x =>
                         x.InvalidRules.Any(z =>
                             z.ValidationRuleIdentifier == ValidationRuleIdentifier.SubsequentBundleOperationsFail))))
                 .Callback<ChargeCommand, ValidationResult>((_, _) => autoRejected++);

             // Act
             await sut.HandleAsync(receivedEvent);

             // Assert
             Assert.Equal(1, accepted);
             Assert.Equal(1, rejected);
             Assert.Equal(2, autoRejected);
        }

        private static void SetupChargePeriodFactory(Mock<IChargePeriodFactory> chargePeriodFactory)
        {
            var newPeriod = new ChargePeriodBuilder()
                .WithStartDateTime(InstantHelper.GetTomorrowAtMidnightUtc())
                .Build();
            chargePeriodFactory
                .Setup(r => r.CreateFromChargeOperationDto(It.IsAny<ChargeOperationDto>()))
                .Returns(newPeriod);
        }

        private static void SetupChargeRepository(Mock<IChargeRepository> chargeRepository)
        {
            var periods = new List<ChargePeriod>
            {
                new ChargePeriodBuilder()
                    .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                    .Build(),
            };
            var charge = new ChargeBuilder().WithPeriods(periods).Build();
            chargeRepository
                .Setup(r => r.GetOrNullAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(charge);
        }

        private static ChargeCommandReceivedEvent CreateReceivedEventWithChargeOperations()
        {
            var validChargeOperationDto = new ChargeOperationDtoBuilder()
                .WithDescription("valid")
                .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                .WithEndDateTime(InstantHelper.GetEndDefault())
                .Build();
            var invalidChargeOperationDto = new ChargeOperationDtoBuilder()
                .WithDescription("invalid")
                .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                .WithEndDateTime(InstantHelper.GetEndDefault())
                .Build();
            var failedChargeOperationDto = new ChargeOperationDtoBuilder()
                .WithDescription("failed")
                .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                .WithEndDateTime(InstantHelper.GetEndDefault())
                .Build();
            var chargeCommand = new ChargeCommandBuilder()
                .WithChargeOperations(
                    new List<ChargeOperationDto>
                    {
                        validChargeOperationDto,
                        invalidChargeOperationDto,
                        failedChargeOperationDto,
                        failedChargeOperationDto,
                    })
                .Build();
            var receivedEvent = new ChargeCommandReceivedEvent(
                InstantHelper.GetTodayAtMidnightUtc(),
                chargeCommand);
            return receivedEvent;
        }

        private static void SetupValidatorsForOperation(
            Mock<IDocumentValidator<ChargeCommand>> documentValidator,
            Mock<IInputValidator<ChargeCommand>> inputValidator,
            Mock<IBusinessValidator<ChargeCommand>> businessValidator,
            ValidationResult invalidValidationResult)
        {
            inputValidator.Setup(v =>
                v.Validate(It.IsAny<ChargeCommand>())).Returns(ValidationResult.CreateSuccess());
            documentValidator.Setup(v =>
                v.ValidateAsync(It.IsAny<ChargeCommand>())).ReturnsAsync(ValidationResult.CreateSuccess());

            businessValidator.Setup(v =>
                    v.ValidateAsync(It.Is<ChargeCommand>(x =>
                        x.ChargeOperations.Single().ChargeDescription == "valid")))
                .Returns(Task.FromResult(ValidationResult.CreateSuccess()));

            businessValidator.Setup(v =>
                    v.ValidateAsync(It.Is<ChargeCommand>(x =>
                        x.ChargeOperations.Single().ChargeDescription == "invalid")))
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

            return ValidationResult.CreateFailure(new List<IValidationRule> { failedRule.Object });
        }

        private static void SetupValidators(
            Mock<IDocumentValidator<ChargeCommand>> documentValidator,
            Mock<IInputValidator<ChargeCommand>> inputValidator,
            Mock<IBusinessValidator<ChargeCommand>> businessValidator,
            ValidationResult validationResult)
        {
            documentValidator.Setup(v => v.ValidateAsync(It.IsAny<ChargeCommand>()))
                .Returns(Task.FromResult(validationResult));
            inputValidator.Setup(v => v.Validate(It.IsAny<ChargeCommand>())).Returns(validationResult);
            businessValidator.Setup(v => v.ValidateAsync(It.IsAny<ChargeCommand>()))
                .Returns(Task.FromResult(validationResult));
        }
    }
}
