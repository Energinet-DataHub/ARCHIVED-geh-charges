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
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestFiles.ChargeLinks;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers;
using GreenEnergyHub.Charges.SystemTests.Fixtures;
using Microsoft.Identity.Client;
using NodaTime;
using Xunit;

namespace GreenEnergyHub.Charges.SystemTests
{
    /// <summary>
    /// Contains system tests where we operate at the level of the API Management.
    /// </summary>
    public class ApiManagementTests : IClassFixture<ApiManagementConfiguration>
    {
        public ApiManagementTests(ApiManagementConfiguration configuration)
        {
            Configuration = configuration;
            TeamVoltClientApp = CreateConfidentialClientApp("volt");
        }

        private ApiManagementConfiguration Configuration { get; }

        private IConfidentialClientApplication TeamVoltClientApp { get; }

        // This shows how we can extract an access token for accessing the 'backend app' on behalf of the 'team client app'
        [SystemFact]
        public async Task When_AcquireTokenForTeamVoltClientApp_Then_AccessTokenIsReturned()
        {
            var actualAuthenticationResult = await TeamVoltClientApp.AcquireTokenForClient(Configuration.BackendAppScope).ExecuteAsync();

            actualAuthenticationResult.AccessToken.Should().NotBeNullOrWhiteSpace();
        }

        // This shows how we can call API Management using a valid access token
        [SystemFact]
        public async Task When_RequestApiManagementWithAccessToken_Then_ResponseIsAccepted()
        {
            // Arrange
            using var httpClient = await CreateHttpClientAsync(TeamVoltClientApp);

            var clock = SystemClock.Instance;
            var xml = EmbeddedResourceHelper.GetEmbeddedFile(ChargeLinkDocument.AnyValid, clock);

            var request = new HttpRequestMessage(HttpMethod.Post, "v1.0/cim/requestchangebillingmasterdata")
            {
                Content = new StringContent(xml, Encoding.UTF8, "application/xml"),
            };

            // Act
            using var actualResponse = await httpClient.SendAsync(request);

            // Assert
            actualResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        }

        // This shows our request will fail if we call API Management without a valid access token
        [SystemFact]
        public async Task When_RequestApiManagementWithoutAccessToken_Then_ResponseIsUnauthorized()
        {
            // Arrange
            using var httpClient = await CreateHttpClientAsync();

            var clock = SystemClock.Instance;
            var xml = EmbeddedResourceHelper.GetEmbeddedFile(ChargeLinkDocument.AnyValid, clock);

            var request = new HttpRequestMessage(HttpMethod.Post, "v1.0/cim/requestchangebillingmasterdata")
            {
                Content = new StringContent(xml, Encoding.UTF8, "application/xml"),
            };

            // Act
            using var actualResponse = await httpClient.SendAsync(request);

            // Assert
            actualResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        private IConfidentialClientApplication CreateConfidentialClientApp(string team) // TODO: use BackendAuthenticationClient
        {
            var teamClientSettings = Configuration.RetrieveB2CTeamClientSettings(team);

            var confidentialClientApp = ConfidentialClientApplicationBuilder
                .Create(teamClientSettings.TeamClientId)
                .WithClientSecret(teamClientSettings.TeamClientSecret)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{Configuration.B2cTenantId}"))
                .Build();

            return confidentialClientApp;
        }

        /// <summary>
        /// Create a http client. Will add an access token if <paramref name="confidentialClientApp"/> is specified.
        /// </summary>
        /// <param name="confidentialClientApp">If not null: an access token is acquired using the client, and set in the authorization header of the http client.</param>
        private async Task<HttpClient> CreateHttpClientAsync(IConfidentialClientApplication? confidentialClientApp = null)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = Configuration.ApiManagementBaseAddress,
            };

            if (confidentialClientApp != null)
            {
                var authenticationResult = await confidentialClientApp.AcquireTokenForClient(Configuration.BackendAppScope).ExecuteAsync();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);
            }

            return httpClient;
        }
    }
}
