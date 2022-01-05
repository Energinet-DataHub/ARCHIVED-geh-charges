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
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Microsoft.Extensions.Configuration;

namespace GreenEnergyHub.Charges.SystemTests.Fixtures
{
    public class SystemTestConfiguration
    {
        public SystemTestConfiguration()
        {
            var configuration = BuildConfiguration();

            BaseAddress = new Uri(configuration.GetValue<string>("MYDOMAIN_BASEADDRESS"));

            var keyVaultUrl = configuration.GetValue<string>("AZURE_SYSTEMTESTS_KEYVAULT_URL");
            var keyVaultConfiguration = BuildKeyVaultConfigurationRoot(keyVaultUrl);

            ApiManagementBaseAddress = new Uri("https://apim-shared-sharedres-u-001.azure-api.net");
            B2CSettings = RetrieveB2CSettings(configuration, keyVaultConfiguration);
        }

        public Uri BaseAddress { get; }

        public B2CSettings B2CSettings { get; }

        public Uri ApiManagementBaseAddress { get; }

        /// <summary>
        /// Load settings from file if available, but also allow
        /// those settings to be overriden using environment variables.
        /// </summary>
        private static IConfigurationRoot BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("systemtest.local.settings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        /// <summary>
        /// Load settings from key vault.
        /// </summary>
        private static IConfigurationRoot BuildKeyVaultConfigurationRoot(string keyVaultUrl)
        {
            return new ConfigurationBuilder()
                .AddAuthenticatedAzureKeyVault(keyVaultUrl)
                .Build();
        }

        private static B2CSettings RetrieveB2CSettings(IConfigurationRoot configuration, IConfigurationRoot keyVaultConfiguration)
        {
            var environment =
                configuration.GetValue<string>("ENVIRONMENT_SHORT") +
                configuration.GetValue<string>("ENVIRONMENT_INSTANCE");

            // TODO: We might need to support specifying this from each test, if developers want to test using different "clients" and permissions (roles)
            // TODO: Why do we have a client per team; would it not be more usefull for different scenarious if we just had clients with different permissions that we could share between all teams?
            var team = configuration.GetValue<string>("TEAM_NAME");

            var b2cTenantId = "4a7411ea-ac71-4b63-9647-b8bd4c5a20e0";
            var backendAppId = "c7e5dc5c-2ee0-420c-b5d2-586e7527302c";
            var teamClientId = keyVaultConfiguration.GetValue<string>(BuildTeamSecretName(environment, team, "client-id"));
            var teamClientSecret = keyVaultConfiguration.GetValue<string>(BuildTeamSecretName(environment, team, "client-secret"));

            return new B2CSettings(b2cTenantId, backendAppId, teamClientId, teamClientSecret);
        }

        private static string BuildTeamSecretName(string environment, string team, string secret)
        {
            return $"B2C-{environment}-{team}-{secret}";
        }
    }
}
