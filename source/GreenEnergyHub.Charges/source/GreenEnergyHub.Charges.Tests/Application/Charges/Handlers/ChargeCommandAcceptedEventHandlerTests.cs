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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Application.Charges.Acknowledgement;
using GreenEnergyHub.Charges.Application.Charges.Handlers;
using GreenEnergyHub.Charges.Domain.ChargeCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.Handlers
{
    public class ChargeCommandAcceptedEventHandlerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalledWithPrices_ShouldCallBothSenders(
            [NotNull] [Frozen] Mock<IChargePublisher> chargeSender,
            [NotNull] [Frozen] Mock<IChargePricesUpdatedPublisher> chargePricesUpdatedSender,
            [NotNull] ChargeCommandAcceptedEvent chargeCommandAcceptedEvent,
            [NotNull] ChargeCommandAcceptedEventHandler sut)
        {
            // Act
            await sut.HandleAsync(chargeCommandAcceptedEvent).ConfigureAwait(false);

            // Assert
            chargeSender.Verify(x => x.PublishChargeCreatedAsync(It.IsAny<ChargeCommandAcceptedEvent>()), Times.Once);
            chargePricesUpdatedSender.Verify(x => x.PublishChargePricesAsync(It.IsAny<ChargeCommandAcceptedEvent>()), Times.Once);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task HandleAsync_WhenCalledWithoutPrices_ShouldOnlyCallChargeCreatedSender(
            [NotNull] [Frozen] Mock<IChargePublisher> chargeSender,
            [NotNull] [Frozen] Mock<IChargePricesUpdatedPublisher> chargePricesUpdatedSender,
            [NotNull] ChargeCommandAcceptedEvent chargeCommandAcceptedEvent,
            [NotNull] ChargeCommandAcceptedEventHandler sut)
        {
            // Arrange
            chargeCommandAcceptedEvent.Command.ChargeOperation.Points = new List<Point>();

            // Act
            await sut.HandleAsync(chargeCommandAcceptedEvent).ConfigureAwait(false);

            // Assert
            chargeSender.Verify(x => x.PublishChargeCreatedAsync(It.IsAny<ChargeCommandAcceptedEvent>()), Times.Once);
            chargePricesUpdatedSender.Verify(x => x.PublishChargePricesAsync(It.IsAny<ChargeCommandAcceptedEvent>()), Times.Never);
        }
    }
}
