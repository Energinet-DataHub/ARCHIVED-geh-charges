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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Energinet.DataHub.MessageHub.Client.Storage;
using Energinet.DataHub.MessageHub.Model.Model;
using GreenEnergyHub.Charges.Application.ChargeLinks.MessageHub;
using GreenEnergyHub.Charges.Domain.AvailableChargeLinkReceiptData;
using GreenEnergyHub.Charges.Infrastructure.ChargeLinkReceiptBundle;
using GreenEnergyHub.Charges.Infrastructure.ChargeLinkReceiptBundle.Cim;
using GreenEnergyHub.Charges.TestCore.Reflection;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Infrastructure.ChargeLinkReceiptBundle
{
    [UnitTest]
    public class ChargeLinkConfirmationBundleCreatorTests
    {
        [Theory]
        [InlineAutoDomainData]
        public async Task CreateAsync_WhenCalled_UsesRepositoryAndSerializer(
            [Frozen] Mock<IAvailableChargeLinkReceiptDataRepository> repository,
            [Frozen] Mock<IChargeLinkReceiptCimSerializer> serializer,
            [Frozen] Mock<IStorageHandler> storageHandler,
            DataBundleRequestDto dataBundleRequestDto,
            List<AvailableChargeLinkReceiptData> availableChargeLinkReceiptData,
            List<Guid> dataAvailableIds,
            Stream stream,
            ChargeLinkConfirmationBundleCreator sut)
        {
            // Arrange
            dataBundleRequestDto.SetPrivateProperty(
                r => r.MessageType,
                ChargeLinkConfirmationDataAvailableNotifier.MessageTypePrefix);

            storageHandler
                .Setup(r => r.GetDataAvailableNotificationIdsAsync(dataBundleRequestDto))
                .ReturnsAsync(dataAvailableIds);

            repository.Setup(
                    r => r.GetAsync(dataAvailableIds))
                .ReturnsAsync(availableChargeLinkReceiptData);

            // Act
            await sut.CreateAsync(dataBundleRequestDto, stream).ConfigureAwait(false);

            // Assert
            serializer.Verify(
                s => s.SerializeToStreamAsync(
                    availableChargeLinkReceiptData,
                    stream,
                    availableChargeLinkReceiptData.First().BusinessReasonCode,
                    availableChargeLinkReceiptData.First().RecipientId,
                    availableChargeLinkReceiptData.First().RecipientRole),
                Times.Once);
        }
    }
}
