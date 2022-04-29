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
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestFiles.ChargeLinks;
using GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers;
using GreenEnergyHub.Charges.SystemTests.Fixtures;
using NodaTime;
using Xunit;

namespace GreenEnergyHub.Charges.SystemTests
{
    /// <summary>
    /// Contains system tests where we operate at the level of the API Management.
    /// </summary>
    [Collection("SystemTest")]
    public class ApiManagementTests : IClassFixture<ApiManagementConfiguration>
    {
        private readonly BackendAuthenticationClient _authenticationClient;

        public ApiManagementTests(ApiManagementConfiguration configuration)
        {
            Configuration = configuration;

            _authenticationClient = new BackendAuthenticationClient(
                Configuration.AuthorizationConfiguration.BackendAppScope,
                Configuration.AuthorizationConfiguration.ClientCredentialsSettings,
                Configuration.AuthorizationConfiguration.B2cTenantId);
        }

        private ApiManagementConfiguration Configuration { get; }

        // This shows how we can extract an access token for accessing the 'backend app' on behalf of the 'team client app'
        [SystemFact]
        public async Task When_AcquireTokenForTeamVoltClientApp_Then_AccessTokenIsReturned()
        {
            var actualAuthenticationResult = await _authenticationClient.GetAuthenticationTokenAsync();
            actualAuthenticationResult.AccessToken.Should().NotBeNullOrWhiteSpace();
        }

        // This shows how we can call API Management using a valid access token
        [SystemFact]
        public async Task When_RequestApiManagementWithAccessToken_Then_ResponseIsAccepted()
        {
            // Arrange
            using var httpClient = await CreateHttpClientAsync(_authenticationClient);

            var currentInstant = SystemClock.Instance.GetCurrentInstant();
            var xml = EmbeddedResourceHelper.GetEmbeddedFile(ChargeLinkDocument.AnyValid, currentInstant);

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

            var currentInstant = SystemClock.Instance.GetCurrentInstant();
            var xml = EmbeddedResourceHelper.GetEmbeddedFile(ChargeLinkDocument.AnyValid, currentInstant);

            var request = new HttpRequestMessage(HttpMethod.Post, "v1.0/cim/requestchangebillingmasterdata")
            {
                Content = new StringContent(xml, Encoding.UTF8, "application/xml"),
            };

            // Act
            using var actualResponse = await httpClient.SendAsync(request);

            // Assert
            actualResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        /// <summary>
        /// Create a http client. Will add an access token if <paramref name="confidentialClientApp"/> is specified.
        /// </summary>
        /// <param name="confidentialClientApp">If not null: an access token is acquired using the client, and set in the authorization header of the http client.</param>
        private async Task<HttpClient> CreateHttpClientAsync(BackendAuthenticationClient? confidentialClientApp = null)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = Configuration.ApiManagementBaseAddress,
            };

            if (confidentialClientApp != null)
            {
                var authenticationResult = await _authenticationClient.GetAuthenticationTokenAsync();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);
            }

            return httpClient;
        }
    }
}
