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
using GreenEnergyHub.Charges.Application.ChargeCommands.Acknowledgement;
using GreenEnergyHub.Charges.Application.ChargeInformations.Handlers;
using GreenEnergyHub.Charges.Domain.ChargeInformations;
using GreenEnergyHub.Charges.Domain.ChargePrices;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.Charges.Tests.Domain.Dtos.ChargeCommands.Validation;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;
using Duration = NodaTime.Duration;

namespace GreenEnergyHub.Charges.Tests.Application.ChargeInformations.Handlers
{
    [UnitTest]
    public class ChargeInformationHandlerTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidationSucceed_StoreAndConfirmCommand(
            [Frozen] Mock<IInputValidator<ChargeOperationDto>> inputValidator,
            [Frozen] Mock<IBusinessValidator<ChargeOperationDto>> businessValidator,
            [Frozen] Mock<IChargeInformationRepository> chargeRepository,
            [Frozen] Mock<IChargeCommandReceiptService> receiptService,
            ChargeInformationBuilder chargeInformationBuilder,
            [Frozen] Mock<IChargeInformationFactory> chargeFactory,
            [Frozen] Mock<IChargePeriodFactory> chargePeriodFactory,
            ChargeCommandReceivedEvent receivedEvent,
            ChargeInformationHandler sut)
        {
            // Arrange
            var validationResult = ValidationResult.CreateSuccess();
            SetupValidators(inputValidator, businessValidator, validationResult);

            var stored = false;
            chargeRepository
                .Setup(r => r.AddAsync(It.IsAny<ChargeInformation>()))
                .Callback<ChargeInformation>(_ => stored = true);
            chargeRepository
                .Setup(r => r.GetOrNullAsync(It.IsAny<ChargeInformationIdentifier>()))
                .ReturnsAsync(null as ChargeInformation);

            var confirmed = false;
            receiptService
                .Setup(s => s.AcceptAsync(It.IsAny<ChargeCommand>()))
                .Callback<ChargeCommand>(_ => confirmed = true);

            var charge = chargeInformationBuilder.WithPeriods(new List<ChargePeriod> { CreateValidPeriod() }).Build();
            chargeFactory
                .Setup(s => s.CreateFromChargeOperationDtoAsync(It.IsAny<ChargeOperationDto>()))
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
            [Frozen] Mock<IInputValidator<ChargeOperationDto>> inputValidator,
            [Frozen] Mock<IBusinessValidator<ChargeOperationDto>> businessValidator,
            [Frozen] Mock<IChargeCommandReceiptService> receiptService,
            [Frozen] Mock<IChargeInformationRepository> chargeRepository,
            ChargeInformationBuilder chargeInformationBuilder,
            ChargeCommandReceivedEvent receivedEvent,
            ChargeInformationHandler sut)
        {
            // Arrange
            var charge = chargeInformationBuilder.Build();
            var validationResult = GetFailedValidationResult();
            SetupValidators(inputValidator, businessValidator, validationResult);

            chargeRepository
                .Setup(r => r.GetOrNullAsync(It.IsAny<ChargeInformationIdentifier>()))
                .ReturnsAsync(charge);

            var rejected = false;
            receiptService.Setup(s => s.RejectAsync(It.IsAny<ChargeCommand>(), It.IsAny<ValidationResult>()))
                .Callback<ChargeCommand, ValidationResult>((_, _) => rejected = true);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            Assert.True(rejected);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_IfEventIsNull_ThrowsArgumentNullException(
            ChargeInformationHandler sut)
        {
            // Arrange
            ChargeCommandReceivedEvent? receivedEvent = null;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.HandleAsync(receivedEvent!));
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_IfValidUpdateEvent_ChargeUpdated(
            [Frozen] Mock<IInputValidator<ChargeOperationDto>> inputValidator,
            [Frozen] Mock<IBusinessValidator<ChargeOperationDto>> businessValidator,
            [Frozen] Mock<IChargeInformationRepository> chargeRepository,
            [Frozen] Mock<IChargePeriodFactory> chargePeriodFactory,
            ChargeCommandReceivedEvent receivedEvent,
            ChargeInformationHandler sut)
        {
            // Arrange
            var validationResult = ValidationResult.CreateSuccess();
            SetupValidators(inputValidator, businessValidator, validationResult);
            var periods = CreateValidPeriods(3);
            var charge = CreateValidCharge(periods);
            var newPeriod = new ChargePeriodBuilder()
                .WithStartDateTime(InstantHelper.GetTomorrowAtMidnightUtc())
                .WithEndDateTime(InstantHelper.GetEndDefault())
                .Build();

            chargeRepository
                .Setup(r => r.GetOrNullAsync(It.IsAny<ChargeInformationIdentifier>()))
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
            [Frozen] Mock<IInputValidator<ChargeOperationDto>> inputValidator,
            [Frozen] Mock<IBusinessValidator<ChargeOperationDto>> businessValidator,
            [Frozen] Mock<IChargeInformationRepository> chargeRepository,
            [Frozen] Mock<IChargePeriodFactory> chargePeriodFactory,
            [Frozen] Mock<IChargePriceRepository> chargePriceRepository,
            [Frozen] Instant stopDate,
            ChargePriceSeriesBuilder priceSeriesBuilder,
            ChargeCommandReceivedEvent receivedEvent,
            ChargeInformationHandler sut)
        {
            // Arrange
            var validationResult = ValidationResult.CreateSuccess();
            SetupValidators(inputValidator, businessValidator, validationResult);
            var periods = CreateValidPeriodsFromOffset(stopDate);
            var charge = CreateValidCharge(periods);
            var newPeriod = new ChargePeriodBuilder().WithStartDateTime(stopDate).WithEndDateTime(stopDate).Build();
            chargeRepository
                .Setup(r => r.GetOrNullAsync(It.IsAny<ChargeInformationIdentifier>()))
                .ReturnsAsync(charge);
            chargePeriodFactory
                .Setup(r => r.CreateFromChargeOperationDto(It.IsAny<ChargeOperationDto>()))
                .Returns(newPeriod);

            var priceList = priceSeriesBuilder
                .WithTimeAndPrice(InstantHelper.GetYesterdayAtMidnightUtc(), 1.00m)
                .WithTimeAndPrice(InstantHelper.GetTodayAtMidnightUtc(), 2.00m)
                .WithTimeAndPrice(InstantHelper.GetTomorrowAtMidnightUtc(), 3.00m)
                .WithTimeAndPrice(InstantHelper.GetTodayPlusDaysAtMidnightUtc(2), 4.00m)
                .Build();
            chargePriceRepository
                .Setup(r => r.GetOrNullAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Instant>(),
                    It.IsAny<Instant>()))!
                .ReturnsAsync(null as IEnumerable<ChargePrice>);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            charge.Periods.Count.Should().Be(2);
            var actual = charge.Periods.OrderByDescending(p => p.StartDateTime).First();
            actual.EndDateTime.Should().Be(stopDate);
            chargePriceRepository.Verify(x => x.RemoveRange(
                It.IsAny<IEnumerable<ChargePrice>>()));
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidCancelStop_ThenStopCancelled(
            [Frozen] Mock<IInputValidator<ChargeOperationDto>> inputValidator,
            [Frozen] Mock<IBusinessValidator<ChargeOperationDto>> businessValidator,
            [Frozen] Mock<IChargeInformationRepository> chargeRepository,
            [Frozen] Mock<IChargePeriodFactory> chargePeriodFactory,
            ChargeInformationHandler sut)
        {
            // Arrange
            var validationResult = ValidationResult.CreateSuccess();
            SetupValidators(inputValidator, businessValidator, validationResult);
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
            var charge = new ChargeInformationBuilder().WithPeriods(periods).Build();
            var newPeriod = new ChargePeriodBuilder()
                .WithStartDateTime(InstantHelper.GetTomorrowAtMidnightUtc())
                .Build();
            chargeRepository
                .Setup(r => r.GetOrNullAsync(It.IsAny<ChargeInformationIdentifier>()))
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
             [Frozen] Mock<IChargeInformationRepository> chargeRepository,
             [Frozen] Mock<IChargePeriodFactory> chargePeriodFactory,
             [Frozen] Mock<IDocumentValidator<ChargeCommand>> documentValidator,
             [Frozen] Mock<IInputValidator<ChargeOperationDto>> inputValidator,
             [Frozen] Mock<IBusinessValidator<ChargeOperationDto>> businessValidator,
             [Frozen] Mock<IChargeCommandReceiptService> receiptService,
             ChargeInformationHandler sut)
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
                 .Callback<ChargeCommand>(_ => accepted++);

             var validationResultsArgs = new List<ValidationResult>();
             receiptService.Setup(s => s.RejectAsync(It.IsAny<ChargeCommand>(), It.IsAny<ValidationResult>()))
                 .Callback<ChargeCommand, ValidationResult>((_, s) => validationResultsArgs.Add(s));

             // Act
             await sut.HandleAsync(receivedEvent);

             // Assert
             accepted.Should().Be(1);

             var validationRules = validationResultsArgs.Single().InvalidRules.ToList();
             var invalid = validationRules.Where(vr =>
                 vr.ValidationRuleIdentifier == ValidationRuleIdentifier.StartDateValidation);
             var subsequent = validationRules.Where(vr =>
                 vr.ValidationRuleIdentifier == ValidationRuleIdentifier.SubsequentBundleOperationsFail);

             validationRules.Count.Should().Be(3);
             invalid.Count().Should().Be(1);
             subsequent.Count().Should().Be(2);
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

        private static void SetupChargeRepository(Mock<IChargeInformationRepository> chargeRepository)
        {
            var periods = new List<ChargePeriod>
            {
                new ChargePeriodBuilder()
                    .WithStartDateTime(InstantHelper.GetYesterdayAtMidnightUtc())
                    .Build(),
            };
            var charge = new ChargeInformationBuilder().WithPeriods(periods).Build();
            chargeRepository
                .Setup(r => r.GetOrNullAsync(It.IsAny<ChargeInformationIdentifier>()))
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
            Mock<IInputValidator<ChargeOperationDto>> inputValidator,
            Mock<IBusinessValidator<ChargeOperationDto>> businessValidator,
            ValidationResult invalidValidationResult)
        {
            inputValidator.Setup(v =>
                v.Validate(It.IsAny<ChargeOperationDto>())).Returns(ValidationResult.CreateSuccess());
            documentValidator.Setup(v =>
                v.ValidateAsync(It.IsAny<ChargeCommand>())).ReturnsAsync(ValidationResult.CreateSuccess());

            businessValidator.Setup(v =>
                    v.ValidateAsync(It.Is<ChargeOperationDto>(x =>
                        x.ChargeDescription == "valid")))
                .Returns(Task.FromResult(ValidationResult.CreateSuccess()));

            businessValidator.Setup(v =>
                    v.ValidateAsync(It.Is<ChargeOperationDto>(x =>
                        x.ChargeDescription == "invalid")))
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

        private static ChargeInformation CreateValidCharge(IEnumerable<ChargePeriod> periods)
        {
            return new ChargeInformationBuilder()
                .WithPeriods(periods)
                .Build();
        }

        private static void SetupValidators(
            Mock<IInputValidator<ChargeOperationDto>> inputValidator,
            Mock<IBusinessValidator<ChargeOperationDto>> businessValidator,
            ValidationResult validationResult)
        {
            inputValidator.Setup(v => v.Validate(It.IsAny<ChargeOperationDto>()))
                .Returns(validationResult);
            businessValidator.Setup(v => v.ValidateAsync(It.IsAny<ChargeOperationDto>()))
                .Returns(Task.FromResult(validationResult));
        }

        private static ValidationResult GetFailedValidationResult()
        {
            var failedRule = new Mock<IValidationRule>();
            failedRule.Setup(r => r.IsValid).Returns(false);

            return ValidationResult.CreateFailure(new List<IValidationRule> { failedRule.Object });
        }
    }
}
