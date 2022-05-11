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
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.ChargeCommands.Acknowledgement;
using GreenEnergyHub.Charges.Application.ChargeCommands.Handlers;
using GreenEnergyHub.Charges.Application.ChargePrices.Handlers;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Domain.ChargeInformation;
using GreenEnergyHub.Charges.Domain.ChargePrices;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestFiles.Charges;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Handlers
{
    [UnitTest]
    public class ChargePricesHandlerTests
    {
        private static bool _stored;

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidationSucceed_StoreAndConfirmCommand(
            [Frozen] Mock<IInputValidator<ChargeOperationDto>> inputValidator,
            [Frozen] Mock<IBusinessValidator<ChargeOperationDto>> businessValidator,
            [Frozen] Mock<IChargeCommandReceiptService> receiptService,
            [Frozen] Mock<IChargePriceRepository> chargePriceRepository,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IChargePriceFactory> chargePriceFactory,
            ChargePriceBuilder chargePriceBuilder,
            ChargeBuilder chargeBuilder,
            ChargeCommandReceivedEvent receivedEvent,
            ChargePricesHandler sut)
        {
            // Arrange
            var validationResult = ValidationResult.CreateSuccess();
            SetupValidators(inputValidator, businessValidator, validationResult);

            SetupRepositories(chargePriceRepository, chargeRepository, chargeBuilder);
            var confirmed = false;
            receiptService
                .Setup(s => s.AcceptAsync(It.IsAny<ChargeCommand>()))
                .Callback<ChargeCommand>(_ => confirmed = true);

            var chargePrice = chargePriceBuilder.Build();
            chargePriceFactory
                .Setup(s => s.CreateFromChargeOperationDtoAsync(It.IsAny<ChargeOperationDto>(), It.IsAny<Point>()))
                .ReturnsAsync(chargePrice);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            Assert.True(_stored);
            Assert.True(confirmed);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidationFails_RejectsEvent(
            [Frozen] Mock<IInputValidator<ChargeOperationDto>> inputValidator,
            [Frozen] Mock<IBusinessValidator<ChargeOperationDto>> businessValidator,
            [Frozen] Mock<IChargeCommandReceiptService> receiptService,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            ChargeBuilder chargeBuilder,
            ChargeCommandReceivedEvent receivedEvent,
            ChargePricesHandler sut)
        {
            // Arrange
            var validationResult = GetFailedValidationResult();
            SetupValidators(inputValidator, businessValidator, validationResult);
            var rejected = false;
            var charge = chargeBuilder.Build();
            chargeRepository
                .Setup(r => r.GetOrNullAsync(It.IsAny<ChargeInformationIdentifier>()))
                .ReturnsAsync(charge);

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
            ChargePricesHandler sut)
        {
            // Arrange
            ChargeCommandReceivedEvent? receivedEvent = null;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.HandleAsync(receivedEvent!));
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenPriceSeriesWithResolutionPT1H_StoreSeriesAndConfirmCommand(
            [Frozen] Mock<IInputValidator<ChargeOperationDto>> inputValidator,
            [Frozen] Mock<IBusinessValidator<ChargeOperationDto>> businessValidator,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            [Frozen] Mock<IChargePriceRepository> chargePriceRepository,
            [Frozen] Mock<IChargePriceFactory> chargePriceFactory,
            ChargeBuilder chargeBuilder,
            ChargePriceBuilder chargePriceBuilder,
            ChargePricesHandler sut)
        {
            // Arrange
            var validationResult = ValidationResult.CreateSuccess();
            SetupValidators(inputValidator, businessValidator, validationResult);
            SetupRepositories(chargePriceRepository, chargeRepository, chargeBuilder);
            var chargeCommand = CreateChargeCommandWith24Points();
            var chargePrice = chargePriceBuilder.Build();
            chargePriceFactory
                .Setup(x => x.CreateFromChargeOperationDtoAsync(It.IsAny<ChargeOperationDto>(), It.IsAny<Point>()))
                .ReturnsAsync(chargePrice);
            var receivedEvent = new ChargeCommandReceivedEvent(InstantHelper.GetTodayAtMidnightUtc(), chargeCommand);

            // Act
            await sut.HandleAsync(receivedEvent);

            // Assert
            chargePriceRepository.Verify(
                x => x.AddAsync(It.IsAny<ChargePrice>()),
                Times.Exactly(24));
        }

        private static void SetupRepositories(
            [Frozen] Mock<IChargePriceRepository> chargePriceRepository,
            [Frozen] Mock<IChargeRepository> chargeRepository,
            ChargeBuilder chargeBuilder)
        {
            chargePriceRepository
                .Setup(r => r.AddAsync(It.IsAny<ChargePrice>()))
                .Callback<ChargePrice>(_ => _stored = true);
            chargePriceRepository
                .Setup(r => r.GetOrNullAsync(It.IsAny<Guid>(), It.IsAny<Instant>(), It.IsAny<Instant>()))!
                .ReturnsAsync(null as ICollection<ChargePrice>);
            var charge = chargeBuilder.Build();
            chargeRepository
                .Setup(r => r.GetOrNullAsync(It.IsAny<ChargeInformationIdentifier>()))!
                .ReturnsAsync(charge);
        }

        private static void SetupValidators(
            Mock<IInputValidator<ChargeOperationDto>> inputValidator,
            Mock<IBusinessValidator<ChargeOperationDto>> businessValidator,
            ValidationResult validationResult)
        {
            inputValidator.Setup(v => v.Validate(It.IsAny<ChargeOperationDto>())).Returns(validationResult);
            businessValidator.Setup(v => v.ValidateAsync(It.IsAny<ChargeOperationDto>()))
                .Returns(Task.FromResult(validationResult));
        }

        private static ValidationResult GetFailedValidationResult()
        {
            var failedRule = new Mock<IValidationRule>();
            failedRule.Setup(r => r.IsValid).Returns(false);

            return ValidationResult.CreateFailure(new List<IValidationRule> { failedRule.Object });
        }

        private static ChargeCommand CreateChargeCommandWith24Points()
        {
            var points = new List<Point>();
            var price = 0.00M;
            for (var i = 1; i <= 24; i++)
            {
                points.Add(new Point(i, price + i, InstantHelper.GetTodayPlusDaysAtMidnightUtc(i)));
            }

            var operation = new ChargeOperationDtoBuilder()
                .WithPoints(points)
                .Build();

            return new ChargeCommandBuilder()
                .WithChargeOperation(operation)
                .Build();
        }
    }
}
