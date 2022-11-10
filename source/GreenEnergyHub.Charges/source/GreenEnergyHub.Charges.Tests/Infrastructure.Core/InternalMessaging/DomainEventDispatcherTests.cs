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
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using GreenEnergyHub.Charges.Domain.Dtos.Events;
using GreenEnergyHub.Charges.Infrastructure.Core.InternalMessaging;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Factories;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Core.InternalMessaging
{
    [UnitTest]
    public class DomainEventDispatcherTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task DispatchAsync_WhenValidDomainEvent_ThenDispatcherCalled(
            ChargePriceOperationsAcceptedEvent domainEvent,
            [Frozen] Mock<IJsonSerializer> jsonSerializer,
            [Frozen] Mock<IServiceBusDispatcher> serviceBusDispatcher,
            [Frozen] Mock<IServiceBusMessageFactory> serviceBusMessageFactory,
            DomainEventDispatcher sut)
        {
            // Arrange
            // Act
            await sut.DispatchAsync(domainEvent);

            // Assert
            jsonSerializer.Verify(j => j.Serialize(domainEvent), Times.Once);
            serviceBusMessageFactory.Verify(
                j => j.CreateInternalMessage(
                    It.IsAny<string>(),
                    nameof(ChargePriceOperationsAcceptedEvent)),
                Times.Once);
            serviceBusDispatcher.Verify(
                s => s.DispatchAsync(It.IsAny<ServiceBusMessage>()),
                Times.Once);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task DispatchAsync_WhenDomainEventIsNull_ThenThrowsException(
            [Frozen] Mock<IJsonSerializer> jsonSerializer,
            [Frozen] Mock<IServiceBusDispatcher> serviceBusDispatcher,
            [Frozen] Mock<IServiceBusMessageFactory> serviceBusMessageFactory,
            DomainEventDispatcher sut)
        {
            // Arrange
            ChargePriceOperationsAcceptedEvent domainEvent = null!;

            // Act
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.DispatchAsync(domainEvent));

            // Assert
            jsonSerializer.Verify(j => j.Serialize(domainEvent), Times.Never);
            serviceBusMessageFactory.Verify(
                j => j.CreateInternalMessage(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
            serviceBusDispatcher.Verify(
                s => s.DispatchAsync(It.IsAny<ServiceBusMessage>()),
                Times.Never);
        }
    }
}
