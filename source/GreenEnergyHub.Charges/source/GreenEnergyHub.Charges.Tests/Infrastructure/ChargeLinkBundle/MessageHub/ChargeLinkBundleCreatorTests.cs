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
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Energinet.DataHub.MessageHub.Client.Model;
using GreenEnergyHub.Charges.Domain.AvailableChargeLinksData;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
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
            [Frozen] Mock<IAvailableChargeLinksDataRepository> respository,
            [Frozen] Mock<IChargeLinkCimSerializer> serializer,
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
                    stream,
                    // Due to the nature of the interface to the MessageHub and the use of MessageType in that
                    // BusinessReasonCode, RecipientId, RecipientRole and ReceiptStatus will always be the same value
                    // on all records in the list. We need to check that its equal to the first row.
                    availableChargeLinksData.First().BusinessReasonCode,
                    availableChargeLinksData.First().RecipientId,
                    availableChargeLinksData.First().RecipientRole),
                Times.Once);
        }
    }
}
