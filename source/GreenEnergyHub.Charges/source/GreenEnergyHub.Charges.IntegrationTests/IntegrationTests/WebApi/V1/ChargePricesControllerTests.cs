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

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Energinet.DataHub.Charges.Contracts.ChargePrice;
using FluentAssertions;
using FluentAssertions.Common;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.Database;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.WebApi;
using GreenEnergyHub.Charges.TestCore.Attributes;
using GreenEnergyHub.Charges.TestCore.Builders.Query;
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
        IClassFixture<WebApiFactory>
    {
        private const string BaseUrl = "/v1/ChargePrices";
        private readonly ChargesDatabaseManager _databaseManager;

        public ChargePricesControllerTests(
            ChargesWebApiFixture chargesWebApiFixture,
            ITestOutputHelper testOutputHelper)
            : base(chargesWebApiFixture, testOutputHelper)
        {
            _databaseManager = chargesWebApiFixture.DatabaseManager;
        }

        [Theory]
        [InlineAutoMoqData]
        public async Task SearchAsync_WhenChargePricesExists_ReturnsOkAndCorrectContentType(
            WebApiFactory factory)
        {
            // Arrange
            var sut = CreateHttpClient(factory);
            var searchCriteria = new ChargePricesSearchCriteriaV1DtoBuilder().Build();

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
            var expectedTotalAmount = (int)chargePoint.Price;
            var expectedFromDate = chargePoint.Time;
            var expectedToDate = chargePoint.Time.AddDays(1);

            var sut = CreateHttpClient(factory);
            var searchCriteria =
                new ChargePricesSearchCriteriaV1DtoBuilder()
                    .WithChargeId(chargePoint.ChargeId)
                    .WithFromDateTime(expectedFromDate)
                    .WithToDateTime(expectedToDate)
                    .Build();

            // Act
            var response = await sut.PostAsJsonAsync($"{BaseUrl}/Search", searchCriteria);

            // Assert
            var jsonString = await response.Content.ReadAsStringAsync();
            var chargePricesV1Dto = JsonSerializer.Deserialize<ChargePricesV1Dto>(
                jsonString,
                GetJsonSerializerOptions())!;

            chargePricesV1Dto.ChargePrices.Should().NotBeEmpty();
            chargePricesV1Dto.TotalAmount.Should().Be(expectedTotalAmount);
            var actualPrice = chargePricesV1Dto.ChargePrices.First();
            actualPrice.Price.Should().Be(chargePoint.Price);
            actualPrice.FromDateTime.Should().Be(expectedFromDate.ToDateTimeOffset());
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
