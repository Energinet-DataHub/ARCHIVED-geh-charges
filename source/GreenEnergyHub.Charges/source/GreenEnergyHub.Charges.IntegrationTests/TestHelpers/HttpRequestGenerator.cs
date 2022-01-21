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
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.IntegrationTests.Fixtures;
using Microsoft.Identity.Client;
using NodaTime;

namespace GreenEnergyHub.Charges.IntegrationTests.TestHelpers
{
    public class HttpRequestGenerator
    {
        private readonly ChargesFunctionAppFixture _chargesFunctionAppFixture;
        private readonly string _endpointUrl;

        public HttpRequestGenerator(ChargesFunctionAppFixture chargesFunctionAppFixture, string endpointUrl)
        {
            _chargesFunctionAppFixture = chargesFunctionAppFixture;
            _endpointUrl = endpointUrl;
        }

        public async Task<(HttpRequestMessage Request, string CorrelationId)> CreateHttpPostRequestAsync(string testFilePath)
        {
            var clock = SystemClock.Instance;
            var chargeXml = EmbeddedResourceHelper.GetEmbeddedFile(testFilePath, clock);
            var correlationId = CorrelationIdGenerator.Create();

            var request = new HttpRequestMessage(HttpMethod.Post, _endpointUrl)
            {
                Content = new StringContent(chargeXml, Encoding.UTF8, "application/xml"),
            };
            request.ConfigureTraceContext(correlationId);

            var authenticationResult = await GetAuthenticationTokenAsync();
            request.Headers.Add("Authorization", $"Bearer {authenticationResult.AccessToken}");

            return (request, correlationId);
        }

        public async Task<(HttpRequestMessage Request, string CorrelationId)> CreateHttpGetRequestAsync()
        {
            var correlationId = CorrelationIdGenerator.Create();
            var request = new HttpRequestMessage(HttpMethod.Get, _endpointUrl);
            request.ConfigureTraceContext(correlationId);

            var authenticationResult = await GetAuthenticationTokenAsync();
            request.Headers.Add("Authorization", $"Bearer {authenticationResult.AccessToken}");

            return (request, correlationId);
        }

        private async Task<AuthenticationResult> GetAuthenticationTokenAsync()
        {
            var confidentialClientApp = CreateConfidentialClientApp();
            var result = await confidentialClientApp.AcquireTokenForClient(
                    _chargesFunctionAppFixture.AuthorizationConfiguration.BackendAppScope)
                .ExecuteAsync().ConfigureAwait(false);
            return result;
        }

        private IConfidentialClientApplication CreateConfidentialClientApp()
        {
            var (teamClientId, teamClientSecret) =
                _chargesFunctionAppFixture.AuthorizationConfiguration.ClientCredentialsSettings;

            var confidentialClientApp = ConfidentialClientApplicationBuilder
                .Create(teamClientId)
                .WithClientSecret(teamClientSecret)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{_chargesFunctionAppFixture.AuthorizationConfiguration.B2cTenantId}"))
                .Build();

            return confidentialClientApp;
        }
    }
}
