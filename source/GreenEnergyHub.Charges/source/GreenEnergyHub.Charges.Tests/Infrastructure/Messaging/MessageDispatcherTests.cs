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
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Energinet.DataHub.Core.Messaging.Transport;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Messaging
{
    [UnitTest]
    public class MessageDispatcherTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task DispatchAsync_WhenCalled_UsesSerializer(
            [Frozen] [NotNull] Mock<MessageSerializer> serializer,
            [NotNull] IOutboundMessage message,
            [NotNull] byte[] serializedBytes,
            [NotNull] MessageDispatcher<IOutboundMessage> sut)
        {
            // Arrange
            IOutboundMessage? serializedMessage = null;
            serializer.Setup(
                    s => s.ToBytesAsync(
                        It.IsAny<IOutboundMessage>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(serializedBytes))
                .Callback<IOutboundMessage, CancellationToken>((m, _) => serializedMessage = m);

            // Act
            await sut.DispatchAsync(message).ConfigureAwait(false);

            // Assert
            Assert.Equal(message, serializedMessage);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task DispatchAsync_WhenMessageIsNull_ThrowsArgumentNullException(
            [NotNull] MessageDispatcher<IOutboundMessage> sut)
        {
            IOutboundMessage? message = null;

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.DispatchAsync(message!))
                .ConfigureAwait(false);
        }
    }
}
