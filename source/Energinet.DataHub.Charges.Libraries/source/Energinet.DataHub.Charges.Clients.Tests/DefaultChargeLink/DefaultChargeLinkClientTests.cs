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
using AutoFixture.Xunit2;
using Energinet.DataHub.Charges.Clients.DefaultChargeLink;
using Energinet.DataHub.Charges.Clients.DefaultChargeLink.Models;
using Energinet.DataHub.Charges.Clients.ServiceBus;
using Energinet.DataHub.Charges.Clients.ServiceBus.Providers;
using GreenEnergyHub.TestHelpers;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Charges.Clients.CreateDefaultChargeLink.Tests.DefaultChargeLink
{
    [UnitTest]
    public class DefaultChargeLinkClientTests
    {
        private const string MeteringPointId = "F9A5115D-44EB-4AD4-BC7E-E8E8A0BC425E";
        private const string CorrelationId = "fake_value";

        [Theory]
        [InlineAutoDomainData(MeteringPointId, null!)]
        [InlineAutoDomainData(null!, CorrelationId)]
        public async Task SendAsync_WhenAnyArgumentIsNull_ThrowsArgumentNullException(
            string meteringPointId,
            string correlationId,
            [Frozen] Mock<IServiceBusRequestSenderProvider> serviceBusRequestSenderProviderMock)
        {
            // Arrange
            var createDefaultChargeLinksDto = new RequestDefaultChargeLinksForMeteringPointDto(meteringPointId);

            var sut = new DefaultChargeLinkClient(serviceBusRequestSenderProviderMock.Object);

            // Act + Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut
                .CreateDefaultChargeLinksRequestAsync(createDefaultChargeLinksDto, correlationId))
                .ConfigureAwait(false);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task CreateDefaultChargeLinksRequestAsync_WhenInputIsValid_SendsMessage(
            [Frozen] Mock<IServiceBusRequestSenderProvider> serviceBusRequestSenderProviderMock,
            [Frozen] Mock<IServiceBusRequestSender> serviceBusRequestSenderMock)
        {
            // Arrange
            serviceBusRequestSenderProviderMock.Setup(x => x
                    .GetInstance())
                .Returns(serviceBusRequestSenderMock.Object);

            var sut = new DefaultChargeLinkClient(serviceBusRequestSenderProviderMock.Object);

            var createDefaultChargeLinksDto = new RequestDefaultChargeLinksForMeteringPointDto(MeteringPointId);

            // Act
            await sut.CreateDefaultChargeLinksRequestAsync(
                createDefaultChargeLinksDto, CorrelationId).ConfigureAwait(false);

            // Assert
            serviceBusRequestSenderMock.Verify(
                x => x.SendRequestAsync(
                    It.IsAny<byte[]>(),
                    CorrelationId),
                Times.Once);
        }
    }
}
