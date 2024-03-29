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
using System.IO;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Energinet.DataHub.MessageHub.Client.Peek;
using Energinet.DataHub.MessageHub.Client.Storage;
using Energinet.DataHub.MessageHub.Model.Model;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Bundling;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Infrastructure.Bundling
{
    [UnitTest]
    public class BundleReplierTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task ReplyAsync_SendsAResponseAsync(
            [Frozen] Mock<IStorageHandler> storageHandlerMock,
            [Frozen] Mock<IDataBundleResponseSender> sender,
            BundleReplier sut,
            Mock<MemoryStream> anyBundleStreamMock,
            DataBundleRequestDto request,
            Uri anyUri)
        {
            storageHandlerMock
                .Setup(handler => handler.AddStreamToStorageAsync(anyBundleStreamMock.Object, request))
                .ReturnsAsync(anyUri);

            await sut.ReplyAsync(anyBundleStreamMock.Object, request);

            sender.Verify(s =>
                s.SendAsync(It.IsAny<DataBundleResponseDto>()));
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task ReplyAsync_SendsWithSessionIdFromMetadataAsync(
            [Frozen] Mock<IStorageHandler> storageHandlerMock,
            [Frozen] Mock<IDataBundleResponseSender> sender,
            [Frozen] Mock<IMessageMetaDataContext> messageMetaDataContext,
            BundleReplier sut,
            string expectedSessionId,
            Mock<MemoryStream> anyBundleStreamMock,
            DataBundleRequestDto request,
            Uri anyUri)
        {
            // Arrange
            messageMetaDataContext.Setup(m => m.SessionId).Returns(expectedSessionId);
            storageHandlerMock.Setup(
                    handler => handler.AddStreamToStorageAsync(anyBundleStreamMock.Object, request))
                .ReturnsAsync(anyUri);

            // Act
            await sut.ReplyAsync(anyBundleStreamMock.Object, request);

            // Assert
            sender.Verify(s =>
                s.SendAsync(It.IsAny<DataBundleResponseDto>()));
        }

        [Theory]
        [InlineAutoMoqData(typeof(UnknownDataAvailableNotificationIdsException), DataBundleResponseErrorReason.DatasetNotFound)]
        [InlineAutoMoqData(typeof(Exception), DataBundleResponseErrorReason.InternalError)]
        public async Task ReplyErrorAsync_WhenExceptionIsUnknownIds_SendsDatasetNotFoundReplyAsync(
            Type exceptionType,
            DataBundleResponseErrorReason expectedReason,
            [Frozen] Mock<IDataBundleResponseSender> sender,
            DataBundleRequestDto request,
            BundleReplier sut)
        {
            // Arrange
            var e = (Exception)Activator.CreateInstance(exceptionType)!;

            DataBundleResponseDto? actualDto = null;
            sender.Setup(
                    s => s.SendAsync(It.IsAny<DataBundleResponseDto>()))
                .Callback((DataBundleResponseDto dto) => actualDto = dto);

            // Act
            await sut.ReplyErrorAsync(e, request);

            // Assert
            sender.Verify(s =>
                s.SendAsync(It.IsAny<DataBundleResponseDto>()));
            actualDto!.Should().NotBeNull();
            actualDto!.ResponseError.Should().NotBeNull();
            actualDto!.ResponseError!.Reason.Should().Be(expectedReason);
        }
    }
}
