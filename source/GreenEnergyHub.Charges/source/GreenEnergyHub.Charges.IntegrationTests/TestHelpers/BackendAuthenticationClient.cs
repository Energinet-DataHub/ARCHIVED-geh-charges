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
using System.Collections.Generic;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.IntegrationTests.Authorization;
using Microsoft.Identity.Client;

namespace GreenEnergyHub.Charges.IntegrationTests.TestHelpers
{
    public class BackendAuthenticationClient
    {
        private readonly IEnumerable<string> _backendAppScope;
        private readonly ClientCredentialsSettings _clientCredentialsSettings;
        private readonly string _b2CTenantId;

        public BackendAuthenticationClient(
            IEnumerable<string> backendAppScope,
            ClientCredentialsSettings clientCredentialsSettings,
            string b2CTenantId)
        {
            _backendAppScope = backendAppScope;
            _clientCredentialsSettings = clientCredentialsSettings;
            _b2CTenantId = b2CTenantId;
        }

        public async Task<AuthenticationResult> GetAuthenticationTokenAsync()
        {
            var confidentialClientApp = CreateConfidentialClientApp();
            var result = await confidentialClientApp.AcquireTokenForClient(_backendAppScope)
                .ExecuteAsync().ConfigureAwait(false);
            return result;
        }

        private IConfidentialClientApplication CreateConfidentialClientApp()
        {
            var (teamClientId, teamClientSecret) = _clientCredentialsSettings;

            var confidentialClientApp = ConfidentialClientApplicationBuilder
                .Create(teamClientId)
                .WithClientSecret(teamClientSecret)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{_b2CTenantId}"))
                .Build();

            return confidentialClientApp;
        }
    }
}
