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
    public class B2CTestClient
    {
        public B2CTestClient(
            string clientName,
            string teamClientId,
            string teamClientSecret)
        {
            if (string.IsNullOrWhiteSpace(clientName))
                throw new ArgumentException($"'{nameof(clientName)}' cannot be null or whitespace.", nameof(clientName));
            if (string.IsNullOrWhiteSpace(teamClientId))
                throw new ArgumentException($"'{nameof(teamClientId)}' cannot be null or whitespace.", nameof(teamClientId));
            if (string.IsNullOrWhiteSpace(teamClientSecret))
                throw new ArgumentException($"'{nameof(teamClientSecret)}' cannot be null or whitespace.", nameof(teamClientSecret));

            ClientName = clientName;
            ClientCredentialsSettings = new ClientCredentialsSettings(teamClientId, teamClientSecret);
        }

        /// <summary>
        /// B2C test client settings necessary for acquiring an access token for a given 'test client app' in the configured environment.
        /// </summary>
        public ClientCredentialsSettings ClientCredentialsSettings { get; }

        public string ClientName { get; }
    }
}
