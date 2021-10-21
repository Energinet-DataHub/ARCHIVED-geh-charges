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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Energinet.DataHub.MessageHub.Client.Model;
using GreenEnergyHub.Charges.Domain.AvailableChargeLinksData;
using GreenEnergyHub.Charges.Infrastructure.ChargeLinkBundle.Cim;
using GreenEnergyHub.Charges.Infrastructure.ChargeLinkBundle.MessageHub;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.ChargeLinkBundle.MessageHub
{
    [UnitTest]
    public class ChargeLinkBundleCreatorTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task CreateAsync_WhenCalled_UsesRepositoryAndSerializer(
            [NotNull] [Frozen] Mock<IAvailableChargeLinksDataRepository> respository,
            [NotNull] [Frozen] Mock<IChargeLinkCimSerializer> serializer,
            DataBundleRequestDto dataBundleRequestDto,
            List<AvailableChargeLinksData> availableChargeLinksData,
            Stream stream,
            ChargeLinkBundleCreator sut)
        {
            // Arrange
            respository.Setup(
                    r => r.GetAvailableChargeLinksDataAsync(
                        dataBundleRequestDto.DataAvailableNotificationIds))
                .Returns(Task.FromResult(availableChargeLinksData));

            // Act
            await sut.CreateAsync(dataBundleRequestDto, stream).ConfigureAwait(false);

            // Assert
            serializer.Verify(
                s => s.SerializeToStreamAsync(
                    availableChargeLinksData,
                    stream),
                Times.Once);
        }
    }
}
