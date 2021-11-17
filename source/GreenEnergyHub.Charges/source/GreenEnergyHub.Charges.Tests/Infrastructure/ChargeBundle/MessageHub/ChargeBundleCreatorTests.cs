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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Energinet.DataHub.MessageHub.Model.Model;
using GreenEnergyHub.Charges.Domain.AvailableChargeData;
using GreenEnergyHub.Charges.Infrastructure.ChargeBundle.Cim;
using GreenEnergyHub.Charges.Infrastructure.ChargeBundle.MessageHub;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.ChargeBundle.MessageHub
{
    [UnitTest]
    public class ChargeBundleCreatorTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task CreateAsync_WhenCalled_UsesRepositoryAndSerializer(
            [Frozen] Mock<IAvailableChargeDataRepository> respository,
            [Frozen] Mock<IChargeCimSerializer> serializer,
            DataBundleRequestDto dataBundleRequestDto,
            List<AvailableChargeData> availableChargeData,
            Stream stream,
            ChargeBundleCreator sut)
        {
            // Arrange
            respository.Setup(
                    r => r.GetAvailableChargeDataAsync(
                        dataBundleRequestDto.DataAvailableNotificationIds))
                .Returns(Task.FromResult(availableChargeData));

            // Act
            await sut.CreateAsync(dataBundleRequestDto, stream).ConfigureAwait(false);

            // Assert
            serializer.Verify(
                s => s.SerializeToStreamAsync(
                    availableChargeData,
                    stream,
                    availableChargeData.First().BusinessReasonCode,
                    availableChargeData.First().RecipientId,
                    availableChargeData.First().RecipientRole),
                Times.Once);
        }
    }
}
