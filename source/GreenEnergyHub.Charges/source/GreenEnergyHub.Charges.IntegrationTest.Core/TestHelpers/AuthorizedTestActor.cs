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
using System.Threading.Tasks;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration.B2C;
using Microsoft.Identity.Client;

namespace GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers
{
    public class AuthorizedTestActor
    {
        private readonly string _localTimeZoneName;

        public B2CClientAppSettings B2CClientAppSettings { get; }

        private AuthenticationResult? AuthenticationResult { get; set; }

        public AuthorizedTestActor(B2CClientAppSettings b2cClientAppSettings, string localTimeZoneName)
        {
            ArgumentNullException.ThrowIfNull(b2cClientAppSettings);
            if (string.IsNullOrWhiteSpace(localTimeZoneName))
                throw new ArgumentException($"'{nameof(localTimeZoneName)}' cannot be null or whitespace.", nameof(localTimeZoneName));

            B2CClientAppSettings = b2cClientAppSettings;
            _localTimeZoneName = localTimeZoneName;
        }

        public async Task AddAuthenticationAsync(string b2cTenantId, B2CAppSettings backendAppSettings)
        {
            var backendAuthenticationClient = new B2CAppAuthenticationClient(
                b2cTenantId,
                backendAppSettings,
                B2CClientAppSettings);

            AuthenticationResult = await backendAuthenticationClient.GetAuthenticationTokenAsync();
        }

        public (HttpRequestMessage Request, string CorrelationId) PrepareHttpPostRequestWithAuthorization(
            string endpointUrl,
            string testFilePath)
        {
            ArgumentNullException.ThrowIfNull(endpointUrl);
            ArgumentNullException.ThrowIfNull(testFilePath);
            ArgumentNullException.ThrowIfNull(AuthenticationResult);
            ArgumentNullException.ThrowIfNull(_localTimeZoneName);

            var (request, correlationId) =
                HttpRequestGenerator.CreateHttpPostRequestWithAuthorization(
                    endpointUrl,
                    testFilePath,
                    AuthenticationResult.AccessToken,
                    _localTimeZoneName);

            return (request, correlationId);
        }
    }
}
