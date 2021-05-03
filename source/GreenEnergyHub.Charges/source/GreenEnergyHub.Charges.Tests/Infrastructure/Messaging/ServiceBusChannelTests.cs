﻿// Copyright 2020 Energinet DataHub A/S
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

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Azure.Messaging.ServiceBus;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using GreenEnergyHub.TestHelpers;
using GreenEnergyHub.TestHelpers.Traits;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Moq;
using Xunit;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Messaging
{
    /// <summary>
    /// Tests focusing on the Service Bus implementation of the Messaging Channel
    /// </summary>
    [Trait(TraitNames.Category, TraitValues.UnitTest)]
    public class ServiceBusChannelTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task WriteAsync_WhenNoCorrectionId_SendsMessageWithoutCorrelationId(
            [NotNull] [Frozen] Mock<ICorrelationContext> correlationContext,
            [NotNull] [Frozen] Mock<MockableServiceBusSender> serviceBusSender,
            [NotNull] byte[] content)
        {
            // Arrange
            ServiceBusMessage? receivedMessage = null;
            serviceBusSender
                .Setup(s => s.SendMessageAsync(
                    It.IsAny<ServiceBusMessage>(),
                    It.IsAny<CancellationToken>()))
                .Callback<ServiceBusMessage, CancellationToken>((message, token) => receivedMessage = message);

            correlationContext.Setup(c => c.CorrelationId).Returns(string.Empty);

            var sut = new TestableServiceBusChannel(serviceBusSender.Object, correlationContext.Object);

            // Act
            await sut.WriteToAsync(content).ConfigureAwait(false);

            // Assert
            Assert.NotNull(receivedMessage);
            Assert.Empty(receivedMessage!.CorrelationId);
            Assert.True(content.SequenceEqual(receivedMessage.Body.ToArray()));
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task WriteAsync_WhenCorrectionId_SendsMessageWithCorrelationId(
            [NotNull] [Frozen] Mock<ICorrelationContext> correlationContext,
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
                .Callback<ServiceBusMessage, CancellationToken>((message, token) => receivedMessage = message);

            correlationContext.Setup(c => c.CorrelationId).Returns(correlationId);

            var sut = new TestableServiceBusChannel(serviceBusSender.Object, correlationContext.Object);

            // Act
            await sut.WriteToAsync(content).ConfigureAwait(false);

            // Assert
            Assert.NotNull(receivedMessage);
            Assert.Equal(correlationId, receivedMessage!.CorrelationId);
            Assert.True(content.SequenceEqual(receivedMessage.Body.ToArray()));
        }
    }
}
