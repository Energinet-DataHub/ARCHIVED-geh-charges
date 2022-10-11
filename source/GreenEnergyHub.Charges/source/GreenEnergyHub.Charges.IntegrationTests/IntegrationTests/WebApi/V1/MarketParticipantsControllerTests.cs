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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Energinet.Charges.Contracts.Charge;
using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using FluentAssertions;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.WebApi;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.WebApi.V1
{
    [IntegrationTest]
    [Collection(nameof(ChargesWebApiCollectionFixture))]

    public class MarketParticipantsControllerTests :
        WebApiTestBase<ChargesWebApiFixture>,
        IClassFixture<ChargesWebApiFixture>,
        IClassFixture<WebApiFactory>
    {
        private const string BaseUrl = "/v1/MarketParticipants";

        public MarketParticipantsControllerTests(
            ChargesWebApiFixture chargesWebApiFixture,
            ITestOutputHelper testOutputHelper)
            : base(chargesWebApiFixture, testOutputHelper)
        {
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task GetAsync_WhenRequested_ReturnOkAndCorrectContentType(WebApiFactory factory)
        {
            // Arrange
            var sut = CreateHttpClient(factory);

            // Act
            var response = await sut.GetAsync($"{BaseUrl}/GetAsync");

            // Assert
            var contentType = response.Content.Headers.ContentType!.ToString();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            contentType.Should().Be("application/json; charset=utf-8");
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task GetAsync_WhenRequested_ReturnsMarketParticipant(WebApiFactory factory)
        {
            // Arrange
            var sut = CreateHttpClient(factory);

            // Act
            var response = await sut.GetAsync($"{BaseUrl}/GetAsync");

            // Assert
            var jsonString = await response.Content.ReadAsStringAsync();
            var marketParticipantsList = JsonSerializer.Deserialize<List<MarketParticipantV1Dto>>(
                jsonString,
                GetJsonSerializerOptions());

            marketParticipantsList.Should().HaveCountGreaterThan(0);
            var actual = marketParticipantsList!.Single(x => x.Id == Guid.Parse("75ED087C-5A15-4D30-8711-FCD509A8D559"));
            actual.MarketParticipantId.Should().Be("8100000000016");
            actual.Name.Should().Be("Pending charge owner name");
        }

        private static JsonSerializerOptions GetJsonSerializerOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() },
            };
        }

        private static HttpClient CreateHttpClient(WebApiFactory factory)
        {
            var sut = factory.CreateClient();
            factory.ReconfigureJwtTokenValidatorMock(isValid: true);
            sut.DefaultRequestHeaders.Add("Authorization", $"Bearer xxx");
            return sut;
        }
    }
}
