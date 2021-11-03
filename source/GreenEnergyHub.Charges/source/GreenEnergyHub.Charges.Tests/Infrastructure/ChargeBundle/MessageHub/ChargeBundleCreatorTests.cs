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
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Energinet.DataHub.MessageHub.Client.Model;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.AvailableChargeData;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.ChargeBundle.Cim;
using GreenEnergyHub.Charges.Infrastructure.ChargeBundle.MessageHub;
using GreenEnergyHub.Charges.Tests.Builders;
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
        public async Task CreateAsync_WhenCalled_UsesRepositoryAndSerializerAndReturnSuccess(
            [Frozen] Mock<IAvailableChargeDataRepository> respository,
            [Frozen] Mock<IChargeCimSerializer> serializer,
            DataBundleRequestDto dataBundleRequestDto,
            Stream stream,
            ChargeBundleCreator sut)
        {
            // Arrange
            var availableChargeData1 = new AvailableChargeDataBuilder().Build();
            var availableChargeData2 = new AvailableChargeDataBuilder().Build();
            var availableChargeData = new List<AvailableChargeData> { availableChargeData1, availableChargeData2 };

            respository.Setup(
                    r => r.GetAvailableChargeDataAsync(
                        dataBundleRequestDto.DataAvailableNotificationIds))
                .Returns(Task.FromResult(availableChargeData));

            // Act
            var ranSuccessfully = await sut.CreateSuccessfullyAsync(dataBundleRequestDto, stream).ConfigureAwait(false);

            // Assert
            ranSuccessfully.Should().BeTrue();
            serializer.Verify(
                s => s.SerializeToStreamAsync(
                    availableChargeData,
                    stream),
                Times.Once);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task CreateAsync_WhenCalledWithDifferentBusinessReasonCodes_ShouldNotSerializeAndReturnFailed(
            [Frozen] Mock<IAvailableChargeDataRepository> respository,
            [Frozen] Mock<IChargeCimSerializer> serializer,
            DataBundleRequestDto dataBundleRequestDto,
            Stream stream,
            ChargeBundleCreator sut)
        {
            // Arrange
            var availableChargeData1 = new AvailableChargeDataBuilder()
                .WithBusinessReasonCode(BusinessReasonCode.UpdateChargeInformation)
                .Build();
            var availableChargeData2 = new AvailableChargeDataBuilder()
                .WithBusinessReasonCode(BusinessReasonCode.UpdateMasterDataSettlement)
                .Build();
            var availableChargeData = new List<AvailableChargeData> { availableChargeData1, availableChargeData2 };

            respository.Setup(
                    r => r.GetAvailableChargeDataAsync(
                        dataBundleRequestDto.DataAvailableNotificationIds))
                .Returns(Task.FromResult(availableChargeData));

            // Act
            var ranSuccessfully = await sut.CreateSuccessfullyAsync(dataBundleRequestDto, stream).ConfigureAwait(false);

            // Assert
            ranSuccessfully.Should().BeFalse();
            serializer.Verify(
                s => s.SerializeToStreamAsync(
                    availableChargeData,
                    stream),
                Times.Never);
        }
    }
}
