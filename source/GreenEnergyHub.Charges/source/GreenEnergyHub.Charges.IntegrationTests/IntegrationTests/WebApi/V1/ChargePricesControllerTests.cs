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
using Energinet.DataHub.Charges.Contracts.ChargePrice;
using FluentAssertions;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.WebApi;
using GreenEnergyHub.Charges.TestCore.Attributes;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using JsonSerializerOptions = System.Text.Json.JsonSerializerOptions;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.WebApi.V1
{
    [IntegrationTest]
    [Collection(nameof(ChargesWebApiCollectionFixture))]
    public class ChargePricesControllerTests :
        WebApiTestBase<ChargesWebApiFixture>,
        IClassFixture<ChargesWebApiFixture>,
        IClassFixture<ChargesDatabaseFixture>,
        IClassFixture<WebApiFactory>
    {
        private const string BaseUrl = "/v1/ChargePrices";
        private readonly ChargesDatabaseManager _databaseManager;

        public ChargePricesControllerTests(
            ChargesWebApiFixture chargesWebApiFixture,
            ITestOutputHelper testOutputHelper,
            ChargesDatabaseFixture fixture)
            : base(chargesWebApiFixture, testOutputHelper)
        {
            _databaseManager = fixture.DatabaseManager;
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task SearchAsync_WhenChargePricesExists_ReturnsOkAndCorrectContentType(
            WebApiFactory factory)
        {
            // Arrange
            var sut = CreateHttpClient(factory);
            var searchCriteria = new ChargePricesSearchCriteriaV1Dto(Guid.NewGuid(), DateTime.Now, DateTime.Now);

            // Act
            var response = await sut.PostAsJsonAsync($"{BaseUrl}/Search", searchCriteria);

            // Assert
            var contentType = response.Content.Headers.ContentType!.ToString();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            contentType.Should().Be("application/json; charset=utf-8");
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task SearchAsync_WhenRequested_ReturnsChargePrices(WebApiFactory factory)
        {
            // Arrange
            await using var chargesDatabaseContext = _databaseManager.CreateDbQueryContext();

            var charge = chargesDatabaseContext.Charges.Include(c => c.ChargePoints).Single(c => c.SenderProvidedChargeId == "TestTariff");
            var chargePoint = charge.ChargePoints.First();

            var sut = CreateHttpClient(factory);
            var searchCriteria =
                new ChargePricesSearchCriteriaV1Dto(
                    chargePoint.ChargeId,
                    chargePoint.Time.AddDays(-1),
                    chargePoint.Time.AddDays(1));

            // Act
            var response = await sut.PostAsJsonAsync($"{BaseUrl}/Search", searchCriteria);

            // Assert
            var jsonString = await response.Content.ReadAsStringAsync();
            var chargePricesList = JsonSerializer.Deserialize<List<ChargePriceV1Dto>>(
                jsonString,
                GetJsonSerializerOptions());

            chargePricesList.Should().HaveCountGreaterThan(0);
            var actual = chargePricesList!.Single(cp => cp.Price == chargePoint.Price);
            actual.FromDateTime.Should().Be(chargePoint.Time);
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
