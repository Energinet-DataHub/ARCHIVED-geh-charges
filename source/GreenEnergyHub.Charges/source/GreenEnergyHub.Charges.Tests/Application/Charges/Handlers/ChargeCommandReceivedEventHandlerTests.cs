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
using System.Threading.Tasks;
using AutoFixture.Xunit2;
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
            ChargeCommandBuilder chargeCommandBuilder,
            ChargeCommandReceivedEventHandler sut)
        {
            // Arrange
            var chargeCommand = chargeCommandBuilder.WithOperationType(OperationType.Create).Build();
            var receivedEvent = new ChargeCommandReceivedEvent(SystemClock.Instance.GetCurrentInstant(), chargeCommand);
            SetupValidator(validator, ValidationResult.CreateSuccess());

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
            [Frozen] Mock<IChargeFactory> chargeFactory,
            ChargeCommandBuilder chargeCommandBuilder,
            ChargeCommandReceivedEventHandler sut)
        {
            // Arrange
            var chargeCommand = chargeCommandBuilder.WithOperationType(OperationType.Update).Build();
            var receivedEvent = new ChargeCommandReceivedEvent(SystemClock.Instance.GetCurrentInstant(), chargeCommand);
            SetupValidator(validator, ValidationResult.CreateSuccess());
            var periods = CreateValidCharges(3);
            var charge = CreateValidCharge(periods);
            var newPeriod = new ChargeBuilder()
                .WithStartDateTime(InstantHelper.GetTomorrowAtMidnightUtc())
                .Build();

            chargeRepository.Setup(r => r.AddAsync(newPeriod));
            chargeFactory.Setup(cf => cf.CreateFromCommandAsync(chargeCommand)).ReturnsAsync(newPeriod);

            /*chargeRepository
                .Setup(r => r.GetOrNullAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(charge);*/
            /*chargePeriodFactory
                .Setup(r => r.CreateFromChargeOperationDto(It.IsAny<Instant>(), It.IsAny<ChargeOperationDto>()))
                .Returns(newPeriod);*/

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert TODO
            chargeRepository.Verify(x => x.AddAsync(It.IsAny<Charge>()), Times.Once);
            /*charge.Periods.Count.Should().Be(4);*/
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_IfValidStopEvent_ChargeStopped(
            [Frozen] Mock<IValidator<ChargeCommand>> validator,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IChargeFactory> chargeFactory,
            [Frozen] Instant stopDate,
            ChargeCommandBuilder chargeCommandBuilder,
            ChargeCommandReceivedEventHandler sut)
        {
            // Arrange
            var chargeCommand = chargeCommandBuilder.WithOperationType(OperationType.Stop).Build();
            var receivedEvent = new ChargeCommandReceivedEvent(SystemClock.Instance.GetCurrentInstant(), chargeCommand);
            var validationResult = ValidationResult.CreateSuccess();
            SetupValidator(validator, validationResult);
            var existingCharges = CreateValidChargesFromOffset(stopDate);
            var stopPeriod = new ChargeBuilder().WithStartDateTime(stopDate).WithIsStop(true).Build();

            chargeRepository.Setup(r => r.AddAsync(stopPeriod));
            chargeFactory.Setup(cf => cf.CreateFromCommandAsync(chargeCommand)).ReturnsAsync(stopPeriod);
            /*chargeRepository
                .Setup(r => r.GetOrNullAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(existingCharges.FirstOrDefault);*/
            /*chargePeriodFactory
                .Setup(r => r.CreateFromChargeOperationDto(It.IsAny<Instant>(), It.IsAny<ChargeOperationDto>()))
                .Returns(newPeriod);*/

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert TODO
            chargeRepository.Verify(x => x.AddAsync(It.IsAny<Charge>()), Times.Once);
            /*charge.Periods.Count.Should().Be(4);
            var actual = charge.Periods.OrderedByReceivedDateTimeAndOrder().First();
            // actual.EndDateTime.Should().Be(stopDate);
            actual.IsStop.Should().Be(true);*/
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidCancelStop_ThenStopCancelled(
            [Frozen] Mock<IValidator<ChargeCommand>> validator,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IChargeFactory> chargeFactory,
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
            /*var periods = new List<Charge>
            {
                new ChargeBuilder().WithIsStop(true).Build(),
            };*/
            var charge = new ChargeBuilder()
                /*.WithPeriods(periods)*/
                .WithIsStop(true)
                .Build();
            var newPeriod = new ChargeBuilder()
                .WithStartDateTime(InstantHelper.GetTomorrowAtMidnightUtc())
                .Build();

            chargeRepository.Setup(r => r.AddAsync(newPeriod));
            chargeFactory.Setup(cf => cf.CreateFromCommandAsync(chargeCommand)).ReturnsAsync(newPeriod);
            chargeRepository.Setup(r => r.GetStopOrNullAsync(It.IsAny<ChargeIdentifier>())).ReturnsAsync(charge);
            /*chargePeriodFactory
                .Setup(r => r.CreateFromChargeOperationDto(It.IsAny<Instant>(), It.IsAny<ChargeOperationDto>()))
                .Returns(newPeriod);*/

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert TODO
            chargeRepository.Verify(x => x.Remove(It.IsAny<Charge>()), Times.Once);
            chargeRepository.Verify(x => x.AddAsync(It.IsAny<Charge>()), Times.Once);
            /*charge.Periods.Count.Should().Be(2);
            var actual = charge.Periods.OrderedByReceivedDateTimeAndOrder().First();
            actual.StartDateTime.Should().Be(InstantHelper.GetTomorrowAtMidnightUtc());
            // actual.EndDateTime.Should().Be(InstantHelper.GetEndDefault());
            actual.IsStop.Should().Be(false);*/
        }

        private static IEnumerable<Charge> CreateValidCharges(int numberOfPeriods = 1)
        {
            for (var i = 0; i < numberOfPeriods; i++)
            {
                yield return new ChargeBuilder()
                    .WithStartDateTime(InstantHelper.GetTodayPlusDaysAtMidnightUtc(i))
                    .Build();
            }
        }

        private static IEnumerable<Charge> CreateValidChargesFromOffset(Instant offsetDate)
        {
            return new List<Charge>
            {
                new ChargeBuilder()
                    .WithStartDateTime(offsetDate.Minus(Duration.FromDays(5)))
                    //.WithIsStop(offsetDate.Minus(Duration.FromDays(1)))
                    .Build(),
                new ChargeBuilder()
                    .WithStartDateTime(offsetDate.Minus(Duration.FromDays(1)))
                    //.WithIsStop(offsetDate.Plus(Duration.FromDays(5)))
                    .Build(),
                new ChargeBuilder()
                    .WithStartDateTime(offsetDate.Plus(Duration.FromDays(5)))
                    //.WithIsStop(InstantHelper.GetEndDefault())
                    .Build(),
            };
        }

        private static Charge CreateValidCharge(IEnumerable<Charge> periods)
        {
            return new ChargeBuilder()
                /*.WithPeriods(periods)*/
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
