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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Azure.Messaging.ServiceBus;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Factories;
using GreenEnergyHub.Charges.TestCore.TestHelpers;
using GreenEnergyHub.TestHelpers;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Core.MessagingExtensions
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
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            [Frozen] Mock<MockableServiceBusSender> serviceBusSender,
            byte[] content)
        {
            // Arrange
            ServiceBusMessage? receivedMessage = null;
            serviceBusSender
                .Setup(s => s.SendMessageAsync(
                    It.IsAny<ServiceBusMessage>(),
                    It.IsAny<CancellationToken>()))
                .Callback<ServiceBusMessage, CancellationToken>((message, _) => receivedMessage = message);
            var genericSender = new ServiceBusSender<TestOutboundMessage>(serviceBusSender.Object);
            var correlationContext = CorrelationContextGenerator.Create(string.Empty);

            var serviceBusMessageFactory = new ServiceBusMessageFactory(
            correlationContext,
            messageMetaDataContext.Object);

            var sut = new TestableServiceBusChannel<TestOutboundMessage>(
                genericSender, serviceBusMessageFactory);

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
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            [Frozen] Mock<MockableServiceBusSender> serviceBusSender,
            byte[] content,
            string correlationId)
        {
            // Arrange
            ServiceBusMessage? receivedMessage = null;
            serviceBusSender
                .Setup(s => s.SendMessageAsync(
                    It.IsAny<ServiceBusMessage>(),
                    It.IsAny<CancellationToken>()))
                .Callback<ServiceBusMessage, CancellationToken>((message, _) => receivedMessage = message);

            var correlationContext = CorrelationContextGenerator.Create(correlationId);
            var genericSender = new ServiceBusSender<TestOutboundMessage>(serviceBusSender.Object);

            var serviceBusMessageFactory = new ServiceBusMessageFactory(
                correlationContext,
                messageMetaDataContext.Object);

            var sut = new TestableServiceBusChannel<TestOutboundMessage>(
                genericSender, serviceBusMessageFactory);

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
            var correlationContext = CorrelationContextGenerator.Create();
            var messageMetaDataContext = new MessageMetaDataContext(SystemClock.Instance);

            var serviceBusMessageFactory = new ServiceBusMessageFactory(
                correlationContext,
                messageMetaDataContext);

            var connectionString = "<your service bus connection string>";
            await using ServiceBusClient client = new(connectionString);

            var topic = "<your service bus topic>";
            var sender = client.CreateSender(topic);
            var genericSender = new ServiceBusSender<TestOutboundMessage>(sender);

            var messageText = "Hello world";
            var message = Encoding.UTF8.GetBytes(messageText);

            var sut = new TestableServiceBusChannel<TestOutboundMessage>(
                genericSender, serviceBusMessageFactory);

            // Act
            await sut.WriteToAsync(message).ConfigureAwait(false);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task WriteAsync_WhenNoReplyTo_SendsMessageWithoutReplyTo(
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            [Frozen] Mock<MockableServiceBusSender> serviceBusSender,
            byte[] content)
        {
            // Arrange
            ServiceBusMessage? receivedMessage = null;
            serviceBusSender
                .Setup(s => s.SendMessageAsync(
                    It.IsAny<ServiceBusMessage>(),
                    It.IsAny<CancellationToken>()))
                .Callback<ServiceBusMessage, CancellationToken>((message, _) => receivedMessage = message);
            var genericSender = new ServiceBusSender<TestOutboundMessage>(serviceBusSender.Object);
            var correlationContext = CorrelationContextGenerator.Create();

            messageMetaDataContext.Setup(c => c.ReplyTo).Returns(string.Empty);
            var serviceBusMessageFactory = new ServiceBusMessageFactory(
                correlationContext,
                messageMetaDataContext.Object);

            var sut = new TestableServiceBusChannel<TestOutboundMessage>(
                genericSender,
                serviceBusMessageFactory);

            // Act
            await sut.WriteToAsync(content).ConfigureAwait(false);

            // Assert
            receivedMessage!.Should().NotBeNull();
            receivedMessage!.ApplicationProperties.TryGetValue("ReplyTo", out var replyTo);
            replyTo.Should().BeNull();
            content.SequenceEqual(receivedMessage.Body.ToArray()).Should().BeTrue();
        }
    }
}
