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
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Energinet.Charges.Contracts.ChargeLink;
using Energinet.DataHub.Charges.Clients.ChargeLinks;
using FluentAssertions;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Moq;
using Moq.Protected;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Charges.Clients.CreateDefaultChargeLink.Tests.ChargeLinks
{
    [UnitTest]
    public class ChargeLinksClientTests
    {
        private const string BaseUrl = "https://chargelinks-test.com/";
        private const string MeteringPointId = "57131310000000000";

        [Theory]
        [InlineAutoMoqData]
        public async Task GetAsync_WhenMeteringPointHasLinks_ReturnsChargeLinks(
            ChargeLinkV2Dto chargeLinkDto,
            Mock<IChargeLinksClientFactory> chargeLinksClientFactory)
        {
            // Arrange
            var responseContent = CreateValidResponseContent(chargeLinkDto);
            var mockHttpMessageHandler = GetMockHttpMessageHandler(HttpStatusCode.OK, responseContent);
            var httpClient = CreateHttpClient(mockHttpMessageHandler);
            chargeLinksClientFactory.Setup(x => x.CreateClient(httpClient))
                                    .Returns(new ChargeLinksClient(httpClient));

            var sut = chargeLinksClientFactory.Object.CreateClient(httpClient);

            var expectedUri = new Uri($"{BaseUrl}{ChargesRelativeUris.GetChargeLinks(MeteringPointId)}");

            // Act
            var result = await sut.GetAsync(MeteringPointId).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result[0].ChargeId.Should().Be(chargeLinkDto.ChargeId);
            result[0].ChargeType.Should().Be(chargeLinkDto.ChargeType);
            result[0].StartDate.Should().Be(chargeLinkDto.StartDate);

            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri == expectedUri),
                ItExpr.IsAny<CancellationToken>());
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task GetAsync_WhenResponseIsNotFound_ReturnsEmptyList(
            Mock<IChargeLinksClientFactory> chargeLinksClientFactory)
        {
            // Arrange
            var mockHttpMessageHandler = GetMockHttpMessageHandler(HttpStatusCode.NotFound, string.Empty);
            var httpClient = CreateHttpClient(mockHttpMessageHandler);
            chargeLinksClientFactory.Setup(x => x.CreateClient(httpClient))
                                    .Returns(new ChargeLinksClient(httpClient));

            var sut = chargeLinksClientFactory.Object.CreateClient(httpClient);

            // Act
            var result = await sut.GetAsync(MeteringPointId).ConfigureAwait(false);

            // Assert
            result.Should().BeOfType<List<ChargeLinkV2Dto>>();
            result.Should().BeEmpty();
        }

        private static string CreateValidResponseContent(ChargeLinkV2Dto chargeLinkDto)
        {
            var chargeLinks = new List<ChargeLinkV2Dto> { chargeLinkDto };
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                Converters = { new JsonStringEnumConverter() },
            };

            var responseContent = JsonSerializer.Serialize<IList<ChargeLinkV2Dto>>(chargeLinks, options);
            return responseContent;
        }

        private static HttpClient CreateHttpClient(Mock<HttpMessageHandler> mockHttpMessageHandler)
        {
            return new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri(BaseUrl),
            };
        }

        private static Mock<HttpMessageHandler> GetMockHttpMessageHandler(HttpStatusCode statusCode, string responseContent)
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = statusCode,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json"),
                })
                .Verifiable();
            return mockHttpMessageHandler;
        }
    }
}
