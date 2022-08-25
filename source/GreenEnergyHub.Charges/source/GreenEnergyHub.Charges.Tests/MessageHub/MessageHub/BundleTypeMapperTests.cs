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
using System.ComponentModel;
using Energinet.DataHub.MessageHub.Model.Model;
using FluentAssertions;
using GreenEnergyHub.Charges.MessageHub.MessageHub;
using GreenEnergyHub.Charges.MessageHub.Models.AvailableData;
using Xunit;
using Xunit.Categories;
using Enum = System.Enum;

namespace GreenEnergyHub.Charges.Tests.MessageHub.MessageHub
{
    [UnitTest]
    public class BundleTypeMapperTests
    {
        [Theory]
        [MemberData(nameof(GetBundleTypes))]
        public void Map_WhenGivenEnum_MapsToString(BundleType bundleType)
        {
            var actual = BundleTypeMapper.Map(bundleType);

            actual.Should().NotBeNullOrWhiteSpace();
        }

        [Theory]
        [InlineData("ChargeDataAvailable", BundleType.ChargeDataAvailable)]
        [InlineData("ChargeConfirmationDataAvailable", BundleType.ChargeConfirmationDataAvailable)]
        [InlineData("ChargeRejectionDataAvailable", BundleType.ChargeRejectionDataAvailable)]
        [InlineData("ChargeLinkDataAvailable", BundleType.ChargeLinkDataAvailable)]
        [InlineData("ChargeLinkConfirmationDataAvailable", BundleType.ChargeLinkConfirmationDataAvailable)]
        [InlineData("ChargeLinkRejectionDataAvailable", BundleType.ChargeLinkRejectionDataAvailable)]
        public void Map_WhenGivenKnownString_MapsToBundleType(
            string input,
            BundleType expected)
        {
            var actual = BundleTypeMapper.Map(input);

            actual.Should().Be(expected);
        }

        [Fact]
        public void Map_WhenGivenDataBundleRequestDtoWithKnownMessageType_ReturnsBundleType()
        {
            // Arrange
            var request = GetDataBundleRequestDtoWithMessageType("ChargeDataAvailable_SomeBusinessReasonCode");

            // Act
            var actual = BundleTypeMapper.Map(request);

            actual.Should().Be(BundleType.ChargeDataAvailable);
        }

        [Fact]
        public void Map_WhenGivenUnknownString_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => BundleTypeMapper.Map(string.Empty));
        }

        [Fact]
        public void Map_WhenGivenUnknownBundleType_ThrowsException()
        {
            Assert.Throws<InvalidEnumArgumentException>(() => BundleTypeMapper.Map(0));
        }

        [Fact]
        public void Map_WhenGivenDataBundleRequestDtoWithWronglyFormattedMessageType_ThrowsException()
        {
            var request = GetDataBundleRequestDtoWithMessageType(string.Empty);

            Assert.Throws<ArgumentException>(() => BundleTypeMapper.Map(request));
        }

        public static IEnumerable<object[]> GetBundleTypes()
        {
            foreach (var bundleType in Enum.GetValues(typeof(BundleType)))
            {
                yield return new object[] { bundleType };
            }
        }

        private DataBundleRequestDto GetDataBundleRequestDtoWithMessageType(string messageType)
        {
            return new DataBundleRequestDto(
                    RequestId: Guid.NewGuid(),
                    DataAvailableNotificationReferenceId: Guid.NewGuid().ToString(),
                    IdempotencyId: Guid.NewGuid().ToString(),
                    new MessageTypeDto(messageType),
                    ResponseFormat.Xml,
                    ResponseVersion: 1.0);
        }
    }
}
