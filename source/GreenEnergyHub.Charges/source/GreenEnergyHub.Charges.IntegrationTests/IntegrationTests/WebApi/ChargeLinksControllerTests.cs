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
using System.Text.Json;
using System.Threading.Tasks;
using Energinet.Charges.Contracts.ChargeLink;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.WebApi
{
    [IntegrationTest]
    public class ChargeLinksControllerTests : IClassFixture<WebApplicationFactory<Charges.WebApi.Startup>>
    {
        private const string BaseUrl = "/ChargeLinks/GetAsync?meteringPointId=";
        private readonly WebApplicationFactory<Charges.WebApi.Startup> _factory;

        public ChargeLinksControllerTests(WebApplicationFactory<Charges.WebApi.Startup> factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("571313180000000005")]
        public async Task GetAsync_WhenMeteringPointIdHasChargeLinks_ReturnsOkAndCorrectContentType(string meteringPointId)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync($"{BaseUrl}{meteringPointId}");

            // Assert
            var contentType = response.Content.Headers.ContentType!.ToString();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            contentType.Should().Be("application/json; charset=utf-8");
        }

        [Theory]
        [InlineData("571313180000000005")]
        public async Task GetAsync_WhenMeteringPointIdHasChargeLinks_ReturnsOrderedChargeLinks(string meteringPointId)
        {
            // Arrange
            var client = _factory.CreateClient();
            var expectedChargeLinks = 5;
            var expectedChargeId = "TestTariff2"; // To verify ordering works as expected
            var expectedStart = new DateTimeOffset(2021, 12, 31, 23, 00, 00, TimeSpan.FromHours(0));

            // Act
            var response = await client.GetAsync($"{BaseUrl}{meteringPointId}");
            var jsonString = await response.Content.ReadAsStringAsync();
            var actual = JsonSerializer.Deserialize<List<ChargeLinkDto>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Assert
            actual!.Count.Should().Be(expectedChargeLinks);
            actual![3].ChargeId.Should().Be(expectedChargeId);
            actual![3].StartDate.Should().Be(expectedStart);
        }

        [Theory]
        [InlineData("404")]
        public async Task GetAsync_WhenMeteringPointIdDoesNotExist_ReturnsNotFound(string meteringPointId)
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"{BaseUrl}{meteringPointId}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Theory]
        [InlineData("")]
        public async Task GetAsync_WhenNoMeteringPointIdInput_ReturnsBadRequest(string meteringPointId)
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"{BaseUrl}{meteringPointId}");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
