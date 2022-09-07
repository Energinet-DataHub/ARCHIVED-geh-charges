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
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.BundleSpecification.Charges;
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim.Bundles.ChargeReceipt;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.TestFiles;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.BundleSpecification.Charges
{
    [UnitTest]
    public class ChargePriceRejectionBundleSpecificationTests
    {
        private const string MaxLengthId = "1_______10________20________3012345";

        [Theory]
        [InlineAutoMoqData(0)]
        [InlineAutoMoqData(10)]
        [InlineAutoMoqData(100)]
        [InlineAutoMoqData(1000)]
        public async Task GetMessageWeight_WhenCalled_ReturnedWeightIsHigherThanSerializedStream(
            int noOfReasons,
            ChargeReceiptCimSerializer serializer,
            ChargePriceRejectionBundleSpecification sut)
        {
            // Arrange
            var availableData = GetRejection(noOfReasons);

            var stream = new MemoryStream();
            await serializer.SerializeToStreamAsync(
                new List<AvailableChargeReceiptData> { availableData },
                stream,
                BusinessReasonCode.UpdateChargeInformation,
                "senderId",
                MarketParticipantRole.MeteringPointAdministrator,
                MaxLengthId,
                MarketParticipantRole.GridAccessProvider);

            // Act
            var actual = sut.GetMessageWeight(availableData);

            // Assert
            var streamWeightInKilobytes = (int)stream.Length / 1000; // It is kilobyte, but uses 1000 instead of 1024
            actual.Should().BeGreaterOrEqualTo(streamWeightInKilobytes);
        }

        [Fact]
        public void SizeOfMaximumDocumentWithoutReasons_ShouldNotExceedDefinedWeight()
        {
            // Arrange
            var confirmationMessageWeightInBytes = (long)ChargePriceRejectionBundleSpecification.RejectionWeight * 1000;

            // Act
            var xmlSizeInBytes = new FileInfo(FilesForCalculatingBundleSize.WorstCaseChargeReceipt).Length;

            // Assert
            xmlSizeInBytes.Should().BeLessOrEqualTo(confirmationMessageWeightInBytes);
        }

        private static AvailableChargeReceiptData GetRejection(int noOfReasons)
        {
            return new AvailableChargeReceiptData(
                "senderId",
                MarketParticipantRole.MeteringPointAdministrator,
                MaxLengthId,
                MarketParticipantRole.GridAccessProvider,
                BusinessReasonCode.UpdateChargeInformation,
                SystemClock.Instance.GetCurrentInstant(),
                Guid.Empty,
                ReceiptStatus.Rejected,
                MaxLengthId,
                DocumentType.RejectRequestChangeOfPriceList,
                0,
                Guid.NewGuid(),
                AvailableReceiptValidationErrorGenerator.CreateReasons(noOfReasons));
        }
    }
}
