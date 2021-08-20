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

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Serialization;
using GreenEnergyHub.Charges.TestCore;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Messaging.Transport;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Messaging
{
    [UnitTest]
    public class MessageExtractorTests
    {
        [Fact]
        public void Constructor_WhenDeserializerIsNull_ThrowsArgumentNullException()
        {
            JsonMessageDeserializer<IInboundMessage>? deserializer = null;

            Assert.Throws<ArgumentNullException>(
                () => new MessageExtractor<IInboundMessage>(deserializer!));
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task ExtractAsync_WhenGivenNullArray_ThrowsArgumentNullException(
            [NotNull] MessageExtractor<IInboundMessage> sut)
        {
            byte[]? data = null;

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.ExtractAsync(data!))
                .ConfigureAwait(false);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task ExtractAsync_WhenGivenByteArray_CallsDeserializerAndReturnsResult(
            [Frozen] [NotNull] Mock<JsonMessageDeserializer<IInboundMessage>> deserializer,
            [NotNull] byte[] data,
            [NotNull] IInboundMessage message)
        {
            // Arrange
            byte[]? deserializeBytes = null;
            deserializer.Setup(
                    s => s.FromBytesAsync(
                        It.IsAny<byte[]>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(message))
                .Callback<byte[], CancellationToken>(
                    (calledData, _) => deserializeBytes = calledData);

            var sut = new MessageExtractor<IInboundMessage>(deserializer.Object);

            // Act
            var result = await sut.ExtractAsync(data).ConfigureAwait(false);

            // Assert
            Assert.Equal(deserializeBytes, data);
            Assert.Equal(message, result);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task ExtractAsync_WhenGivenNullStream_ThrowsArgumentNullException(
            [NotNull] MessageExtractor<IInboundMessage> sut)
        {
            Stream? stream = null;

            await Assert.ThrowsAsync<ArgumentNullException>(
                    () => sut.ExtractAsync(stream!))
                .ConfigureAwait(false);
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task ExtractAsync_WhenGivenStream_CallsDeserializerAndReturnsResult(
            [Frozen] [NotNull] Mock<JsonMessageDeserializer<IInboundMessage>> deserializer,
            [NotNull] IInboundMessage message)
        {
            // Arrange
            await using var stream = new MemoryStream();

            var calledDeserializer = false;
            deserializer.Setup(
                    s => s.FromBytesAsync(
                        It.IsAny<byte[]>(),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(message))
                .Callback<byte[], CancellationToken>(
                    (_, _) => calledDeserializer = true);

            var sut = new MessageExtractor<IInboundMessage>(deserializer.Object);

            // Act
            var result = await sut.ExtractAsync(stream).ConfigureAwait(false);

            // Assert
            Assert.True(calledDeserializer);
            Assert.Equal(message, result);
        }
    }
}
