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
using System.Threading.Tasks;
using Energinet.Charges.Contracts;
using Energinet.DataHub.Charges.Clients.DefaultChargeLink;
using Energinet.DataHub.Charges.Clients.DefaultChargeLink.Models;
using FluentAssertions;
using Google.Protobuf;
using GreenEnergyHub.Charges.Contracts;
using GreenEnergyHub.TestHelpers;
using Xunit;
using Xunit.Categories;
using CreateDefaultChargeLinksFailed =
    Energinet.Charges.Contracts.CreateDefaultChargeLinksReply.Types.CreateDefaultChargeLinksFailed;
using CreateDefaultChargeLinksSucceeded =
    Energinet.Charges.Contracts.CreateDefaultChargeLinksReply.Types.CreateDefaultChargeLinksSucceeded;

namespace Energinet.DataHub.Charges.Clients.CreateDefaultChargeLink.Tests.DefaultChargeLink
{
    [UnitTest]
    public class DefaultChargeLinkReplyReaderTests
    {
        private ErrorCode _errorCodeTestResult = ErrorCode.MeteringPointUnknown;
        private string? _knownMeteringPointIdTestResult;
        private string? _unknownMeteringPointIdTestResult;
        private bool _didCreateChargeLinksTestResult;

        [Theory]
        [InlineAutoDomainData("knownMeteringPointId1234", true)]
        [InlineAutoDomainData("knownMeteringPointId5678", false)]
        public async Task ReadAsync_WhenDefaultChargeLinksCreationSucceeded_MapsDataAsSucceededDto(
            string meteringPointId, bool didCreateChargeLinks)
        {
            // Arrange
            var createDefaultChargeLinksSucceeded = new CreateDefaultChargeLinksReply
            {
                MeteringPointId = meteringPointId,
                CreateDefaultChargeLinksSucceeded = new CreateDefaultChargeLinksSucceeded
                {
                    DidCreateChargeLinks = didCreateChargeLinks,
                },
            };

            var data = createDefaultChargeLinksSucceeded.ToByteArray();

            var sut = new DefaultChargeLinkReplyReader(HandleSuccess, HandleFailure);

            // Act
            await sut.ReadAsync(data).ConfigureAwait(false);

            // Assert
            sut.Should().NotBeNull();
            _knownMeteringPointIdTestResult.Should().Be(meteringPointId);
            _didCreateChargeLinksTestResult.Should().Be(didCreateChargeLinks);
        }

        [Theory]
        [InlineAutoDomainData("unknownMeteringPointId9876")]
        public async Task DefaultChargeLinksCreationFailed(string meteringPointId)
        {
            // Arrange
            var createDefaultChargeLinksReply = new CreateDefaultChargeLinksReply
                {
                    MeteringPointId = meteringPointId,
                    CreateDefaultChargeLinksFailed = new CreateDefaultChargeLinksFailed
                    {
                        ErrorCode = CreateDefaultChargeLinksFailed.Types.ErrorCode.EcMeteringPointUnknown,
                    },
                };

            var data = createDefaultChargeLinksReply.ToByteArray();

            var sut = new DefaultChargeLinkReplyReader(HandleSuccess, HandleFailure);

            // Act
            await sut.ReadAsync(data).ConfigureAwait(false);

            // Assert
            sut.Should().NotBeNull();
            _unknownMeteringPointIdTestResult.Should().Be(meteringPointId);
            _errorCodeTestResult.Should().Be(ErrorCode.MeteringPointUnknown);
        }

        [Fact]
        public async Task DefaultChargeLinksCreation_Throws_Exception_When_Not_OneOf()
        {
            // Arrange
            var data = new CreateDefaultChargeLinksReply().ToByteArray();
            var sut = new DefaultChargeLinkReplyReader(HandleSuccess, HandleFailure);

            // Act
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await sut.ReadAsync(data).ConfigureAwait(false)).ConfigureAwait(false);
        }

        private async Task HandleFailure(DefaultChargeLinksCreationFailedStatusDto defaultChargeLinksCreationFailedStatus)
        {
            (_unknownMeteringPointIdTestResult, _errorCodeTestResult) = defaultChargeLinksCreationFailedStatus;

            await Task.CompletedTask.ConfigureAwait(false);
        }

        private async Task HandleSuccess(DefaultChargeLinksCreatedSuccessfullyDto defaultChargeLinksCreatedSuccessfully)
        {
            (_knownMeteringPointIdTestResult, _didCreateChargeLinksTestResult) = defaultChargeLinksCreatedSuccessfully;

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
