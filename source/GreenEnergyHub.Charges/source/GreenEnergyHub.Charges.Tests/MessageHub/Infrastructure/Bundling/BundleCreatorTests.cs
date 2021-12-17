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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Energinet.DataHub.MessageHub.Client.Storage;
using Energinet.DataHub.MessageHub.Model.Model;
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Bundling;
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Infrastructure.Bundling
{
    [UnitTest]
    public class BundleCreatorTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task CreateAsync_WhenCalled_UsesRepositoryAndSerializer(
            [Frozen] Mock<IAvailableDataRepository<AvailableDataBase>> repository,
            [Frozen] Mock<ICimSerializer<AvailableDataBase>> serializer,
            DataBundleRequestDto dataBundleRequestDto,
            List<AvailableDataBase> availableData,
            List<Guid> dataAvailableIds,
            [Frozen] Mock<IStorageHandler> storageHandler,
            Stream stream,
            BundleCreator<AvailableDataBase> sut)
        {
            // Arrange
            storageHandler
                .Setup(r => r.GetDataAvailableNotificationIdsAsync(dataBundleRequestDto))
                .ReturnsAsync(dataAvailableIds);

            repository.Setup(
                    r => r.GetAsync(dataAvailableIds))
                .ReturnsAsync(availableData);

            // Act
            await sut.CreateAsync(dataBundleRequestDto, stream).ConfigureAwait(false);

            // Assert
            serializer.Verify(
                s => s.SerializeToStreamAsync(
                    availableData,
                    stream,
                    availableData.First().BusinessReasonCode,
                    availableData.First().RecipientId,
                    availableData.First().RecipientRole),
                Times.Once);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task CreateAsync_WhenCalledWithRequestYieldingNoAvailableData_ThrowsUnknownDataAvailableNotificationIdsException(
            [Frozen] Mock<IAvailableDataRepository<AvailableDataBase>> repository,
            DataBundleRequestDto dataBundleRequestDto,
            List<Guid> dataAvailableIds,
            [Frozen] Mock<IStorageHandler> storageHandler,
            Stream stream,
            BundleCreator<AvailableDataBase> sut)
        {
            // Arrange
            storageHandler
                .Setup(r => r.GetDataAvailableNotificationIdsAsync(dataBundleRequestDto))
                .ReturnsAsync(dataAvailableIds);

            repository.Setup(
                    r => r.GetAsync(dataAvailableIds))
                .ReturnsAsync(new List<AvailableDataBase>());

            // Act
            await Assert.ThrowsAsync<UnknownDataAvailableNotificationIdsException>(
                () => sut.CreateAsync(dataBundleRequestDto, stream));
        }
    }
}
