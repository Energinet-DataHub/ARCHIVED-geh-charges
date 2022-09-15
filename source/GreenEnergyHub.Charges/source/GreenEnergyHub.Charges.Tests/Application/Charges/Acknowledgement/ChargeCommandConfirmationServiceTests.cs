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
            [Frozen] Mock<IChargeInformationCommandRejectedEventFactory> rejectedEventFactory,
            [Frozen] Mock<IDomainEventDispatcher> domainEventDispatcher,
            ChargeInformationCommand command,
            ValidationResult validationResult,
            ChargeInformationCommandRejectedEvent rejectedEvent,
            ChargeCommandReceiptService sut)
        {
            // Arrange
            rejectedEventFactory.Setup(
                f => f.CreateEvent(
                    It.IsAny<ChargeInformationCommand>(),
                    It.IsAny<ValidationResult>()))
                .Returns(rejectedEvent);

            ChargeInformationCommandRejectedEvent? eventForSerialization = null;
            domainEventDispatcher.Setup(
                    d => d.DispatchAsync(
                        It.IsAny<ChargeInformationCommandRejectedEvent>(),
                        It.IsAny<CancellationToken>()))
                .Callback<ChargeInformationCommandRejectedEvent, CancellationToken>(
                    (e, _) => eventForSerialization = e);

            // Act
            await sut.RejectAsync(command, validationResult).ConfigureAwait(false);

            // Assert
            Assert.Equal(rejectedEvent, eventForSerialization);
        }

        [Theory]
        [AutoMoqData]
        public async Task AcceptAsync_WhenCalledWithCommand_UsesFactoryToCreateEventAndDispatchesIt(
            [Frozen] Mock<IChargeInformationCommandAcceptedEventFactory> acceptedEventFactory,
            [Frozen] Mock<IDomainEventDispatcher> domainEventDispatcher,
            ChargeInformationCommand command,
            ChargeInformationCommandAcceptedEvent acceptedEvent,
            ChargeCommandReceiptService sut)
        {
            // Arrange
            acceptedEventFactory.Setup(
                    f => f.CreateEvent(
                        It.IsAny<ChargeInformationCommand>()))
                .Returns(acceptedEvent);

            ChargeInformationCommandAcceptedEvent? eventForSerialization = null;
            domainEventDispatcher.Setup(
                    d => d.DispatchAsync(
                        It.IsAny<ChargeInformationCommandAcceptedEvent>(),
                        It.IsAny<CancellationToken>()))
                .Callback<ChargeInformationCommandAcceptedEvent, CancellationToken>(
                    (e, _) => eventForSerialization = e);

            // Act
            await sut.AcceptAsync(command).ConfigureAwait(false);

            // Assert
            Assert.Equal(acceptedEvent, eventForSerialization);
        }
    }
}
