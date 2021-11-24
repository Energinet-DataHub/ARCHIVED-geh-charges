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

using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.Charges.Clients.Bff;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Charges.Bff.Tests
{
    [UnitTest]
    public class ChargeLinksClientTests
    {
        [Fact]
        public async Task GetChargeLinksByMeteringPointId_WhenCalledWithMeteringPointIdThatHasChargeLinks_ReturnsChargeLinks()
        {
            var mockMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mockMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("hello"),
                });

            var chargeLinksClient = ChargeLinksClientFactory.CreateClient(new HttpClient(mockMessageHandler.Object));

            var response = await chargeLinksClient.GetChargeLinksByMeteringPointIdAsync("1337").ConfigureAwait(false);
            response.Should().NotBeNull();
        }
    }
}
