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

using System.IO;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Energinet.DataHub.MessageHub.Client.Model;
using GreenEnergyHub.Charges.Application.ChargeLinks.MessageHub;
using GreenEnergyHub.Charges.Application.ChargeLinks.MessageHub.Infrastructure;
using GreenEnergyHub.Charges.Application.SeedWork.SyncRequest;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.ChargeLinks.MessageHub
{
    [UnitTest]
    public class ChargeLinkCreatedBundleSenderTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task SendAsync_RepliesWithCreatedBundle(
            [Frozen] Mock<IChargeLinkCreatedBundleCreator> creatorMock,
            [Frozen] Mock<IChargeLinkCreatedBundleReplier> replierMock,
            ChargeLinkCreatedBundleSender sut,
            DataBundleRequestDto anyRequest,
            ISyncRequestMetadata anyMetadata)
        {
            // Arrange
            MemoryStream actualStream = null!;
            creatorMock
                .Setup(creator => creator.CreateAsync(anyRequest, It.IsAny<MemoryStream>()))
                .Callback((DataBundleRequestDto _, Stream s) => actualStream = (MemoryStream)s);

            // Act
            await sut.SendAsync(anyRequest, anyMetadata);

            // Assert => creator was invoked with the expected stream
            creatorMock.Verify(creator => creator.CreateAsync(anyRequest, actualStream));

            // Assert => replier was invoked with the expected stream
            replierMock.Verify(replier => replier.ReplyAsync(actualStream, anyRequest, anyMetadata));
        }
    }
}
