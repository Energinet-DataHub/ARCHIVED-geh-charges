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
using System.IO;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Energinet.DataHub.MessageHub.Client.Model;
using Energinet.DataHub.MessageHub.Client.Peek;
using Energinet.DataHub.MessageHub.Client.Storage;
using GreenEnergyHub.Charges.Application.SeedWork.SyncRequest;
using GreenEnergyHub.Charges.Infrastructure.ChargeLinkBundle.MessageHub;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.ChargeLinkBundle.MessageHub
{
    [UnitTest]
    public class ChargeLinkBundleReplierTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task ReplyAsync_SendsAResponseAsync(
            [Frozen] Mock<IStorageHandler> storageHandlerMock,
            [Frozen] Mock<IDataBundleResponseSender> sender,
            ChargeLinkBundleReplier sut,
            Mock<MemoryStream> anyBundleStreamMock,
            DataBundleRequestDto request,
            ISyncRequestMetadata anyMetadata,
            Uri anyUri)
        {
            storageHandlerMock
                .Setup(handler => handler.AddStreamToStorageAsync(anyBundleStreamMock.Object, request))
                .ReturnsAsync(anyUri);

            await sut.ReplyAsync(anyBundleStreamMock.Object, request, anyMetadata);

            sender.Verify(s =>
                s.SendAsync(It.IsAny<DataBundleResponseDto>(), It.IsAny<DataBundleRequestDto>(), It.IsAny<string>()));
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task ReplyAsync_SendsWithSessionIdFromMetadataAsync(
            [Frozen] Mock<IStorageHandler> storageHandlerMock,
            [Frozen] Mock<IDataBundleResponseSender> sender,
            ChargeLinkBundleReplier sut,
            Mock<MemoryStream> anyBundleStreamMock,
            DataBundleRequestDto request,
            ISyncRequestMetadata anyMetadata,
            Uri anyUri)
        {
            // Arrange
            var expectedSessionId = anyMetadata.SessionId;
            storageHandlerMock.Setup(handler => handler.AddStreamToStorageAsync(anyBundleStreamMock.Object, request))
                .ReturnsAsync(anyUri);

            // Act
            await sut.ReplyAsync(anyBundleStreamMock.Object, request, anyMetadata);

            // Assert
            sender.Verify(s =>
                s.SendAsync(It.IsAny<DataBundleResponseDto>(), It.IsAny<DataBundleRequestDto>(), expectedSessionId));
        }
    }
}
