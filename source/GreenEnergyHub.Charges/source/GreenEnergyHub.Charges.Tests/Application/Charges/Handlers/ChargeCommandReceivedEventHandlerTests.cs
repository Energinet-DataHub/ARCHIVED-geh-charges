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
            [Frozen] Mock<IValidator<ChargeCommand, ChargeOperationDto>> validator,
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
                .Setup(s => s.CreateFromChargeOperationDtoAsync(It.IsAny<ChargeOperationDto>()))
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
            [Frozen] Mock<IValidator<ChargeCommand, ChargeOperationDto>> validator,
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
            [Frozen] Mock<IValidator<ChargeCommand, ChargeOperationDto>> validator,
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
                .WithStartDateTime(InstantHelper.GetTodayPlusDaysAtMidnightUtc(3))
                .WithEndDateTime(InstantHelper.GetTodayPlusDaysAtMidnightUtc(4))
                .Build();

            chargeRepository
                .Setup(r => r.GetOrNullAsync(It.IsAny<ChargeIdentifier>()))
                .ReturnsAsync(charge);
            chargePeriodFactory
                .Setup(r => r.CreateFromChargeOperationDto(It.IsAny<ChargeOperationDto>()))
                .Returns(newPeriod);

            var chargeUpdated = false;
            chargeRepository.Setup(r => r.Update(It.IsAny<Charge>()))
                .Callback<Charge>(_ => chargeUpdated = true);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            Assert.True(chargeUpdated);
        }

        private static IEnumerable<ChargePeriod> CreateValidPeriods(int numberOfPeriods = 1)
        {
            for (var i = 0; i < numberOfPeriods; i++)
            {
                yield return new ChargePeriodBuilder()
                    .WithStartDateTime(InstantHelper.GetTodayPlusDaysAtMidnightUtc(i))
                    .WithEndDateTime(InstantHelper.GetTodayPlusDaysAtMidnightUtc(i + 1))
                    .Build();
            }
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
            Mock<IValidator<ChargeCommand, ChargeOperationDto>> validator, ValidationResult validationResult)
        {
            // TODO: add missing setup for ChargeOperationDto
            validator.Setup(v => v.InputValidate(It.IsAny<ChargeCommand>())).Returns(validationResult);
            validator.Setup(v => v.BusinessValidateAsync(It.IsAny<ChargeCommand>()))
                .Returns(Task.FromResult(validationResult));
        }
    }
}
