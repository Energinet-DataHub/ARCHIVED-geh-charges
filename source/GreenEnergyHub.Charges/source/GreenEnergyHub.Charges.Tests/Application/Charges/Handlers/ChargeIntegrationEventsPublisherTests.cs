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

using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Application.Charges.Acknowledgement;
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.ChargeCommands;
using GreenEnergyHub.Charges.Tests.Builders.Command;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Handlers
{
    [UnitTest]
    public class ChargeIntegrationEventsPublisherTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalledWithPrices_ShouldCallBothSenders(
            [Frozen] Mock<IChargePublisher> chargeSender,
            [Frozen] Mock<IChargePricesUpdatedPublisher> chargePricesUpdatedSender,
            ChargeCommandAcceptedEvent chargeCommandAcceptedEvent,
            ChargeIntegrationEventsPublisher sut)
        {
            // Act
            await sut.PublishAsync(chargeCommandAcceptedEvent).ConfigureAwait(false);

            // Assert
            chargeSender.Verify(x => x.PublishChargeCreatedAsync(It.IsAny<ChargeOperationDto>()), Times.Exactly(3));
            chargePricesUpdatedSender.Verify(x => x.PublishChargePricesAsync(It.IsAny<ChargeOperationDto>()), Times.Exactly(3));
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalledWithoutPrices_ShouldOnlyCallChargeCreatedSender(
            [Frozen] Mock<IChargePublisher> chargeSender,
            [Frozen] Mock<IChargePricesUpdatedPublisher> chargePricesUpdatedSender,
            ChargeCommandBuilder chargeCommandBuilder,
            ChargeCommandAcceptedEventBuilder chargeCommandAcceptedEventBuilder,
            ChargeIntegrationEventsPublisher sut)
        {
            // Arrange
            var chargeCommand = chargeCommandBuilder.Build();
            var acceptedEvent = chargeCommandAcceptedEventBuilder.WithChargeCommand(chargeCommand).Build();

            // Act
            await sut.PublishAsync(acceptedEvent).ConfigureAwait(false);

            // Assert
            chargeSender.Verify(x => x.PublishChargeCreatedAsync(It.IsAny<ChargeOperationDto>()), Times.Once);
            chargePricesUpdatedSender.Verify(x => x.PublishChargePricesAsync(It.IsAny<ChargeOperationDto>()), Times.Never);
        }
    }
}
