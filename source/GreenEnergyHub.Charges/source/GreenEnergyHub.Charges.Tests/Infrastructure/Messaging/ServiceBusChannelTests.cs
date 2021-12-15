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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Azure.Messaging.ServiceBus;
using FluentAssertions;
using GreenEnergyHub.Charges.Infrastructure.Core.Correlation;
using GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions;
using GreenEnergyHub.TestHelpers;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Messaging
{
    /// <summary>
    /// Tests focusing on the Service Bus implementation of the Messaging Channel
    /// </summary>
    [UnitTest]
    public class ServiceBusChannelTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task WriteAsync_WhenNoCorrelationId_SendsMessageWithoutCorrelationId(
            [NotNull] [Frozen] Mock<ICorrelationContext> correlationContext,
            [NotNull] [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            [NotNull] [Frozen] Mock<MockableServiceBusSender> serviceBusSender,
            [NotNull] byte[] content)
        {
            // Arrange
            ServiceBusMessage? receivedMessage = null;
            serviceBusSender
                .Setup(s => s.SendMessageAsync(
                    It.IsAny<ServiceBusMessage>(),
                    It.IsAny<CancellationToken>()))
                .Callback<ServiceBusMessage, CancellationToken>((message, _) => receivedMessage = message);
            var genericSender = new ServiceBusSender<TestOutboundMessage>(serviceBusSender.Object);

            correlationContext.Setup(c => c.Id).Returns(string.Empty);

            var sut = new TestableServiceBusChannel<TestOutboundMessage>(
                genericSender,
                correlationContext.Object,
                messageMetaDataContext.Object);

            // Act
            await sut.WriteToAsync(content).ConfigureAwait(false);

            // Assert
            receivedMessage!.Should().NotBeNull();
            receivedMessage!.CorrelationId.Should().BeEquivalentTo(string.Empty);
            content.SequenceEqual(receivedMessage.Body.ToArray()).Should().BeTrue();
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task WriteAsync_WhenCorrelationId_SendsMessageWithCorrelationId(
            [NotNull] [Frozen] Mock<ICorrelationContext> correlationContext,
            [NotNull] [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            [NotNull] [Frozen] Mock<MockableServiceBusSender> serviceBusSender,
            [NotNull] byte[] content,
            [NotNull] string correlationId)
        {
            // Arrange
            ServiceBusMessage? receivedMessage = null;
            serviceBusSender
                .Setup(s => s.SendMessageAsync(
                    It.IsAny<ServiceBusMessage>(),
                    It.IsAny<CancellationToken>()))
                .Callback<ServiceBusMessage, CancellationToken>((message, _) => receivedMessage = message);

            correlationContext.Setup(c => c.Id).Returns(correlationId);
            var genericSender = new ServiceBusSender<TestOutboundMessage>(serviceBusSender.Object);

            var sut = new TestableServiceBusChannel<TestOutboundMessage>(
                genericSender, correlationContext.Object, messageMetaDataContext.Object);

            // Act
            await sut.WriteToAsync(content).ConfigureAwait(false);

            // Assert
            receivedMessage!.Should().NotBeNull();
            correlationId.Should().BeEquivalentTo(receivedMessage!.CorrelationId);
            content.SequenceEqual(receivedMessage.Body.ToArray()).Should().BeTrue();
        }

        [Fact(Skip = "Manually run test to see the class can communicate with the service bus")]
        public async Task WriteAsync_WhenManuallyRun_EndsUpOnServiceBus()
        {
            // Arrange
            var correlationContext = new CorrelationContext();
            var messageMetaDataContext = new MessageMetaDataContext(SystemClock.Instance);
            correlationContext.SetId(Guid.NewGuid().ToString().Replace("-", string.Empty));

            var connectionString = "<your service bus connection string>";
            await using ServiceBusClient client = new(connectionString);

            var topic = "<your service bus topic>";
            var sender = client.CreateSender(topic);
            var genericSender = new ServiceBusSender<TestOutboundMessage>(sender);

            var messageText = "Hello world";
            var message = Encoding.UTF8.GetBytes(messageText);

            var sut = new TestableServiceBusChannel<TestOutboundMessage>(
                genericSender, correlationContext, messageMetaDataContext);

            // Act
            await sut.WriteToAsync(message).ConfigureAwait(false);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task WriteAsync_WhenNoReplyTo_SendsMessageWithoutReplyTo(
            [NotNull] [Frozen] Mock<ICorrelationContext> correlationContext,
            [NotNull] [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            [NotNull] [Frozen] Mock<MockableServiceBusSender> serviceBusSender,
            [NotNull] string correlationId,
            [NotNull] byte[] content)
        {
            // Arrange
            ServiceBusMessage? receivedMessage = null;
            serviceBusSender
                .Setup(s => s.SendMessageAsync(
                    It.IsAny<ServiceBusMessage>(),
                    It.IsAny<CancellationToken>()))
                .Callback<ServiceBusMessage, CancellationToken>((message, _) => receivedMessage = message);
            var genericSender = new ServiceBusSender<TestOutboundMessage>(serviceBusSender.Object);

            correlationContext.Setup(c => c.Id).Returns(correlationId);
            messageMetaDataContext.Setup(c => c.ReplyTo).Returns(string.Empty);

            var sut = new TestableServiceBusChannel<TestOutboundMessage>(
                genericSender,
                correlationContext.Object,
                messageMetaDataContext.Object);

            // Act
            await sut.WriteToAsync(content).ConfigureAwait(false);

            // Assert
            receivedMessage!.Should().NotBeNull();
            receivedMessage!.ApplicationProperties.TryGetValue("ReplyTo", out var replyTo);
            replyTo.Should().BeNull();
            content.SequenceEqual(receivedMessage.Body.ToArray()).Should().BeTrue();
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task WriteAsync_WhenReplyTo_SendsMessageReplyTo(
            [NotNull] [Frozen] Mock<ICorrelationContext> correlationContext,
            [NotNull] [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            [NotNull] [Frozen] Mock<MockableServiceBusSender> serviceBusSender,
            [NotNull] string replyToExpected,
            [NotNull] string correlationId,
            [NotNull] byte[] content)
        {
            // Arrange
            ServiceBusMessage? receivedMessage = null;
            serviceBusSender
                .Setup(s => s.SendMessageAsync(
                    It.IsAny<ServiceBusMessage>(),
                    It.IsAny<CancellationToken>()))
                .Callback<ServiceBusMessage, CancellationToken>((message, _) => receivedMessage = message);
            var genericSender = new ServiceBusSender<TestOutboundMessage>(serviceBusSender.Object);

            correlationContext.Setup(c => c.Id).Returns(correlationId);
            messageMetaDataContext.Setup(c => c.IsReplyToSet()).Returns(true);
            messageMetaDataContext.Setup(c => c.ReplyTo).Returns(replyToExpected);

            var sut = new TestableServiceBusChannel<TestOutboundMessage>(
                genericSender,
                correlationContext.Object,
                messageMetaDataContext.Object);

            // Act
            await sut.WriteToAsync(content).ConfigureAwait(false);

            // Assert
            receivedMessage!.Should().NotBeNull();
            var replyTo = receivedMessage!.ApplicationProperties["ReplyTo"];
            replyToExpected.Should().BeEquivalentTo(replyTo.ToString());
            content.SequenceEqual(receivedMessage.Body.ToArray()).Should().BeTrue();
        }
    }
}
