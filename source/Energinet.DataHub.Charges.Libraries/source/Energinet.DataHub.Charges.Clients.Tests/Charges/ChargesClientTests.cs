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
using Energinet.DataHub.Charges.Clients.Charges;
using Energinet.DataHub.Charges.Contracts.Charge;
using Energinet.DataHub.Charges.Contracts.ChargeLink;
using Energinet.DataHub.Charges.Contracts.ChargePrice;
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
        public async Task GetChargesAsync_WhenResponseIsEmptyList_ReturnsEmptyList(
            Mock<IChargesClientFactory> chargesClientFactory)
        {
            // Arrange
            var mockHttpMessageHandler = GetMockHttpMessageHandler(HttpStatusCode.OK, "[]");
            var httpClient = CreateHttpClient(mockHttpMessageHandler);
            chargesClientFactory.Setup(x => x.CreateClient(httpClient))
                .Returns(new ChargesClient(httpClient));

            var sut = chargesClientFactory.Object.CreateClient(httpClient);

            // Act
            var result = await sut.GetChargesAsync().ConfigureAwait(false);

            // Assert
            result.Should().BeEmpty();
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task SearchChargesAsync_WhenResponseIsNotSuccess_ThrowsException(
            Mock<IChargesClientFactory> chargesClientFactory,
            ChargeSearchCriteriaV1Dto searchCriteria)
        {
            // Arrange
            var mockHttpMessageHandler = GetMockHttpMessageHandler(HttpStatusCode.InternalServerError, string.Empty);
            var httpClient = CreateHttpClient(mockHttpMessageHandler);
            chargesClientFactory.Setup(x => x.CreateClient(httpClient))
                .Returns(new ChargesClient(httpClient));

            var sut = chargesClientFactory.Object.CreateClient(httpClient);

            // Act / Assert
            await Assert.ThrowsAsync<Exception>(async () => await sut.SearchChargesAsync(searchCriteria).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task SearchChargesAsync_WhenResponseIsBadRequest_ThrowsExceptionWithMessage(
            Mock<IChargesClientFactory> chargesClientFactory,
            ChargeSearchCriteriaV1Dto searchCriteria)
        {
            // Arrange
            var message = "Validation not valid";
            var mockHttpMessageHandler = GetMockHttpMessageHandler(HttpStatusCode.BadRequest, message);
            var httpClient = CreateHttpClient(mockHttpMessageHandler);
            chargesClientFactory.Setup(x => x.CreateClient(httpClient))
                .Returns(new ChargesClient(httpClient));

            var sut = chargesClientFactory.Object.CreateClient(httpClient);

            // Act / Assert
            var ex = await Assert.ThrowsAsync<Exception>(async () => await sut.SearchChargesAsync(searchCriteria).ConfigureAwait(false)).ConfigureAwait(false);
            ex.Message.Should().Contain(message);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task SearchChargesAsync_WhenResponseIsEmptyList_ReturnsEmptyList(
            Mock<IChargesClientFactory> chargesClientFactory,
            ChargeSearchCriteriaV1Dto searchCriteria)
        {
            // Arrange
            var mockHttpMessageHandler = GetMockHttpMessageHandler(HttpStatusCode.OK, "[]");
            var httpClient = CreateHttpClient(mockHttpMessageHandler);
            chargesClientFactory.Setup(x => x.CreateClient(httpClient))
                .Returns(new ChargesClient(httpClient));

            var sut = chargesClientFactory.Object.CreateClient(httpClient);

            // Act
            var actual = await sut.SearchChargesAsync(searchCriteria).ConfigureAwait(false);

            // Assert
            actual.Should().BeEmpty();
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task SearchChargesAsync_WhenSuccess_ReturnsCharges(
            Mock<IChargesClientFactory> chargesClientFactory,
            ChargeSearchCriteriaV1Dto searchCriteria,
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

        [Theory]
        [InlineAutoDomainData]
        public async Task GetMarketParticipantsAsync_WhenSuccess_ReturnsMarketParticipants(
            MarketParticipantV1Dto marketParticipantDto,
            Mock<IChargesClientFactory> chargesClientFactory)
        {
            // Arrange
            var responseContent = CreateValidResponseContent(marketParticipantDto);
            var mockHttpMessageHandler = GetMockHttpMessageHandler(HttpStatusCode.OK, responseContent);
            var httpClient = CreateHttpClient(mockHttpMessageHandler);
            chargesClientFactory.Setup(x => x.CreateClient(httpClient))
                .Returns(new ChargesClient(httpClient));

            var sut = chargesClientFactory.Object.CreateClient(httpClient);

            var expectedUri = new Uri($"{BaseUrl}{ChargesRelativeUris.GetMarketParticipants()}");

            // Act
            var result = await sut.GetMarketParticipantsAsync().ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result[0].Id.Should().Be(marketParticipantDto.Id);
            result[0].MarketParticipantId.Should().Be(marketParticipantDto.MarketParticipantId);
            result[0].Name.Should().Be(marketParticipantDto.Name);

            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri == expectedUri),
                ItExpr.IsAny<CancellationToken>());
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task GetMarketParticipantsAsync_WhenResponseIsEmptyList_ReturnsEmptyList(
            Mock<IChargesClientFactory> chargesClientFactory)
        {
            // Arrange
            var mockHttpMessageHandler = GetMockHttpMessageHandler(HttpStatusCode.OK, "[]");
            var httpClient = CreateHttpClient(mockHttpMessageHandler);
            chargesClientFactory.Setup(x => x.CreateClient(httpClient))
                .Returns(new ChargesClient(httpClient));

            var sut = chargesClientFactory.Object.CreateClient(httpClient);

            // Act
            var result = await sut.GetMarketParticipantsAsync().ConfigureAwait(false);

            // Assert
            result.Should().BeEmpty();
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task GetMarketParticipantsAsync_WhenResponseIsNotSuccess_ThrowsException(
            Mock<IChargesClientFactory> chargesClientFactory)
        {
            // Arrange
            var mockHttpMessageHandler = GetMockHttpMessageHandler(HttpStatusCode.InternalServerError, string.Empty);
            var httpClient = CreateHttpClient(mockHttpMessageHandler);
            chargesClientFactory.Setup(x => x.CreateClient(httpClient))
                .Returns(new ChargesClient(httpClient));

            var sut = chargesClientFactory.Object.CreateClient(httpClient);

            // Act / Assert
            await Assert.ThrowsAsync<Exception>(async () => await sut.GetMarketParticipantsAsync().ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task SearchChargePointsAsync_WhenResponseIsEmptyList_ReturnsEmptyList(
            Mock<IChargesClientFactory> chargesClientFactory,
            ChargePricesSearchCriteriaV1Dto searchCriteria)
        {
            // Arrange
            var mockHttpMessageHandler = GetMockHttpMessageHandler(HttpStatusCode.OK, "[]");
            var httpClient = CreateHttpClient(mockHttpMessageHandler);
            chargesClientFactory.Setup(x => x.CreateClient(httpClient))
                .Returns(new ChargesClient(httpClient));

            var sut = chargesClientFactory.Object.CreateClient(httpClient);

            // Act
            var result = await sut.SearchChargePricesAsync(searchCriteria).ConfigureAwait(false);

            // Assert
            result.Should().BeEmpty();
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task SearchChargePointsAsync_WhenResponseIsNotSuccess_ThrowsException(
            Mock<IChargesClientFactory> chargesClientFactory,
            ChargePricesSearchCriteriaV1Dto searchCriteria)
        {
            // Arrange
            var mockHttpMessageHandler = GetMockHttpMessageHandler(HttpStatusCode.InternalServerError, string.Empty);
            var httpClient = CreateHttpClient(mockHttpMessageHandler);
            chargesClientFactory.Setup(x => x.CreateClient(httpClient))
                .Returns(new ChargesClient(httpClient));

            var sut = chargesClientFactory.Object.CreateClient(httpClient);

            // Act / Assert
            await Assert.ThrowsAsync<Exception>(async () => await sut.SearchChargePricesAsync(searchCriteria).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Theory]
        [InlineAutoDomainData]
        public async Task SearchChargePointsAsync_WhenSuccess_ReturnsChargePoints(
            Mock<IChargesClientFactory> chargesClientFactory,
            ChargePricesSearchCriteriaV1Dto searchCriteria,
            ChargePriceV1Dto chargePointDto)
        {
            // Arrange
            var responseContent = CreateValidResponseContent(chargePointDto);
            var mockHttpMessageHandler = GetMockHttpMessageHandler(HttpStatusCode.OK, responseContent);
            var httpClient = CreateHttpClient(mockHttpMessageHandler);
            chargesClientFactory.Setup(x => x.CreateClient(httpClient))
                .Returns(new ChargesClient(httpClient));

            var sut = chargesClientFactory.Object.CreateClient(httpClient);

            var expectedUri = new Uri($"{BaseUrl}{ChargesRelativeUris.SearchChargePrices()}");

            // Act
            var result = await sut.SearchChargePricesAsync(searchCriteria).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result[0].Price.Should().Be(chargePointDto.Price);
            result[0].FromDateTime.Should().Be(chargePointDto.FromDateTime);
            result[0].ToDateTime.Should().Be(chargePointDto.ToDateTime);

            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri == expectedUri),
                ItExpr.IsAny<CancellationToken>());
        }

        private static string CreateValidResponseContent<TModel>(TModel responseDto)
        {
            var responseDtos = new List<TModel> { responseDto };
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

            var responseContent = JsonSerializer.Serialize<IList<TModel>>(responseDtos, options);
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
