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

using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.IntegrationTests.Authorization;
using NodaTime;

namespace GreenEnergyHub.Charges.IntegrationTests.TestHelpers
{
    public class HttpRequestGenerator
    {
        private readonly AuthenticationClient _authenticationClient;

        public HttpRequestGenerator(AuthorizationConfiguration authorizationConfiguration)
        {
            _authenticationClient = new AuthenticationClient(
                authorizationConfiguration.BackendAppScope,
                authorizationConfiguration.ClientCredentialsSettings,
                authorizationConfiguration.B2cTenantId);
        }

        public async Task<(HttpRequestMessage Request, string CorrelationId)> CreateHttpPostRequestAsync(
            string endpointUrl, string testFilePath)
        {
            var clock = SystemClock.Instance;
            var chargeXml = EmbeddedResourceHelper.GetEmbeddedFile(testFilePath, clock);
            var correlationId = CorrelationIdGenerator.Create();

            var request = new HttpRequestMessage(HttpMethod.Post, endpointUrl)
            {
                Content = new StringContent(chargeXml, Encoding.UTF8, "application/xml"),
            };
            request.ConfigureTraceContext(correlationId);

            var authenticationResult = await _authenticationClient.GetAuthenticationTokenAsync();
            request.Headers.Add("Authorization", $"Bearer {authenticationResult.AccessToken}");

            return (request, correlationId);
        }

        public async Task<(HttpRequestMessage Request, string CorrelationId)> CreateHttpGetRequestAsync(
            string endpointUrl)
        {
            var correlationId = CorrelationIdGenerator.Create();
            var request = new HttpRequestMessage(HttpMethod.Get, endpointUrl);
            request.ConfigureTraceContext(correlationId);

            var authenticationResult = await _authenticationClient.GetAuthenticationTokenAsync();
            request.Headers.Add("Authorization", $"Bearer {authenticationResult.AccessToken}");

            return (request, correlationId);
        }
    }
}
