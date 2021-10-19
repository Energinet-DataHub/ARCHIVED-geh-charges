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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Charges.Libraries.DefaultChargeLinkMessages;
using Energinet.DataHub.Charges.Libraries.Factories;
using Energinet.DataHub.Charges.Libraries.Models;
using Energinet.DataHub.Charges.Libraries.ServiceBus;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Charges.Clients.CreateDefaultChargeLink.Tests.DefaultChargeLinkMessages
{
    [UnitTest]
    public class DefaultChargeLinkMessagesRequestClientTests
    {
        private const string ReplyToQueueName = "ReplyToQueue";
        private const string MeteringPointId = "F9A5115D-44EB-4AD4-BC7E-E8E8A0BC425E";
        private const string CorrelationId = "fake_value";

        [Theory]
        [InlineAutoMoqData(MeteringPointId, null)]
        [InlineAutoMoqData(null, CorrelationId)]
        [InlineAutoMoqData(null, null)]
        public async Task SendAsync_NullArgument_ThrowsException(
            string meteringPointId,
            string correlationId,
            [NotNull] [Frozen] Mock<ServiceBusClient> serviceBusClientMock,
            [NotNull] [Frozen] Mock<IServiceBusRequestSenderFactory> serviceBusRequestSenderFactoryMock)
        {
            // Arrange
            serviceBusClientMock.Setup(x => x.DisposeAsync()).Returns(default(ValueTask));
            var createDefaultChargeLinkMessagesDto = meteringPointId != null ? new CreateDefaultChargeLinkMessagesDto(meteringPointId) : null;
            await using var target = new DefaultChargeLinkMessagesRequestClient(
                serviceBusClientMock.Object, serviceBusRequestSenderFactoryMock.Object, ReplyToQueueName);

            // Act + Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => target
                .CreateDefaultChargeLinkMessagesRequestAsync(createDefaultChargeLinkMessagesDto!, correlationId))
                .ConfigureAwait(false);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task CreateDefaultChargeLinkMessagesRequestAsync_ValidInput_SendsMessage(
            [NotNull] [Frozen] Mock<ServiceBusClient> serviceBusClientMock,
            [NotNull] [Frozen] Mock<IServiceBusRequestSenderFactory> serviceBusRequestSenderFactoryMock,
            [NotNull] [Frozen] Mock<IServiceBusRequestSender> serviceBusRequestSenderMock)
        {
            // Arrange
            const string createLinkMessagesRequestQueueName = "create-link-messages-request";
            var serviceBusSenderMock = new Mock<ServiceBusSender>();

            serviceBusClientMock.Setup(x => x.CreateSender(createLinkMessagesRequestQueueName)).Returns(serviceBusSenderMock.Object);
            serviceBusClientMock.Setup(x => x.DisposeAsync()).Returns(default(ValueTask));

            serviceBusRequestSenderFactoryMock.Setup(x => x.Create(serviceBusClientMock.Object, ReplyToQueueName))
                .Returns(serviceBusRequestSenderMock.Object);

            await using var sut = new DefaultChargeLinkMessagesRequestClient(
                serviceBusClientMock.Object, serviceBusRequestSenderFactoryMock.Object, ReplyToQueueName);

            var defaultChargeLinkMessagesDto = new CreateDefaultChargeLinkMessagesDto(MeteringPointId);

            // Act
            await sut.CreateDefaultChargeLinkMessagesRequestAsync(defaultChargeLinkMessagesDto, CorrelationId).ConfigureAwait(false);

            // Assert
            serviceBusRequestSenderMock.Verify(
                x => x.SendRequestAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
