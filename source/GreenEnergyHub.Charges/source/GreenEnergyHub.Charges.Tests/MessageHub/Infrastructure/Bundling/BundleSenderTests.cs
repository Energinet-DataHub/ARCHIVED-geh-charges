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
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Energinet.DataHub.MessageHub.Client.Storage;
using Energinet.DataHub.MessageHub.Model.Model;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Bundling;
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksReceiptData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Moq;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.Infrastructure.Bundling
{
    [UnitTest]
    public class BundleSenderTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task SendAsync_RepliesWithCreatedBundle(
            [Frozen] Mock<IBundleCreator> creatorMock,
            [Frozen] Mock<IBundleReplier> replierMock,
            [Frozen] Mock<IBundleCreatorProvider> creatorProviderMock,
            BundleSender sut,
            DataBundleRequestDto anyRequest)
        {
            // Arrange
            MemoryStream actualStream = null!;
            creatorMock
                .Setup(creator => creator.CreateAsync(anyRequest, It.IsAny<MemoryStream>()))
                .Callback((DataBundleRequestDto _, Stream s) => actualStream = (MemoryStream)s);
            creatorProviderMock
                .Setup(provider => provider.Get(anyRequest))
                .Returns(creatorMock.Object);

            // Act
            await sut.SendAsync(anyRequest);

            // Assert => creator was invoked with the expected stream
            creatorMock.Verify(creator => creator.CreateAsync(anyRequest, actualStream));

            // Assert => replier was invoked with the expected stream
            replierMock.Verify(replier => replier.ReplyAsync(actualStream, anyRequest));
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task SendAsync_WhenExceptionIsThrown_RepliesWithError(
            [Frozen] Mock<IBundleCreatorProvider> creatorProviderMock,
            [Frozen] Mock<IBundleReplier> replierMock,
            DataBundleRequestDto anyRequest,
            BundleSender sut)
        {
            // Arrange
            creatorProviderMock.Setup(
                    c => c.Get(anyRequest))
                .Throws<Exception>();

            // Act
            await sut.SendAsync(anyRequest);

            // Assert
            replierMock.Verify(replier => replier.ReplyAsync(It.IsAny<Stream>(), It.IsAny<DataBundleRequestDto>()), Times.Never);
            replierMock.Verify(replier => replier.ReplyErrorAsync(It.IsAny<Exception>(), anyRequest));
        }

        [Theory]
        [InlineAutoMoqData(BusinessReasonCode.Unknown)]
        [InlineAutoMoqData(BusinessReasonCode.UpdateChargeInformation)]
        [InlineAutoMoqData(BusinessReasonCode.UpdateMasterDataSettlement)]
        public void Get_ReturnsValidData_ForAllBundleTypes(
            BusinessReasonCode businessReasonCode,
            IStorageHandler storageHandler)
        {
            // Arrange
            var bundleTypes = (BundleType[])Enum.GetValues(typeof(BundleType));
            var bundleCreators = CreateBundleCreators(bundleTypes, storageHandler);

            // Act
            // Assert
            foreach (var bundleType in bundleTypes)
            {
                var creatorProvider = new BundleCreatorProvider(bundleCreators);
                var messageType = $"{bundleType}_{businessReasonCode}";
                var request = new DataBundleRequestDto(
                    RequestId: Guid.NewGuid(),
                    DataAvailableNotificationReferenceId: Guid.NewGuid().ToString(),
                    IdempotencyId: Guid.NewGuid().ToString(),
                    new MessageTypeDto(messageType),
                    ResponseFormat.Xml,
                    1.0);

                var actual = creatorProvider.Get(request);

                actual.Should().NotBeNull();
            }
        }

        private IList<IBundleCreator> CreateBundleCreators(
            IEnumerable<BundleType> bundleTypes,
            IStorageHandler storageHandler)
        {
            var bundleCreators = new List<IBundleCreator>();

            foreach (var bundleType in bundleTypes)
            {
                switch (bundleType)
                {
                    case BundleType.ChargeDataAvailable:
                        bundleCreators.Add(CreateBundleCreator<AvailableChargeData>(storageHandler));
                        break;
                    case BundleType.ChargeConfirmationDataAvailable:
                        bundleCreators.Add(CreateBundleCreator<AvailableChargeReceiptData>(storageHandler));
                        break;
                    case BundleType.ChargeRejectionDataAvailable:
                        // BundleCreator<AvailableChargeReceiptData> already added
                        break;
                    case BundleType.ChargeLinkDataAvailable:
                        bundleCreators.Add(CreateBundleCreator<AvailableChargeLinksData>(storageHandler));
                        break;
                    case BundleType.ChargeLinkConfirmationDataAvailable:
                        bundleCreators.Add(CreateBundleCreator<AvailableChargeLinksReceiptData>(storageHandler));
                        break;
                    case BundleType.ChargeLinkRejectionDataAvailable:
                        // BundleCreator<AvailableChargeLinksReceiptData> already added
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(bundleType));
                }
            }

            return bundleCreators;
        }

        private IBundleCreator CreateBundleCreator<T>(IStorageHandler storageHandler)
            where T : AvailableDataBase
        {
            var cimSerializer = new Mock<ICimSerializer<T>>().Object;
            var availableDataRepository = new Mock<IAvailableDataRepository<T>>().Object;
            return new BundleCreator<T>(availableDataRepository, cimSerializer, storageHandler);
        }
    }
}
