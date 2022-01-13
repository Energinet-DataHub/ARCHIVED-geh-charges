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
using System.Text;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.BundleSpecification.ChargeLinks;
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim;
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim.Bundles.ChargeLinkReceipt;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeLinksReceiptData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.Tests.Builders;
using GreenEnergyHub.Charges.Tests.TestFiles;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.MessageHub.BundleSpecification.ChargeLinks
{
    [UnitTest]
    public class ChargeLinksRejectionBundleSpecificationTests
    {
        private const string DataHubSenderId = "5790001330552";
        private const string MaxLengthId = "00000000000000000000000000000000000";
        private const int MaxTextLengthInTest = 10000;

        [Theory]
        [InlineAutoMoqData(0)]
        [InlineAutoMoqData(10)]
        [InlineAutoMoqData(100)]
        [InlineAutoMoqData(1000)]
        public async Task GetMessageWeight_WhenCalled_ReturnedWeightIsHigherThanSerializedStream(
            int noOfReasons,
            HubSenderMarketParticipantBuilder hubSenderBuilder,
            [Frozen] Mock<IMarketParticipantRepository> marketParticipantRepository,
            [Frozen] Mock<ICimIdProvider> cimIDProvider,
            ChargeLinksReceiptCimSerializer serializer,
            ChargeLinksRejectionBundleSpecification sut)
        {
            // Arrange
            var availableData = GetRejection(noOfReasons);

            marketParticipantRepository
                .Setup(r => r.GetHubSenderAsync())
                .ReturnsAsync(hubSenderBuilder.Build());

            cimIDProvider.Setup(
                    c => c.GetUniqueId())
                .Returns(MaxLengthId);

            var stream = new MemoryStream();
            await serializer.SerializeToStreamAsync(
                new List<AvailableChargeLinksReceiptData>() { availableData },
                stream,
                BusinessReasonCode.UpdateChargeInformation,
                DataHubSenderId,
                MarketParticipantRole.MeteringPointAdministrator,
                MaxLengthId,
                MarketParticipantRole.GridAccessProvider);

            // Act
            var actual = sut.GetMessageWeight(availableData);

            // Assert
            actual.Should().BeGreaterOrEqualTo((int)stream.Length / 1000); // It is kilobyte, but uses 1000 instead of 1024
        }

        [Fact]
        public void SizeOfMaximumDocumentWithuotReasons_ShouldNotExceedDefinedWeight()
        {
            // Arrange
            var confirmationMessageWeightInBytes = (long)ChargeLinksRejectionBundleSpecification.RejectionWeight * 1000;

            // Act
            var xmlSizeInBytes = new FileInfo(FilesForCalculatingBundleSize.WorstCaseChargeReceipt).Length;

            // Assert
            xmlSizeInBytes.Should().BeLessOrEqualTo(confirmationMessageWeightInBytes);
        }

        private AvailableChargeLinksReceiptData GetRejection(int noOfReasons)
        {
            return new AvailableChargeLinksReceiptData(
                DataHubSenderId,
                MarketParticipantRole.MeteringPointAdministrator,
                MaxLengthId,
                MarketParticipantRole.EnergySupplier,
                BusinessReasonCode.UpdateChargeInformation,
                SystemClock.Instance.GetCurrentInstant(),
                Guid.NewGuid(),
                ReceiptStatus.Rejected,
                string.Empty,
                MaxLengthId,
                GetReasons(noOfReasons));
        }

        private List<AvailableReceiptValidationError> GetReasons(int noOfReasons)
        {
            var reasons = new List<AvailableReceiptValidationError>();

            for (var i = 0; i < noOfReasons; i++)
            {
                reasons.Add(GetReason());
            }

            return reasons;
        }

        private AvailableReceiptValidationError GetReason()
        {
            var text = CreateStringOfRandomLength();
            return new AvailableReceiptValidationError(ReasonCode.D01, text);
        }

        private static string CreateStringOfRandomLength()
        {
            var builder = new StringBuilder();
            var randomizer = new Random();
            var length = randomizer.Next(0, MaxTextLengthInTest);
            for (var i = 0; i < length; i++)
            {
                builder.Append('0');
            }

            return builder.ToString();
        }
    }
}
