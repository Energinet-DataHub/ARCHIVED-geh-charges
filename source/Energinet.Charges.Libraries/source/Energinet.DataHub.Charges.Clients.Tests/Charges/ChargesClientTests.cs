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
using Energinet.Charges.Contracts.Charge;
using Energinet.Charges.Contracts.ChargeLink;
using Energinet.DataHub.Charges.Clients.Charges;
using FluentAssertions;
using GreenEnergyHub.TestHelpers;
using Moq;
using Moq.Protected;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Charges.Clients.CreateDefaultChargeLink.Tests.Charges
{
    [UnitTest]
    public class ChargesClientTests
    {
        private const string BaseUrl = "https://charges-test.com/";
        private const string MeteringPointId = "57131310000000000";

        [Theory]
        [InlineAutoDomainData]
        public async Task GetChargeLinksAsync_WhenMeteringPointHasLinks_ReturnsChargeLinks(
            ChargeLinkV1Dto chargeLinkDto,
            Mock<IChargesClientFactory> chargesClientFactory)
        {
            // Arrange
            var responseContent = CreateValidResponseContent(chargeLinkDto);
            var mockHttpMessageHandler = GetMockHttpMessageHandler(HttpStatusCode.OK, responseContent);
            var httpClient = CreateHttpClient(mockHttpMessageHandler);
            chargesClientFactory.Setup(x => x.CreateClient(httpClient))
                .Returns(new ChargesClient(httpClient));

            var sut = chargesClientFactory.Object.CreateClient(httpClient);

            var expectedUri = new Uri($"{BaseUrl}{ChargesRelativeUris.GetChargeLinks(MeteringPointId)}");

            // Act
            var result = await sut.GetChargeLinksAsync(MeteringPointId).ConfigureAwait(false);

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
        [InlineAutoDomainData]
        public async Task GetChargeLinksAsync_WhenResponseIsNotFound_ReturnsEmptyList(
            Mock<IChargesClientFactory> chargesClientFactory)
        {
            // Arrange
            var mockHttpMessageHandler = GetMockHttpMessageHandler(HttpStatusCode.NotFound, string.Empty);
            var httpClient = CreateHttpClient(mockHttpMessageHandler);
            chargesClientFactory.Setup(x => x.CreateClient(httpClient))
                .Returns(new ChargesClient(httpClient));

            var sut = chargesClientFactory.Object.CreateClient(httpClient);

            // Act
            var result = await sut.GetChargeLinksAsync(MeteringPointId).ConfigureAwait(false);

            // Assert
            result.Should().BeOfType<List<ChargeLinkV1Dto>>();
            result.Should().BeEmpty();
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task GetChargesAsync_WhenSuccess_ReturnsCharges(
            ChargeV1Dto chargeDto,
            Mock<IChargesClientFactory> chargesClientFactory)
        {
            // Arrange
            var responseContent = CreateValidResponseContent(chargeDto);
            var mockHttpMessageHandler = GetMockHttpMessageHandler(HttpStatusCode.OK, responseContent);
            var httpClient = CreateHttpClient(mockHttpMessageHandler);
            chargesClientFactory.Setup(x => x.CreateClient(httpClient))
                .Returns(new ChargesClient(httpClient));

            var sut = chargesClientFactory.Object.CreateClient(httpClient);

            var expectedUri = new Uri($"{BaseUrl}{ChargesRelativeUris.GetCharges()}");

            // Act
            var result = await sut.GetChargesAsync().ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result[0].ChargeId.Should().Be(chargeDto.ChargeId);
            result[0].ChargeType.Should().Be(chargeDto.ChargeType);
            result[0].ChargeName.Should().Be(chargeDto.ChargeName);

            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri == expectedUri),
                ItExpr.IsAny<CancellationToken>());
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task GetChargesAsync_WhenResponseIsNotFound_ReturnsEmptyList(
            Mock<IChargesClientFactory> chargesClientFactory)
        {
            // Arrange
            var mockHttpMessageHandler = GetMockHttpMessageHandler(HttpStatusCode.NotFound, string.Empty);
            var httpClient = CreateHttpClient(mockHttpMessageHandler);
            chargesClientFactory.Setup(x => x.CreateClient(httpClient))
                .Returns(new ChargesClient(httpClient));

            var sut = chargesClientFactory.Object.CreateClient(httpClient);

            // Act
            var result = await sut.GetChargesAsync().ConfigureAwait(false);

            // Assert
            result.Should().BeOfType<List<ChargeV1Dto>>();
            result.Should().BeEmpty();
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task SearchChargesAsync_WhenResponseIsNotFound_ReturnsEmptyList(
            Mock<IChargesClientFactory> chargesClientFactory,
            SearchCriteriaDtoBuilder searchCriteriaDtoBuilder)
        {
            // Arrange
            var mockHttpMessageHandler = GetMockHttpMessageHandler(HttpStatusCode.NotFound, string.Empty);
            var httpClient = CreateHttpClient(mockHttpMessageHandler);
            chargesClientFactory.Setup(x => x.CreateClient(httpClient))
                .Returns(new ChargesClient(httpClient));

            var sut = chargesClientFactory.Object.CreateClient(httpClient);
            var searchCriteria = searchCriteriaDtoBuilder.Build();

            // Act
            var result = await sut.SearchChargesAsync(searchCriteria).ConfigureAwait(false);

            // Assert
            result.Should().BeOfType<List<ChargeV1Dto>>();
            result.Should().BeEmpty();
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task SearchChargesAsync_WhenSuccess_ReturnsCharges(
            Mock<IChargesClientFactory> chargesClientFactory,
            SearchCriteriaDtoBuilder searchCriteriaDtoBuilder,
            ChargeV1Dto chargeDto)
        {
            // Arrange
            var responseContent = CreateValidResponseContent(chargeDto);
            var mockHttpMessageHandler = GetMockHttpMessageHandler(HttpStatusCode.OK, responseContent);
            var httpClient = CreateHttpClient(mockHttpMessageHandler);
            chargesClientFactory.Setup(x => x.CreateClient(httpClient))
                .Returns(new ChargesClient(httpClient));

            var sut = chargesClientFactory.Object.CreateClient(httpClient);

            var expectedUri = new Uri($"{BaseUrl}{ChargesRelativeUris.SearchCharges()}");
            var searchCriteria = searchCriteriaDtoBuilder.Build();

            // Act
            var result = await sut.SearchChargesAsync(searchCriteria).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result[0].ChargeId.Should().Be(chargeDto.ChargeId);
            result[0].ChargeType.Should().Be(chargeDto.ChargeType);
            result[0].ChargeName.Should().Be(chargeDto.ChargeName);

            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri == expectedUri),
                ItExpr.IsAny<CancellationToken>());
        }

        private static string CreateValidResponseContent<TModel>(TModel chargeLinkDto)
        {
            var chargeLinks = new List<TModel> { chargeLinkDto };
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

            var responseContent = JsonSerializer.Serialize<IList<TModel>>(chargeLinks, options);
            return responseContent;
        }

        private static HttpClient CreateHttpClient(Mock<HttpMessageHandler> mockHttpMessageHandler)
        {
            return new HttpClient(mockHttpMessageHandler.Object) { BaseAddress = new Uri(BaseUrl) };
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
                .ReturnsAsync(new HttpResponseMessage { StatusCode = statusCode, Content = new StringContent(responseContent, Encoding.UTF8, "application/json") })
                .Verifiable();
            return mockHttpMessageHandler;
        }
    }
}
