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
using Energinet.DataHub.MessageHub.Model.Model;
using FluentAssertions;
using GreenEnergyHub.Charges.Application.ChargeLinks.MessageHub;
using GreenEnergyHub.Charges.Domain.AvailableChargeLinksData;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.Tests.Application.ChargeLinks.MessageHub
{
    [UnitTest]
    public class ChargeLinkNotificationFactoryTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Create_WhenGivenAvailableLinks_CreatesNotifications(
            IReadOnlyList<AvailableChargeLinksData> availableData,
            ChargeLinkNotificationFactory sut)
        {
            // Act
            var actualNotificationList = sut.Create(availableData);

            // Assert
            actualNotificationList.Should().HaveSameCount(availableData);
            for (var i = 0; i < actualNotificationList.Count; i++)
            {
                actualNotificationList[i].Uuid.Should().Be(availableData[i].AvailableDataReferenceId);
                actualNotificationList[i].Recipient.Value.Should().Be(availableData[i].RecipientId);
                actualNotificationList[i].MessageType.Value.Should().Be(
                    ChargeLinkNotificationFactory.MessageTypePrefix + "_" +
                    availableData[i].BusinessReasonCode);
                actualNotificationList[i].Origin.Should().Be(DomainOrigin.Charges);
                actualNotificationList[i].SupportsBundling.Should().BeTrue();
                actualNotificationList[i].RelativeWeight.Should()
                    .Be(ChargeLinkNotificationFactory.MessageWeight);
            }
        }

        [Fact]
        public void SizeOfMaximumDocument_ShouldNotExceedDefinedWeight()
        {
            // Arrange
            var testFilePath = "TestFiles/SingleChargeLinkCimSerializerWorstCase.blob";
            var chargeMessageWeightInBytes = (long)ChargeLinkNotificationFactory.MessageWeight * 1000;

            // Act
            var xmlSizeInBytes = new System.IO.FileInfo(testFilePath).Length;

            // Assert
            xmlSizeInBytes.Should().BeLessOrEqualTo(chargeMessageWeightInBytes);
        }
    }
}
