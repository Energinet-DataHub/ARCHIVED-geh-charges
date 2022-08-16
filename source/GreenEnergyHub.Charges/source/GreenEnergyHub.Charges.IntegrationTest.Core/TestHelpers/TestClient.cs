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
using GreenEnergyHub.Charges.IntegrationTest.Core.Authorization;

namespace GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers
{
    public class TestClient
    {
        public TestClient(
            string clientName,
            string teamClientId,
            string teamClientSecret)
        {
            ClientName = clientName;
            TeamClientId = teamClientId;
            TeamClientSecret = teamClientSecret;
            ClientCredentialsSettings = RetrieveB2CTestClientSettings(clientName, teamClientId, teamClientSecret);
        }

        /// <summary>
        /// Retrieve B2C test client settings necessary for acquiring an access token for a given 'test client app' in the configured environment.
        /// </summary>
        /// <param name="clientName">Team name or shorthand.</param>
        /// <param name="clientId">Client ID</param>
        /// <param name="clientSecret">Client secret</param>
        /// <returns>Settings for 'test client app'</returns>
        public static ClientCredentialsSettings RetrieveB2CTestClientSettings(string clientName, string clientId, string clientSecret)
        {
            if (string.IsNullOrWhiteSpace(clientName))
                throw new ArgumentException($"'{nameof(clientName)}' cannot be null or whitespace.", nameof(clientName));

            return new ClientCredentialsSettings(clientId, clientSecret);
        }

        public string ClientName { get; }

        public string TeamClientId { get; }

        public string TeamClientSecret { get; }

        public ClientCredentialsSettings ClientCredentialsSettings { get; }
    }
}
