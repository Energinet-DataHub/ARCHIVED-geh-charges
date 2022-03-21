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
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
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
            [Frozen] Mock<IValidator<ChargeCommand>> validator,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IChargeCommandReceiptService> receiptService,
            [Frozen] Mock<Charge> charge,
            [Frozen] Mock<IChargeFactory> chargeFactory,
            ChargeCommandReceivedEvent receivedEvent,
            ChargeCommandReceivedEventHandler sut)
        {
            // Arrange
            var validationResult = ValidationResult.CreateSuccess();
            SetupValidator(validator, validationResult);

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

            chargeFactory
                .Setup(s => s.CreateFromCommandAsync(It.IsAny<ChargeCommand>()))
                .ReturnsAsync(charge.Object);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            Assert.True(stored);
            Assert.True(confirmed);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidationFails_RejectsEvent(
            [Frozen] Mock<IValidator<ChargeCommand>> validator,
            [Frozen] Mock<IChargeCommandReceiptService> receiptService,
            ChargeCommandReceivedEvent receivedEvent,
            ChargeCommandReceivedEventHandler sut)
        {
            // Arrange
            var validationResult = GetFailedValidationResult();
            SetupValidator(validator, validationResult);

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
            [Frozen] Mock<IValidator<ChargeCommand>> validator,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IChargePeriodFactory> chargePeriodFactory,
            ChargeCommandReceivedEvent receivedEvent,
            ChargeCommandReceivedEventHandler sut)
        {
            // Arrange
            var validationResult = ValidationResult.CreateSuccess();
            SetupValidator(validator, validationResult);
            var periods = CreateValidPeriods(3);
            var charge = CreateValidCharge(periods);
            var newPeriod = new ChargePeriodBuilder()
                .WithStartDateTime(InstantHelper.GetTomorrowAtMidnightUtc())
                .Build();

            chargeRepository
                .Setup(r => r.GetOrNullAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(charge);
            chargePeriodFactory
                .Setup(r => r.CreateFromChargeOperationDto(It.IsAny<Instant>(), It.IsAny<ChargeOperationDto>()))
                .Returns(newPeriod);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            charge.Periods.Count.Should().Be(2);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_IfValidStopEvent_ChargeStopped(
            [Frozen] Mock<IValidator<ChargeCommand>> validator,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IChargePeriodFactory> chargePeriodFactory,
            [Frozen] Instant stopDate,
            ChargeCommandBuilder chargeCommandBuilder,
            ChargeCommandReceivedEventHandler sut)
        {
            // Arrange
            var chargeCommand = chargeCommandBuilder.WithOperationType(OperationType.Stop).Build();
            var receivedEvent = new ChargeCommandReceivedEvent(SystemClock.Instance.GetCurrentInstant(), chargeCommand);
            var validationResult = ValidationResult.CreateSuccess();
            SetupValidator(validator, validationResult);
            var periods = CreateValidPeriodsFromOffset(stopDate);
            var charge = CreateValidCharge(periods);
            var newPeriod = new ChargePeriodBuilder().WithStartDateTime(stopDate).WithIsStop(true).Build();

            chargeRepository
                .Setup(r => r.GetOrNullAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(charge);
            chargePeriodFactory
                .Setup(r => r.CreateFromChargeOperationDto(It.IsAny<Instant>(), It.IsAny<ChargeOperationDto>()))
                .Returns(newPeriod);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            charge.Periods.Count.Should().Be(4);
            var actual = charge.Periods
                .OrderByDescending(p => p.ReceivedDateTime)
                .ThenByDescending(p => p.ReceivedOrder)
                .First();
            // actual.EndDateTime.Should().Be(stopDate);
            actual.IsStop.Should().Be(true);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidCancelStop_ThenStopCancelled(
            [Frozen] Mock<IValidator<ChargeCommand>> validator,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IChargePeriodFactory> chargePeriodFactory,
            ChargeCommandReceivedEventHandler sut)
        {
            // Arrange
            var validationResult = ValidationResult.CreateSuccess();
            SetupValidator(validator, validationResult);
            var chargeCommand = new ChargeCommandBuilder()
                .WithStartDateTime(InstantHelper.GetTomorrowAtMidnightUtc())
                .WithOperationType(OperationType.CancelStop)
                /*.WithEndDateTime(InstantHelper.GetEndDefault())*/
                .Build();
            var receivedEvent = new ChargeCommandReceivedEvent(InstantHelper.GetTodayAtMidnightUtc(), chargeCommand);
            var periods = new List<ChargePeriod>
            {
                new ChargePeriodBuilder().WithIsStop(true).Build(),
            };
            var charge = new ChargeBuilder().WithPeriods(periods).Build();
            var newPeriod = new ChargePeriodBuilder()
                .WithStartDateTime(InstantHelper.GetTomorrowAtMidnightUtc())
                .Build();
            chargeRepository
                .Setup(r => r.GetOrNullAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(charge);
            chargePeriodFactory
                .Setup(r => r.CreateFromChargeOperationDto(It.IsAny<Instant>(), It.IsAny<ChargeOperationDto>()))
                .Returns(newPeriod);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            charge.Periods.Count.Should().Be(1);
            var actual = charge.Periods
                .OrderByDescending(p => p.ReceivedDateTime)
                .ThenByDescending(p => p.ReceivedOrder)
                .First();
            // actual.EndDateTime.Should().Be(InstantHelper.GetEndDefault());
            actual.IsStop.Should().Be(false);
        }

        private static IEnumerable<ChargePeriod> CreateValidPeriods(int numberOfPeriods = 1)
        {
            for (var i = 0; i < numberOfPeriods; i++)
            {
                yield return new ChargePeriodBuilder()
                    .WithStartDateTime(InstantHelper.GetTodayPlusDaysAtMidnightUtc(i))
                    .Build();
            }
        }

        private static IEnumerable<ChargePeriod> CreateValidPeriodsFromOffset(Instant offsetDate)
        {
            return new List<ChargePeriod>
            {
                new ChargePeriodBuilder()
                    .WithStartDateTime(offsetDate.Minus(Duration.FromDays(5)))
                    //.WithIsStop(offsetDate.Minus(Duration.FromDays(1)))
                    .Build(),
                new ChargePeriodBuilder()
                    .WithStartDateTime(offsetDate.Minus(Duration.FromDays(1)))
                    //.WithIsStop(offsetDate.Plus(Duration.FromDays(5)))
                    .Build(),
                new ChargePeriodBuilder()
                    .WithStartDateTime(offsetDate.Plus(Duration.FromDays(5)))
                    //.WithIsStop(InstantHelper.GetEndDefault())
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

        private static void SetupValidator(
            Mock<IValidator<ChargeCommand>> validator, ValidationResult validationResult)
        {
            validator.Setup(v => v.InputValidate(It.IsAny<ChargeCommand>())).Returns(validationResult);
            validator.Setup(v => v.BusinessValidateAsync(It.IsAny<ChargeCommand>()))
                .Returns(Task.FromResult(validationResult));
        }
    }
}
