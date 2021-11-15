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
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Contracts;
using GreenEnergyHub.Charges.Infrastructure.CreateDefaultChargeLinkReplier;
using GreenEnergyHub.Charges.Infrastructure.ServiceBusReplySenderProvider;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.ChargeLinks.CreateDefaultChargeLinkReplier
{
    [UnitTest]
    public class CreateDefaultChargeLinkReplierTests
    {
        private const string ReplyToQueueName = "ReplyToQueue";
        private const string MeteringPointId = "F9A5115D-44EB-4AD4-BC7E-E8E8A0BC425E";
        private const string CorrelationId = "fake_value";

        [Theory]
        [InlineAutoMoqData(MeteringPointId, null!, ReplyToQueueName)]
        [InlineAutoMoqData(null!, CorrelationId, ReplyToQueueName)]
        [InlineAutoMoqData(MeteringPointId, CorrelationId, null!)]
        public async Task ReplyWithSucceededAsync_WhenArgumentIsNull_ThrowsArgumentNullException(
            string meteringPointId,
            string correlationId,
            string replyQueue,
            [NotNull] [Frozen] Mock<IServiceBusReplySenderProvider> serviceBusReplySenderProviderMock)
        {
            // Arrange
            var sut = new CreateDefaultChargeLinksReplier(serviceBusReplySenderProviderMock.Object);

            // Act + Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut
                    .ReplyWithSucceededAsync(
                        meteringPointId!, false, replyQueue!, correlationId!))
                .ConfigureAwait(false);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task ReplyWithSucceededAsync_WhenInputIsValid_SendsMessage(
            [NotNull] [Frozen] Mock<IServiceBusReplySenderProvider> serviceBusReplySenderProviderMock,
            [NotNull] [Frozen] Mock<IServiceBusReplySender> serviceBusRequestSenderMock,
            [NotNull] [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContextMock,
            string anyMeteringPointId,
            string replyTo,
            bool anyDidCreateDefaultCharges)
        {
            // Arrange
            messageMetaDataContextMock.Setup(x => x.ReplyTo).Returns(replyTo);

            serviceBusReplySenderProviderMock.Setup(x => x
                    .GetInstance(replyTo))
                .Returns(serviceBusRequestSenderMock.Object);

            var sut = new CreateDefaultChargeLinksReplier(serviceBusReplySenderProviderMock.Object);

            // Act
            await sut.ReplyWithSucceededAsync(
                anyMeteringPointId,
                anyDidCreateDefaultCharges,
                messageMetaDataContextMock.Object.ReplyTo,
                CorrelationId).ConfigureAwait(false);

            // Assert
            serviceBusRequestSenderMock.Verify(x => x.SendReplyAsync(It.IsAny<byte[]>(), CorrelationId), Times.Once);
        }

        [Theory]
        [InlineAutoMoqData(MeteringPointId, null!, ReplyToQueueName)]
        [InlineAutoMoqData(null!, CorrelationId, ReplyToQueueName)]
        [InlineAutoMoqData(MeteringPointId, CorrelationId, null!)]
        public async Task ReplyWithFailedAsync_WhenAnyArgumentIsNull_ThrowsArgumentNullException(
            string meteringPointId,
            string correlationId,
            string replyQueue,
            ErrorCode errorCode,
            [NotNull] [Frozen] Mock<IServiceBusReplySenderProvider> serviceBusRequestSenderProviderMock)
        {
            // Arrange
            var sut = new CreateDefaultChargeLinksReplier(serviceBusRequestSenderProviderMock.Object);

            // Act + Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut
                    .ReplyWithFailedAsync(
                        meteringPointId!, errorCode, replyQueue!, correlationId!))
                .ConfigureAwait(false);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task ReplyWithFailedAsync_WhenInputIsValid_SendsMessage(
            [NotNull] [Frozen] Mock<IServiceBusReplySenderProvider> serviceBusReplySenderProviderMock,
            [NotNull] [Frozen] Mock<IServiceBusReplySender> serviceBusReplySenderMock,
            string meteringPointId,
            ErrorCode errorCode)
        {
            // Arrange
            const string replyQueueName = "create-link-reply";

            serviceBusReplySenderProviderMock.Setup(x => x.
                    GetInstance(replyQueueName)).Returns(serviceBusReplySenderMock.Object);

            var sut = new CreateDefaultChargeLinksReplier(serviceBusReplySenderProviderMock.Object);

            // Act
            await sut.ReplyWithFailedAsync(
                meteringPointId,
                errorCode,
                replyQueueName,
                CorrelationId).ConfigureAwait(false);

            // Assert
            serviceBusReplySenderMock.Verify(x => x.SendReplyAsync(It.IsAny<byte[]>(), CorrelationId), Times.Once);
        }
    }
}
