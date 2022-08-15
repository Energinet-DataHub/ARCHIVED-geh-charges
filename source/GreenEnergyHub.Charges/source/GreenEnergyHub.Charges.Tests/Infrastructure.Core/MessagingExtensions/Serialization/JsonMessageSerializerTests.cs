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
using Energinet.DataHub.Core.JsonSerialization;
using Energinet.DataHub.Core.Messaging.Transport;
using GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Serialization;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.Core.MessagingExtensions.Serialization
{
    [UnitTest]
    public class JsonMessageSerializerTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task ToBytesAsync_WhenCalledWIthOutboundMessage_CallsNeededComponents(
            [NotNull] [Frozen] Mock<IJsonOutboundMapperFactory> mapperFactory,
            [NotNull] [Frozen] Mock<IJsonSerializer> jsonSerializer,
            [NotNull] Mock<IJsonOutboundMapper> mapper,
            [NotNull] Mock<IOutboundMessage> message,
            [NotNull] string stringResult,
            [NotNull] JsonMessageSerializer sut)
        {
            // Arrange
            mapperFactory
                .Setup(f => f.GetMapper(message.Object))
                .Returns(mapper.Object);

            jsonSerializer
                .Setup(s => s.Serialize(It.IsAny<object>()))
                .Returns(stringResult);

            // Act
            var result = await sut.ToBytesAsync(message.Object).ConfigureAwait(false);

            // Assert
            Assert.NotNull(result);

            mapperFactory.Verify(m => m.GetMapper(message.Object));
            mapperFactory.VerifyAll();
            mapper.Verify(m => m.Convert(message.Object));
            mapperFactory.VerifyAll();
            jsonSerializer.Verify(s => s.Serialize(It.IsAny<object>()));
            jsonSerializer.VerifyAll();
        }
    }
}
