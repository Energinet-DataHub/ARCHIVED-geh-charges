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
using AutoFixture.Xunit2;
using Energinet.DataHub.Core.Messaging.Protobuf;
using Energinet.DataHub.Core.Messaging.Transport;
using Google.Protobuf;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Serialization;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Core.MessagingExtensions.Serialization
{
    [UnitTest]
    public class ProtobufDeserializerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public void Ctor_WhenCalledWithNullMapper_ThrowsException(
            ProtobufParser<IMessage> parser)
        {
            // Arrange
            ProtobufInboundMapperFactory? factory = null;

            // Assert
            Assert.Throws<ArgumentNullException>(() => new ProtobufDeserializer<IMessage>(factory!, parser));
        }

        [Theory]
        [InlineAutoDomainData]
        public void Ctor_WhenCalledWithNullParser_ThrowsException(
            ProtobufInboundMapperFactory factory)
        {
            // Arrange
            ProtobufParser<IMessage>? parser = null;

            // Assert
            Assert.Throws<ArgumentNullException>(() => new ProtobufDeserializer<IMessage>(factory, parser!));
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task FromBytesAsync_WhenCalled_DeserializeMessage(
            [Frozen] Mock<IServiceProvider> serviceProvider,
            [Frozen] Mock<ProtobufParser<IMessage>> parser,
            Mock<ProtobufInboundMapper> mapper,
            byte[] data,
            IMessage message,
            IInboundMessage expected,
            ProtobufDeserializer<IMessage> sut)
        {
            // Arrange
            parser.Setup(
                    p => p.Parse(data))
                .Returns(message);

            serviceProvider.Setup(
                    s => s.GetService(
                        It.IsAny<Type>()))
                .Returns(mapper.Object);

            mapper.Setup(
                    m => m.Convert(message))
                .Returns(expected);

            // Act
            var actual = await sut.FromBytesAsync(data).ConfigureAwait(false);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(expected, actual);
        }
    }
}
