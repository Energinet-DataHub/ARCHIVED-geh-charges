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
using FluentAssertions;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.WebApi;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.WebApi.V1
{
    [IntegrationTest]
    [Collection(nameof(ChargesWebApiCollectionFixture))]
    public class ChargesControllerTests :
        WebApiTestBase<ChargesWebApiFixture>,
        IClassFixture<ChargesWebApiFixture>,
        IClassFixture<WebApiFactory>,
        IAsyncLifetime
    {
        private const string BaseUrl = "/v1/Charges/GetAsync";
        private readonly HttpClient _client;

        public ChargesControllerTests(
            ChargesWebApiFixture chargesWebApiFixture,
            WebApiFactory factory,
            ITestOutputHelper testOutputHelper)
            : base(chargesWebApiFixture, testOutputHelper)
        {
            _client = factory.CreateClient();
            factory.ReconfigureJwtTokenValidatorMock(isValid: true);
        }

        public Task InitializeAsync()
        {
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer xxx");
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            _client.Dispose();
            return Task.CompletedTask;
        }

        [Fact]
        public async Task GetAsync_WhenChargesExists_ReturnsOkAndCorrectContentType()
        {
            // Act
            var response = await _client.GetAsync($"{BaseUrl}");

            // Assert
            var contentType = response.Content.Headers.ContentType!.ToString();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            contentType.Should().Be("application/json; charset=utf-8");
        }

        [Fact]
        public async Task GetAsync_WhenRequested_ReturnsChargeInformation()
        {
            // Act
            var response = await _client.GetAsync($"{BaseUrl}");

            // Assert
            var jsonString = await response.Content.ReadAsStringAsync();
            var chargesList = JsonSerializer.Deserialize<List<ChargeV1Dto>>(
                jsonString,
                GetJsonSerializerOptions());

            chargesList.Should().HaveCountGreaterThan(0);
            var actual = chargesList!.Single(x => x.ChargeId == "EA-001");
            actual.ChargeType.Should().Be(ChargeType.D03);
            actual.Resolution.Should().Be(Resolution.PT1H);
            actual.ChargeName.Should().Be("Elafgift");
            actual.ChargeOwner.Should().Be("5790000432752");
            actual.ChargeOwnerName.Should().Be("<Aktørnavn XYZ>");
            actual.TaxIndicator.Should().BeTrue();
            actual.TransparentInvoicing.Should().BeTrue();
            actual.ValidFromDateTime.Should().Be(new DateTime(2014, 12, 31, 23, 00, 00));
            actual.ValidToDateTime.Should().Be(new DateTime(9999, 12, 31, 23, 59, 59));
        }

        private static JsonSerializerOptions GetJsonSerializerOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true, Converters = { new JsonStringEnumConverter() },
            };
        }
    }
}
