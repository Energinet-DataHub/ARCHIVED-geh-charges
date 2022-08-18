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
using System.Threading.Tasks;
using FluentAssertions;
using GreenEnergyHub.Charges.IntegrationTest.Core.Fixtures.WebApi;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace GreenEnergyHub.Charges.IntegrationTests.IntegrationTests.WebApi
{
    [IntegrationTest]
    [Collection(nameof(ChargesWebApiCollectionFixture))]
    public class SwaggerTests :
        WebApiTestBase<ChargesWebApiFixture>,
        IClassFixture<ChargesWebApiFixture>,
        IClassFixture<WebApiFactory>,
        IAsyncLifetime
    {
        private readonly HttpClient _client;
        private readonly BackendAuthenticationClient _authenticationClient;

        public SwaggerTests(
            ChargesWebApiFixture chargesWebApiFixture,
            WebApiFactory factory,
            ITestOutputHelper testOutputHelper)
            : base(chargesWebApiFixture, testOutputHelper)
        {
            _client = factory.CreateClient();
            var clientCredentialsSettings = chargesWebApiFixture.AuthorizationConfiguration.B2CTestClients
                .First(tc => tc.ClientName == AuthorizationConfigurationData.GridAccessProvider8100000000030)
                .ClientCredentialsSettings;

            _authenticationClient = new BackendAuthenticationClient(
                chargesWebApiFixture.AuthorizationConfiguration.BackendAppScope,
                clientCredentialsSettings,
                chargesWebApiFixture.AuthorizationConfiguration.B2CTenantId);
        }

        public async Task InitializeAsync()
        {
            var authenticationResult = await _authenticationClient.GetAuthenticationTokenAsync();
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authenticationResult.AccessToken}");
        }

        public Task DisposeAsync()
        {
            _client.Dispose();
            return Task.CompletedTask;
        }

        [Fact]
        public async Task StartPage_ReturnsOk()
        {
            var response = await _client.GetAsync("/swagger/index.html");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Theory]
        [InlineData("v1")]
        [InlineData("v2")]
        public async Task SpecificationForVersion_ReturnsOk(string version)
        {
            var response = await _client.GetAsync($"/swagger/{version}/swagger.json");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
