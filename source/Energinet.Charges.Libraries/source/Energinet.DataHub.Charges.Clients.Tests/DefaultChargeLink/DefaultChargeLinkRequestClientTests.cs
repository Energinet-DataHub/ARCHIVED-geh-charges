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
using Energinet.DataHub.Charges.Libraries.DefaultChargeLink;
using Energinet.DataHub.Charges.Libraries.Enums;
using Energinet.DataHub.Charges.Libraries.Factories;
using Energinet.DataHub.Charges.Libraries.Models;
using Energinet.DataHub.Charges.Libraries.ServiceBus;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Charges.Clients.CreateDefaultChargeLink.Tests.DefaultChargeLink
{
    [UnitTest]
    public class DefaultChargeLinkClientTests
    {
        private const string ReplyToQueueName = "ReplyToQueue";
        private const string MeteringPointId = "F9A5115D-44EB-4AD4-BC7E-E8E8A0BC425E";
        private const string CorrelationId = "fake_value";

        [Theory]
        [InlineAutoMoqData(MeteringPointId, null!)]
        [InlineAutoMoqData(null!, CorrelationId)]
        public async Task SendAsync_WhenAnyArgumentIsNull_ThrowsException(
            string meteringPointId,
            string correlationId,
            [NotNull] [Frozen] Mock<ServiceBusClient> serviceBusClientMock,
            [NotNull] [Frozen] Mock<IServiceBusRequestSenderFactory> serviceBusRequestSenderFactoryMock)
        {
            // Arrange
            serviceBusClientMock.Setup(x => x.DisposeAsync()).Returns(default(ValueTask));
            var createDefaultChargeLinksDto = meteringPointId != null ? new CreateDefaultChargeLinksDto(meteringPointId) : null;
            await using var sut = new DefaultChargeLinkClient(
                serviceBusClientMock.Object, serviceBusRequestSenderFactoryMock.Object, ReplyToQueueName);

            // Act + Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut
                .CreateDefaultChargeLinksRequestAsync(createDefaultChargeLinksDto!, correlationId))
                .ConfigureAwait(false);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task CreateDefaultChargeLinksRequestAsync_WhenInputIsValid_SendsMessage(
            [NotNull] [Frozen] Mock<ServiceBusClient> serviceBusClientMock,
            [NotNull] [Frozen] Mock<IServiceBusRequestSenderFactory> serviceBusRequestSenderFactoryMock,
            [NotNull] [Frozen] Mock<IServiceBusRequestSender> serviceBusRequestSenderMock)
        {
            // Arrange
            const string createLinkRequestQueueName = "create-link-request";
            var serviceBusSenderMock = new Mock<ServiceBusSender>();

            serviceBusClientMock.Setup(
                x => x.CreateSender(createLinkRequestQueueName))
                .Returns(serviceBusSenderMock.Object);
            serviceBusClientMock.Setup(
                x => x.DisposeAsync()).Returns(default(ValueTask));

            serviceBusRequestSenderFactoryMock.Setup(x => x
                    .Create(serviceBusClientMock.Object, ReplyToQueueName))
                .Returns(serviceBusRequestSenderMock.Object);

            await using var sut = new DefaultChargeLinkClient(
                serviceBusClientMock.Object, serviceBusRequestSenderFactoryMock.Object, ReplyToQueueName);

            var createDefaultChargeLinksDto = new CreateDefaultChargeLinksDto(MeteringPointId);

            // Act
            await sut.CreateDefaultChargeLinksRequestAsync(
                createDefaultChargeLinksDto, CorrelationId).ConfigureAwait(false);

            // Assert
            serviceBusRequestSenderMock.Verify(
                x => x.SendRequestAsync(
                    It.IsAny<byte[]>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Once);
        }

        [Theory]
        [InlineAutoMoqData(MeteringPointId, null!, ReplyToQueueName)]
        [InlineAutoMoqData(null!, CorrelationId, ReplyToQueueName)]
        [InlineAutoMoqData(MeteringPointId, CorrelationId, null!)]
        public async Task CreateDefaultChargeLinksSucceededRespondAsync_WhenAnyArgumentIsNull_ThrowsException(
            string meteringPointId,
            string correlationId,
            string replyQueue,
            [NotNull] [Frozen] Mock<ServiceBusClient> serviceBusClientMock,
            [NotNull] [Frozen] Mock<IServiceBusRequestSenderFactory> serviceBusRequestSenderFactoryMock)
        {
            // Arrange
            serviceBusClientMock.Setup(x => x.DisposeAsync()).Returns(default(ValueTask));
            var createDefaultChargeLinksSucceededDto = meteringPointId != null ?
                new CreateDefaultChargeLinksSucceededDto(meteringPointId, true) : null;
            await using var sut = new DefaultChargeLinkClient(
                serviceBusClientMock.Object, serviceBusRequestSenderFactoryMock.Object, replyQueue);

            // Act + Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut
                    .CreateDefaultChargeLinksSucceededReplyAsync(
                        createDefaultChargeLinksSucceededDto!,
                        correlationId,
                        replyQueue))
                .ConfigureAwait(false);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task CreateDefaultChargeLinksSucceededRespondAsync_WhenInputIsValid_SendsMessage(
            [NotNull] [Frozen] Mock<ServiceBusClient> serviceBusClientMock,
            [NotNull] [Frozen] Mock<IServiceBusRequestSenderFactory> serviceBusRequestSenderFactoryMock,
            [NotNull] [Frozen] Mock<IServiceBusRequestSender> serviceBusRequestSenderMock)
        {
            // Arrange
            const string respondQueueName = "create-link-respond";
            var serviceBusSenderMock = new Mock<ServiceBusSender>();

            serviceBusClientMock.Setup(
                x => x.CreateSender(respondQueueName)).Returns(serviceBusSenderMock.Object);
            serviceBusClientMock.Setup(
                x => x.DisposeAsync()).Returns(default(ValueTask));

            serviceBusRequestSenderFactoryMock.Setup(x => x
                    .Create(serviceBusClientMock.Object, ReplyToQueueName))
                .Returns(serviceBusRequestSenderMock.Object);

            await using var sut = new DefaultChargeLinkClient(
                serviceBusClientMock.Object, serviceBusRequestSenderFactoryMock.Object, ReplyToQueueName);

            var createDefaultChargeLinksSucceededDto =
                new CreateDefaultChargeLinksSucceededDto(MeteringPointId, true);

            // Act
            await sut.CreateDefaultChargeLinksSucceededReplyAsync(
                createDefaultChargeLinksSucceededDto,
                CorrelationId,
                respondQueueName).ConfigureAwait(false);

            // Assert
            serviceBusRequestSenderMock.Verify(
                x => x.SendRequestAsync(
                    It.IsAny<byte[]>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Once);
        }

        [Theory]
        [InlineAutoMoqData(MeteringPointId, null!, ReplyToQueueName)]
        [InlineAutoMoqData(null!, CorrelationId, ReplyToQueueName)]
        [InlineAutoMoqData(MeteringPointId, CorrelationId, null!)]
        public async Task CreateDefaultChargeLinksFailedRespondAsync_WhenAnyArgumentIsNull_ThrowsException(
            string meteringPointId,
            string correlationId,
            string replyQueue,
            [NotNull] [Frozen] Mock<ServiceBusClient> serviceBusClientMock,
            [NotNull] [Frozen] Mock<IServiceBusRequestSenderFactory> serviceBusRequestSenderFactoryMock)
        {
            // Arrange
            serviceBusClientMock.Setup(x => x.DisposeAsync()).Returns(default(ValueTask));
            var createDefaultChargeLinksFailedDto = meteringPointId != null ?
                new CreateDefaultChargeLinksFailedDto(meteringPointId, ErrorCode.MeteringPointUnknown) : null;
            await using var sut = new DefaultChargeLinkClient(
                serviceBusClientMock.Object, serviceBusRequestSenderFactoryMock.Object, replyQueue);

            // Act + Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut
                    .CreateDefaultChargeLinksFailedReplyAsync(
                        createDefaultChargeLinksFailedDto!,
                        correlationId,
                        replyQueue))
                .ConfigureAwait(false);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task CreateDefaultChargeLinksFailedRespondAsync_WhenInputIsValid_SendsMessage(
            [NotNull] [Frozen] Mock<ServiceBusClient> serviceBusClientMock,
            [NotNull] [Frozen] Mock<IServiceBusRequestSenderFactory> serviceBusRequestSenderFactoryMock,
            [NotNull] [Frozen] Mock<IServiceBusRequestSender> serviceBusRequestSenderMock)
        {
            // Arrange
            const string respondQueueName = "create-link-respond";
            var serviceBusSenderMock = new Mock<ServiceBusSender>();

            serviceBusClientMock.Setup(
                x => x.CreateSender(respondQueueName)).Returns(serviceBusSenderMock.Object);
            serviceBusClientMock.Setup(
                x => x.DisposeAsync()).Returns(default(ValueTask));

            serviceBusRequestSenderFactoryMock.Setup(x => x
                    .Create(serviceBusClientMock.Object, ReplyToQueueName))
                .Returns(serviceBusRequestSenderMock.Object);

            await using var sut = new DefaultChargeLinkClient(
                serviceBusClientMock.Object, serviceBusRequestSenderFactoryMock.Object, ReplyToQueueName);

            var createDefaultChargeLinksFailedDto =
                new CreateDefaultChargeLinksFailedDto(MeteringPointId, ErrorCode.MeteringPointUnknown);

            // Act
            await sut.CreateDefaultChargeLinksFailedReplyAsync(
                createDefaultChargeLinksFailedDto,
                CorrelationId,
                respondQueueName).ConfigureAwait(false);

            // Assert
            serviceBusRequestSenderMock.Verify(
                x => x.SendRequestAsync(
                    It.IsAny<byte[]>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Once);
        }
    }
}
