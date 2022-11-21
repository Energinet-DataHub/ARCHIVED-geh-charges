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
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Energinet.DataHub.Charges.Contracts.Charge;
using Energinet.DataHub.Charges.Contracts.ChargeHistory;
using FluentAssertions;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.WebApi;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Data;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using ChargeType = Energinet.DataHub.Charges.Contracts.Charge.ChargeType;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.WebApi.V1
{
    [IntegrationTest]
    [Collection(nameof(ChargesWebApiCollectionFixture))]
    [Trait("Category", "WebApiTests")]
    public class ChargeHistoryControllerTests :
        WebApiTestBase<ChargesWebApiFixture>,
        IClassFixture<ChargesWebApiFixture>,
        IClassFixture<WebApiFactory>
    {
        private const string BaseUrl = "/v1/ChargeHistory";

        public ChargeHistoryControllerTests(
            ChargesWebApiFixture chargesWebApiFixture,
            ITestOutputHelper testOutputHelper)
            : base(chargesWebApiFixture, testOutputHelper)
        {
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task GetAsync_WhenChargeHistoryExists_ReturnsOkAndCorrectContentType(
            WebApiFactory factory)
        {
            // Arrange
            var sut = CreateHttpClient(factory);

            var searchCriteria = new ChargeHistorySearchCriteriaV1Dto(
                "TestTar001",
                ChargeType.D03,
                SeededData.MarketParticipants.Provider8100000000030.Gln,
                DateTimeOffset.Now);

            // Act
            var response = await sut.PostAsJsonAsync($"{BaseUrl}/GetAsync", searchCriteria);

            // Assert
            var contentType = response.Content.Headers.ContentType!.ToString();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            contentType.Should().Be("application/json; charset=utf-8");
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task GetAsync_WhenRequested_ReturnsChargeHistory(WebApiFactory factory)
        {
            // Arrange
            var sut = CreateHttpClient(factory);
            var searchCriteria = new ChargeHistorySearchCriteriaV1Dto(
                "TestTar001",
                ChargeType.D03,
                SeededData.MarketParticipants.Provider8100000000030.Gln,
                DateTimeOffset.Now);

            // Act
            var response = await sut.PostAsJsonAsync($"{BaseUrl}/GetAsync", searchCriteria);

            // Assert
            var jsonString = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<List<ChargeHistoryV1Dto>>(
                jsonString,
                GetJsonSerializerOptions())!;

            actual.Should().HaveCount(1);

            var actualChargeHistoryV1Dto = actual.Single();
            actualChargeHistoryV1Dto.Name.Should().Be("Name");
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
            sut.DefaultRequestHeaders.Add("Authorization", "Bearer xxx");
            return sut;
        }
    }
}
