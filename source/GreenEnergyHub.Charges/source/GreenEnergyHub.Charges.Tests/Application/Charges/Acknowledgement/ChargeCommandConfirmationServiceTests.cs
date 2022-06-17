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

using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Application.Charges.Acknowledgement;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandRejectedEvents;

using GreenEnergyHub.Charges.Domain.Dtos.ChargeInformationCommands;
using GreenEnergyHub.Charges.Domain.Dtos.Validation;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Acknowledgement
{
    [UnitTest]
    public class ChargeCommandConfirmationServiceTests
    {
        [Theory]
        [AutoMoqData]
        public async Task RejectAsync_WhenCalledWithCommandAndResult_UsesFactoryToCreateEventAndDispatchesIt(
            [Frozen] Mock<IChargeCommandRejectedEventFactory> rejectedEventFactory,
            [Frozen] Mock<IMessageDispatcher<ChargeCommandRejectedEvent>> rejectedEventDispatcher,
            ChargeInformationCommand command,
            ValidationResult validationResult,
            ChargeCommandRejectedEvent rejectedEvent,
            ChargeCommandReceiptService sut)
        {
            // Arrange
            rejectedEventFactory.Setup(
                f => f.CreateEvent(
                    It.IsAny<ChargeInformationCommand>(),
                    It.IsAny<ValidationResult>()))
                .Returns(rejectedEvent);

            ChargeCommandRejectedEvent? eventForSerialization = null;
            rejectedEventDispatcher.Setup(
                    d => d.DispatchAsync(
                        It.IsAny<ChargeCommandRejectedEvent>(),
                        It.IsAny<CancellationToken>()))
                .Callback<ChargeCommandRejectedEvent, CancellationToken>(
                    (e, _) => eventForSerialization = e);

            // Act
            await sut.RejectAsync(command, validationResult).ConfigureAwait(false);

            // Assert
            Assert.Equal(rejectedEvent, eventForSerialization);
        }

        [Theory]
        [AutoMoqData]
        public async Task AcceptAsync_WhenCalledWithCommand_UsesFactoryToCreateEventAndDispatchesIt(
            [Frozen] Mock<IChargeCommandAcceptedEventFactory> acceptedEventFactory,
            [Frozen] Mock<IMessageDispatcher<ChargeCommandAcceptedEvent>> acceptedEventDispatcher,
            ChargeInformationCommand command,
            ChargeCommandAcceptedEvent acceptedEvent,
            ChargeCommandReceiptService sut)
        {
            // Arrange
            acceptedEventFactory.Setup(
                    f => f.CreateEvent(
                        It.IsAny<ChargeInformationCommand>()))
                .Returns(acceptedEvent);

            ChargeCommandAcceptedEvent? eventForSerialization = null;
            acceptedEventDispatcher.Setup(
                    d => d.DispatchAsync(
                        It.IsAny<ChargeCommandAcceptedEvent>(),
                        It.IsAny<CancellationToken>()))
                .Callback<ChargeCommandAcceptedEvent, CancellationToken>(
                    (e, _) => eventForSerialization = e);

            // Act
            await sut.AcceptAsync(command).ConfigureAwait(false);

            // Assert
            Assert.Equal(acceptedEvent, eventForSerialization);
        }
    }
}
