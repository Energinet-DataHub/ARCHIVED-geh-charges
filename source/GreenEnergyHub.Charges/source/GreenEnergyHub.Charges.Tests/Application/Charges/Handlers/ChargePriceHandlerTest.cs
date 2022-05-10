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
using GreenEnergyHub.Charges.Application.ChargeCommands.Acknowledgement;
using GreenEnergyHub.Charges.Application.ChargePrices.Handlers;
using GreenEnergyHub.Charges.Domain.ChargeInformation;
using GreenEnergyHub.Charges.Domain.ChargePrices;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Handlers
{
    [UnitTest]
    public class ChargePriceHandlerTest
    {
        private static bool _stored = false;

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidationSucceed_StoreAndConfirmCommand(
            [Frozen] Mock<IInputValidator<ChargeCommand>> inputValidator,
            [Frozen] Mock<IBusinessValidator<ChargeCommand>> businessValidator,
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
                .Setup(r => r.GetOrNullAsync(It.IsAny<ChargeIdentifier>()))!
                .ReturnsAsync(charge);
        }

        private static void SetupValidators(
            Mock<IInputValidator<ChargeCommand>> inputValidator,
            Mock<IBusinessValidator<ChargeCommand>> businessValidator,
            ValidationResult validationResult)
        {
            inputValidator.Setup(v => v.Validate(It.IsAny<ChargeCommand>())).Returns(validationResult);
            businessValidator.Setup(v => v.ValidateAsync(It.IsAny<ChargeCommand>()))
                .Returns(Task.FromResult(validationResult));
        }
    }
}
