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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using GreenEnergyHub.Charges.FunctionHost.Common;
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
            actualContent.Should().Be(Enum.GetName(typeof(HealthStatus), HealthStatus.Healthy));
        }

        [Fact]
        public async Task When_ChargeDatabaseIsDeletedAndRequestReadinessStatus_Then_ResponseIsServiceUnavailableAndUnhealthy()
        {
            try
            {
                await Fixture.DatabaseManager.DeleteDatabaseAsync();
                var actualResponse = await _client.GetAsync("/monitor/ready");

                // Assert
                actualResponse.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);

                var actualContent = await actualResponse.Content.ReadAsStringAsync();
                actualContent.Should().Be(Enum.GetName(typeof(HealthStatus), HealthStatus.Unhealthy));
            }
            finally
            {
                await Fixture.DatabaseManager.CreateDatabaseAsync();
            }
        }

        [Fact]
        public async Task CWhen_RequestReadinessStatus_Then_ResponseIsOkAndHealthy()
        {
            var actualResponse = await _client.GetAsync("/monitor/ready");

            // Assert
            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var actualContent = await actualResponse.Content.ReadAsStringAsync();
            actualContent.Should().Be(Enum.GetName(typeof(HealthStatus), HealthStatus.Healthy));
        }
    }
}
