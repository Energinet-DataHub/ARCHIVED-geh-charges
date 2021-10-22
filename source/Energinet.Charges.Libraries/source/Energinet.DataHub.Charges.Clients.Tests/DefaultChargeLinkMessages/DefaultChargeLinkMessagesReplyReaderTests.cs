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
using Energinet.Charges.Contracts;
using Energinet.DataHub.Charges.Libraries.DefaultChargeLinkMessages;
using Energinet.DataHub.Charges.Libraries.Enums;
using Energinet.DataHub.Charges.Libraries.Models;
using FluentAssertions;
using Google.Protobuf;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Xunit;
using Xunit.Categories;
using ErrorCode = Energinet.DataHub.Charges.Libraries.Enums.ErrorCode;

namespace Energinet.DataHub.Charges.Clients.CreateDefaultChargeLink.Tests.DefaultChargeLinkMessages
{
    [UnitTest]
    public class DefaultChargeLinkMessagesReplyReaderTests
    {
        private ErrorCode _errorCodeTestResult;
        private string? _knownMeteringPointIdTestResult;
        private string? _unknownMeteringPointIdTestResult;

        [Theory]
        [InlineAutoMoqData(MessageType.CreateDefaultLinksMessagesSucceeded, "knownMeteringPointId1234")]
        [InlineAutoMoqData(MessageType.CreateDefaultLinksMessagesSucceeded, "knownMeteringPointId5678")]
        public async Task DefaultChargeLinkMessagesCreationSucceeded(
            MessageType messageType, string meteringPointId)
        {
            // Arrange
            var createDefaultChargeLinkMessagesSucceeded = new CreateDefaultChargeLinkMessagesSucceeded
            {
                MeteringPointId = meteringPointId,
            };

            var data = createDefaultChargeLinkMessagesSucceeded.ToByteArray();

            var target = new DefaultChargeLinkMessagesReplyReader(HandleSuccess, HandleFailure);

            // Act
            await target.ReadAsync(data, messageType).ConfigureAwait(false);

            // Assert
            target.Should().NotBeNull();
            _knownMeteringPointIdTestResult.Should().Be(meteringPointId);
        }

        [Theory]
        [InlineAutoMoqData(MessageType.CreateDefaultLinksMessagesFailed, "unknownMeteringPointId9876")]
        public async Task DefaultChargeLinkMessagesCreationFailed(
            MessageType messageType, string meteringPointId)
        {
            // Arrange
            var createDefaultChargeLinkMessagesFailed = new CreateDefaultChargeLinkMessagesFailed
                {
                    MeteringPointId = meteringPointId,
                    ErrorCode = CreateDefaultChargeLinkMessagesFailed.Types.ErrorCode.EcMeteringPointUnknown,
                };

            var data = createDefaultChargeLinkMessagesFailed.ToByteArray();

            var target = new DefaultChargeLinkMessagesReplyReader(HandleSuccess, HandleFailure);

            // Act
            await target.ReadAsync(data, messageType).ConfigureAwait(false);

            // Assert
            target.Should().NotBeNull();
            _unknownMeteringPointIdTestResult.Should().Be(meteringPointId);
            _errorCodeTestResult.Should().Be(ErrorCode.MeteringPointUnknown);
        }

        private async Task HandleFailure(CreateDefaultChargeLinkMessagesFailedDto createDefaultChargeLinkMessagesFailed)
        {
            (_unknownMeteringPointIdTestResult, _errorCodeTestResult) = createDefaultChargeLinkMessagesFailed;

            await Task.CompletedTask.ConfigureAwait(false);
        }

        private async Task HandleSuccess(CreateDefaultChargeLinkMessagesSucceededDto createDefaultChargeLinkMessagesSucceeded)
        {
            _knownMeteringPointIdTestResult = createDefaultChargeLinkMessagesSucceeded.MeteringPointId;

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
