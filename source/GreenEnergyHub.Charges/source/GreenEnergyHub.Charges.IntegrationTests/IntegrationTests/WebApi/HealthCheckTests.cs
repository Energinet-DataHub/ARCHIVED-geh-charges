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
using System.Threading.Tasks;
using FluentAssertions;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.WebApi;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.WebApi
{
    [IntegrationTest]
    [Collection(nameof(ChargesWebApiCollectionFixture))]
    public class HealthCheckTests :
        WebApiTestBase<ChargesWebApiFixture>,
        IClassFixture<ChargesWebApiFixture>,
        IClassFixture<WebApiFactory>,
        IAsyncLifetime
    {
        private readonly HttpClient _client;

        public HealthCheckTests(
            ChargesWebApiFixture chargesWebApiFixture,
            WebApiFactory factory,
            ITestOutputHelper testOutputHelper)
            : base(chargesWebApiFixture, testOutputHelper)
        {
            _client = factory.CreateClient();
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            _client.Dispose();
            return Task.CompletedTask;
        }

        [Fact]
        public async Task When_RequestLivenessStatus_Then_ResponseIsOkAndHealthy()
        {
            var actualResponse = await _client.GetAsync("/monitor/live");

            // Assert
            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var actualContent = await actualResponse.Content.ReadAsStringAsync();
            actualContent.Should().Be("Healthy");
        }

        [Fact(Skip = "Leaving this test here for now, as we might change it to be a negative test by e.g. not having the database")]
        public async Task When_RequestReadinessStatus_Then_ResponseIsServiceUnavailableAndUnhealthy()
        {
            var actualResponse = await _client.GetAsync("/monitor/ready");

            // Assert
            actualResponse.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);

            var actualContent = await actualResponse.Content.ReadAsStringAsync();
            actualContent.Should().Be("Unhealthy");
        }

        [Fact]
        public async Task When_RequestReadinessStatus_Then_ResponseIsOkAndHealthy()
        {
            var actualResponse = await _client.GetAsync("/monitor/ready");

            // Assert
            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var actualContent = await actualResponse.Content.ReadAsStringAsync();
            actualContent.Should().Be("Healthy");
        }
    }
}
