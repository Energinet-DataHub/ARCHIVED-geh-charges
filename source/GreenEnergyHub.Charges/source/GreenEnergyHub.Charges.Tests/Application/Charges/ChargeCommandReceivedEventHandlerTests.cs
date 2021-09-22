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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Application.Charges.Acknowledgement;
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Domain.ChargeCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.ChargeCommands;
using GreenEnergyHub.Charges.Domain.ChargeCommands.Validation;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges
{
    [UnitTest]
    public class ChargeCommandReceivedEventHandlerTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidationSucceed_StoreAndConfirmCommand(
            [Frozen] [NotNull] Mock<IChargeCommandValidator> validator,
            [Frozen] [NotNull] Mock<IChargeRepository> repository,
            [Frozen] [NotNull] Mock<IChargeCommandConfirmationService> confirmationService,
            [NotNull] ChargeCommandReceivedEvent receivedEvent,
            [NotNull] ChargeCommandReceivedEventHandler sut)
        {
            // Arrange
            var validationResult = ChargeCommandValidationResult.CreateSuccess();
            validator.Setup(
                    v => v.ValidateAsync(
                        It.IsAny<ChargeCommand>()))
                .Returns(
                    Task.FromResult(validationResult));

            var stored = false;
            repository.Setup(
                    r => r.StoreChargeAsync(
                        It.IsAny<Charge>()))
                .Callback<Charge>(
                    (_) => stored = true);

            var confirmed = false;
            confirmationService.Setup(
                    s => s.AcceptAsync(
                        It.IsAny<ChargeCommand>()))
                .Callback<ChargeCommand>(
                    (_) => confirmed = true);

            // Act
            await sut.HandleAsync(receivedEvent).ConfigureAwait(false);

            // Assert
            Assert.True(stored);
            Assert.True(confirmed);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_WhenValidationFails_RejectsEvent(
            [Frozen] [NotNull] Mock<IChargeCommandValidator> validator,
            [Frozen] [NotNull] Mock<IChargeCommandConfirmationService> confirmationService,
            [NotNull] ChargeCommandReceivedEvent receivedEvent,
            [NotNull] ChargeCommandReceivedEventHandler sut)
        {
            // Arrange
            var validationResult = GetFailedValidationResult();
            validator.Setup(
                    v => v.ValidateAsync(
                        It.IsAny<ChargeCommand>()))
                .Returns(
                    Task.FromResult(validationResult));

            var rejected = false;
            confirmationService.Setup(
                    s => s.RejectAsync(
                        It.IsAny<ChargeCommand>(),
                        validationResult))
                .Callback<ChargeCommand, ChargeCommandValidationResult>(
                    (_, _) => rejected = true);

            // Act
            await sut.HandleAsync(receivedEvent).ConfigureAwait(false);

            // Assert
            Assert.True(rejected);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task HandleAsync_IfEventIsNull_ThrowsArgumentNullException(
            [NotNull] ChargeCommandReceivedEventHandler sut)
        {
            // Arrange
            ChargeCommandReceivedEvent? receivedEvent = null;

            // Act / Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => sut.HandleAsync(receivedEvent!))
                .ConfigureAwait(false);
        }

        private static ChargeCommandValidationResult GetFailedValidationResult()
        {
            var failedRule = new Mock<IValidationRule>();
            failedRule.Setup(
                    r => r.IsValid)
                .Returns(false);

            return ChargeCommandValidationResult.CreateFailure(
                new List<IValidationRule> { failedRule.Object });
        }
    }
}
