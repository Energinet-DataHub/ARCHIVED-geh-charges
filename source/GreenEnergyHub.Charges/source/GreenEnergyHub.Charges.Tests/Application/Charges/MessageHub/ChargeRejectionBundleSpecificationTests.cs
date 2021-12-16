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
using System.Text;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using GreenEnergyHub.Charges.Domain.Configuration;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using GreenEnergyHub.Charges.Infrastructure.Core;
using GreenEnergyHub.Charges.Infrastructure.Core.Cim.MarketDocument;
using GreenEnergyHub.Charges.MessageHub.Application.Charges;
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim;
using GreenEnergyHub.Charges.MessageHub.Infrastructure.Cim.Bundles.ChargeReceipt;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableChargeReceiptData;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Reflection;
using GreenEnergyHub.Charges.Tests.TestFiles;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.Charges.MessageHub
{
    [UnitTest]
    public class ChargeRejectionBundleSpecificationTests
    {
        private const string MaxLengthId = "00000000000000000000000000000000000";
        private const int MaxTextLengthInTest = 10000;

        [Theory]
        [InlineAutoMoqData(0)]
        [InlineAutoMoqData(10)]
        [InlineAutoMoqData(100)]
        [InlineAutoMoqData(1000)]
        public async Task GetMessageWeight_WhenCalled_ReturnedWeightIsHigherThanSerializedStream(
            int noOfReasons,
            [Frozen] Mock<IHubSenderConfiguration> hubSenderConfiguration,
            [Frozen] Mock<ICimIdProvider> cimIDProvider,
            ChargeReceiptCimSerializer serializer,
            ChargeRejectionBundleSpecification sut)
        {
            // Arrange
            var availableData = GetRejection(noOfReasons);

            var marketParticipant = new MarketParticipant
            {
                MarketParticipantId = MaxLengthId,
                BusinessProcessRole = MarketParticipantRole.GridAccessProvider,
            };

            hubSenderConfiguration.Setup(
                    c => c.GetSenderMarketParticipant())
                .Returns(marketParticipant);

            cimIDProvider.Setup(
                    c => c.GetUniqueId())
                .Returns(MaxLengthId);

            var stream = new MemoryStream();
            await serializer.SerializeToStreamAsync(
                new List<AvailableChargeReceiptData>() { availableData },
                stream,
                BusinessReasonCode.UpdateChargeInformation,
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
            var confirmationMessageWeightInBytes = (long)ChargeRejectionBundleSpecification.RejectionWeight * 1000;

            // Act
            var xmlSizeInBytes = new System.IO.FileInfo(BundleSize.WorstCaseChargeReceipt).Length;

            // Assert
            xmlSizeInBytes.Should().BeLessOrEqualTo(confirmationMessageWeightInBytes);
        }

        private AvailableChargeReceiptData GetRejection(int noOfReasons)
        {
            return new AvailableChargeReceiptData(
                MaxLengthId,
                MarketParticipantRole.GridAccessProvider,
                BusinessReasonCode.UpdateChargeInformation,
                SystemClock.Instance.GetCurrentInstant(),
                Guid.Empty,
                ReceiptStatus.Rejected,
                MaxLengthId,
                GetReasons(noOfReasons));
        }

        private List<AvailableChargeReceiptDataReasonCode> GetReasons(int noOfReasons)
        {
            var reasons = new List<AvailableChargeReceiptDataReasonCode>();

            for (var i = 0; i < noOfReasons; i++)
            {
                reasons.Add(GetReason());
            }

            return reasons;
        }

        private AvailableChargeReceiptDataReasonCode GetReason()
        {
            // We create a string of random length for the text
            var builder = new StringBuilder();
            var randomizer = new Random();
            var length = randomizer.Next(0, MaxTextLengthInTest);
            for (var i = 0; i < length; i++)
            {
                builder.Append("0");
            }

            return new AvailableChargeReceiptDataReasonCode(
                ReasonCode.IncorrectChargeInformation,
                builder.ToString());
        }
    }
}
