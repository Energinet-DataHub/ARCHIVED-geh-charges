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
using GreenEnergyHub.Charges.IntegrationTests.TestHelpers;
using GreenEnergyHub.Charges.SystemTests.Fixtures;
using Microsoft.Identity.Client;
using NodaTime;

namespace GreenEnergyHub.Charges.SystemTests
{
    public class ApiManagementTests
    {
        public ApiManagementTests()
        {
            Configuration = new ApiManagementConfiguration();
        }

        private ApiManagementConfiguration Configuration { get; }

        private string TeamVolt => "volt";

        // This shows how we can extract an access token on behalf of the "team client"
        [SystemFact]
        public async Task When_AquireTokenForTeamVoltClient_Then_AccessTokenIsReturned()
        {
            // Arrange
            var b2cSettings = Configuration.RetrieveB2CSettings(TeamVolt);

            var confidentialClientApp = ConfidentialClientApplicationBuilder
                .Create(b2cSettings.TeamClientId)
                .WithClientSecret(b2cSettings.TeamClientSecret)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{b2cSettings.B2cTenantId}"))
                .Build();

            var scopes = new[] { $"{b2cSettings.BackendAppId}/.default" };

            // Act
            var actualAuthenticationResult = await confidentialClientApp.AcquireTokenForClient(scopes).ExecuteAsync();

            // Assert
            actualAuthenticationResult.AccessToken.Should().NotBeNullOrWhiteSpace();
        }

        // This shows how we can call API Management using a valid access token
        [SystemFact]
        public async Task When_RequestApiManagementWithAccessToken_Then_ResponseIsOk()
        {
            // Arrange
            var b2cSettings = Configuration.RetrieveB2CSettings(TeamVolt);
            var confidentialClientApp = ConfidentialClientApplicationBuilder
                .Create(b2cSettings.TeamClientId)
                .WithClientSecret(b2cSettings.TeamClientSecret)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{b2cSettings.B2cTenantId}"))
                .Build();

            var scopes = new[] { $"{b2cSettings.BackendAppId}/.default" };
            var authenticationResult = await confidentialClientApp.AcquireTokenForClient(scopes).ExecuteAsync();

            using var httpClient = new HttpClient
            {
                BaseAddress = Configuration.ApiManagementBaseAddress,
            };

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);

            var clock = SystemClock.Instance;
            var xml = EmbeddedResourceHelper.GetEmbeddedFile("TestFiles/RequestChangeBillingMasterData.xml", clock);

            var request = new HttpRequestMessage(HttpMethod.Post, "v1.0/cim/requestbillingmasterdata")
            {
                Content = new StringContent(xml, Encoding.UTF8, "application/xml"),
            };

            // Act
            using var actualResponse = await httpClient.SendAsync(request);

            // Assert
            actualResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
