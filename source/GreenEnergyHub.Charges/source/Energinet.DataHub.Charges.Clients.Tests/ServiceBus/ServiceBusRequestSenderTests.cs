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

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Azure.Messaging.ServiceBus;
using Energinet.Charges.Contracts;
using Energinet.DataHub.Charges.Clients.ServiceBus;
using Google.Protobuf;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Charges.Clients.CreateDefaultChargeLink.Tests.ServiceBus
{
    [UnitTest]
    public class ServiceBusRequestSenderTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task SendsMessage(
            [NotNull] [Frozen] Mock<ServiceBusClient> serviceBusClientMock,
            string meteringPointId,
            string requestQueueName,
            string replyToQueueName,
            string correlationId)
        {
            // Arrange
            var serviceBusSenderMock = new Mock<ServiceBusSender>();
            serviceBusClientMock.Setup(x => x.CreateSender(requestQueueName))
                .Returns(serviceBusSenderMock.Object);
            serviceBusClientMock.Setup(x => x.DisposeAsync()).Returns(default(ValueTask));

            serviceBusSenderMock.Setup(x => x.SendMessageAsync(It.IsAny<ServiceBusMessage>(), default));

            var sut = new ServiceBusRequestSender(serviceBusSenderMock.Object, replyToQueueName);

            var defaultChargeLinks = new CreateDefaultChargeLinks { MeteringPointId = meteringPointId };

            // Act
            await sut.SendRequestAsync(defaultChargeLinks.ToByteArray(), correlationId).ConfigureAwait(false);

            // Act // Assert
            serviceBusSenderMock.Verify(x => x.SendMessageAsync(It.IsAny<ServiceBusMessage>(), default), Times.Once);
        }
    }
}
