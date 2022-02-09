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
using System.Threading.Tasks;
using GreenEnergyHub.Charges.IntegrationTests.Authorization;

namespace GreenEnergyHub.Charges.IntegrationTests.TestHelpers
{
    public class AuthenticatedHttpRequestGenerator
    {
        private readonly HttpRequestGenerator _httpRequestGenerator;
        private readonly BackendAuthenticationClient _backendAuthenticationClient;

        public AuthenticatedHttpRequestGenerator(
            HttpRequestGenerator httpRequestGenerator,
            AuthorizationConfiguration authorizationConfiguration)
        {
            _httpRequestGenerator = httpRequestGenerator;
            _backendAuthenticationClient = new BackendAuthenticationClient(
                authorizationConfiguration.BackendAppScope,
                authorizationConfiguration.ClientCredentialsSettings,
                authorizationConfiguration.B2cTenantId);
        }

        public async Task<(HttpRequestMessage Request, string CorrelationId)> CreateAuthenticatedHttpPostRequestAsync(
            string endpointUrl, string testFilePath)
        {
            var (request, correlationId) = _httpRequestGenerator.CreateHttpPostRequest(endpointUrl, testFilePath);

            await AddAuthenticationAsync(request);

            return (request, correlationId);
        }

        private async Task AddAuthenticationAsync(HttpRequestMessage request)
        {
            var authenticationResult = await _backendAuthenticationClient.GetAuthenticationTokenAsync();
            request.Headers.Add("Authorization", $"Bearer {authenticationResult.AccessToken}");
        }
    }
}
