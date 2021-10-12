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

using System.Threading.Tasks;
using FluentAssertions;
using Google.Protobuf;
using GreenEnergyHub.Charges.Contracts;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.DataHub.Charges.Libraries.DefaultChargeLink;
using GreenEnergyHub.DataHub.Charges.Libraries.Enums;
using GreenEnergyHub.DataHub.Charges.Libraries.Models;
using Xunit;
using Xunit.Categories;
using ErrorCode = GreenEnergyHub.DataHub.Charges.Libraries.Enums.ErrorCode;

namespace GreenEnergyHub.DataHub.Charges.Clients.CreateDefaultChargeLink.Tests.DefaultChargeLink
{
    [UnitTest]
    public class DefaultChargeLinkReplyDeserializerTests
    {
        private ErrorCode _errorCodeTestResult;
        private string _knownMeteringPointIdTestResult;
        private string _unknownMeteringPointIdTestResult;
        private bool _didCreateChargeLinksTestResult;

        [Theory]
        [InlineAutoMoqData(MessageType.RequestSucceeded, "knownMeteringPointId1234")]
        public async Task DeserializeMessageAsync_Test_OnSuccess(MessageType messageType, string meteringPointId)
        {
            // Arrange
            var createDefaultChargeLinksFailed = new CreateDefaultChargeLinksFailed
            {
                MeteringPointId = meteringPointId,
                ErrorCode = CreateDefaultChargeLinksFailed.Types.ErrorCode.EcMeteringPointUnknown,
            };

            var data = createDefaultChargeLinksFailed.ToByteArray();

            var target = new DefaultChargeLinkReplyReader(HandleSuccess, HandleFailure);

            // Act
            await target.ReadAsync(data, messageType).ConfigureAwait(false);

            // Assert
            target.Should().NotBeNull();
            _knownMeteringPointIdTestResult.Should().Be(meteringPointId);
            _didCreateChargeLinksTestResult.Should().BeTrue();
        }

        [Theory]
        [InlineAutoMoqData(MessageType.RequestFailed, "unknownMeteringPointId9876")]
        public async Task DeserializeMessageAsync_Test_OnFailure(MessageType messageType, string meteringPointId)
        {
            // Arrange
            var createDefaultChargeLinksFailed = new CreateDefaultChargeLinksFailed
                {
                    MeteringPointId = meteringPointId,
                    ErrorCode = CreateDefaultChargeLinksFailed.Types.ErrorCode.EcMeteringPointUnknown,
                };

            var data = createDefaultChargeLinksFailed.ToByteArray();

            var target = new DefaultChargeLinkReplyReader(HandleSuccess, HandleFailure);

            // Act
            await target.ReadAsync(data, messageType).ConfigureAwait(false);

            // Assert
            target.Should().NotBeNull();
            _unknownMeteringPointIdTestResult.Should().Be(meteringPointId);
            _errorCodeTestResult.Should().Be(ErrorCode.MeteringPointUnknown);
        }

        private async Task HandleFailure(CreateDefaultChargeLinksFailedDto createDefaultChargeLinksFailed)
        {
            var (meteringPointId, errorCode) = createDefaultChargeLinksFailed;
            _unknownMeteringPointIdTestResult = meteringPointId;
            _errorCodeTestResult = errorCode;

            await Task.CompletedTask.ConfigureAwait(false);
        }

        private async Task HandleSuccess(CreateDefaultChargeLinksSucceededDto createDefaultChargeLinksSucceeded)
        {
            var (meteringPointId, didCreateChargeLinks) = createDefaultChargeLinksSucceeded;
            _knownMeteringPointIdTestResult = meteringPointId;
            _didCreateChargeLinksTestResult = didCreateChargeLinks;

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
