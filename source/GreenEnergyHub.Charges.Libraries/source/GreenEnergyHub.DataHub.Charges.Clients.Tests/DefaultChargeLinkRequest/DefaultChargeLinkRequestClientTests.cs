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
using Azure.Messaging.ServiceBus;
using GreenEnergyHub.DataHub.Charges.Libraries.DefaultChargeLinkRequest;
using GreenEnergyHub.DataHub.Charges.Libraries.Models;
using GreenEnergyHub.PostOffice.Communicator.Factories;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.DataHub.Charges.Clients.CreateDefaultChargeLink.Tests.DefaultChargeLinkRequest
{
    [UnitTest]
    public class DefaultChargeLinkRequestClientTests
    {
        private const string RespondQueue = "RespondQueue";

        [Fact]
        public async Task SendAsync_NullArgument_ThrowsException()
        {
            // Arrange
            var serviceBusClientFactory = new Mock<IServiceBusClientFactory>();
            await using var target = new DefaultChargeLinkRequestClient(serviceBusClientFactory.Object, RespondQueue);

            // Act + Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => target.CreateDefaultChargeLinksRequestAsync(null!)).ConfigureAwait(false);
        }

        [Fact]
        public async Task CreateDefaultChargeLinksRequestAsync_ValidInput_SendsMessage()
        {
            // Arrange
            var serviceBusSenderMock = new Mock<ServiceBusSender>();
            var serviceBusSessionReceiverMock = new Mock<ServiceBusSessionReceiver>();

            await using var mockedServiceBusClient = new MockedServiceBusClient(
                "create-link-request",
                string.Empty,
                serviceBusSenderMock.Object,
                serviceBusSessionReceiverMock.Object);

            var serviceBusClientFactory = new Mock<IServiceBusClientFactory>();
            serviceBusClientFactory.Setup(x => x.Create()).Returns(mockedServiceBusClient);

            await using var target = new DefaultChargeLinkRequestClient(serviceBusClientFactory.Object, RespondQueue);

            var createDefaultChargeLinksDto = new CreateDefaultChargeLinksDto(
                "F9A5115D-44EB-4AD4-BC7E-E8E8A0BC425E",
                "fake_value");

            // Act
            await target.CreateDefaultChargeLinksRequestAsync(createDefaultChargeLinksDto).ConfigureAwait(false);

            // Assert
            serviceBusSenderMock.Verify(x => x.SendMessageAsync(It.IsAny<ServiceBusMessage>(), default), Times.Once);
        }
    }
}
