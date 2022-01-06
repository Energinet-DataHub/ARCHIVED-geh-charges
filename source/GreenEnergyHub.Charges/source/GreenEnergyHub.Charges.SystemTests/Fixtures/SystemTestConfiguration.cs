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

            Environment =
                configuration.GetValue<string>("ENVIRONMENT_SHORT") +
                configuration.GetValue<string>("ENVIRONMENT_INSTANCE");

            var keyVaultUrl = configuration.GetValue<string>("AZURE_SYSTEMTESTS_KEYVAULT_URL");
            KeyVaultConfiguration = BuildKeyVaultConfigurationRoot(keyVaultUrl);

            ApiManagementBaseAddress = KeyVaultConfiguration.GetValue<Uri>(BuildApiManagementEnvironmentSecretName(Environment, "host-url"));
        }

        /// <summary>
        /// Environment short name with instance indication.
        /// </summary>
        public string Environment { get; }

        /// <summary>
        /// An example of a base address setting (e.g. for a domain service).
        /// </summary>
        public Uri BaseAddress { get; }

        /// <summary>
        /// The base address for the API Management in the configured environment.
        /// </summary>
        public Uri ApiManagementBaseAddress { get; }

        private IConfigurationRoot KeyVaultConfiguration { get; }

        /// <summary>
        /// Retrieve B2C settings necessary for aquiring an access token for a given "team client" in the configured environment.
        /// </summary>
        /// <param name="team">Team name or shorthand.</param>
        /// <returns>B2C settings for "team client"</returns>
        public B2CSettings RetrieveB2CSettings(string team)
        {
            if (string.IsNullOrWhiteSpace(team))
                throw new ArgumentException($"'{nameof(team)}' cannot be null or whitespace.", nameof(team));

            var b2cTenantId = KeyVaultConfiguration.GetValue<string>(BuildB2CEnvironmentSecretName(Environment, "tenant-id"));
            var backendAppId = KeyVaultConfiguration.GetValue<string>(BuildB2CEnvironmentSecretName(Environment, "backend-app-id"));
            var teamClientId = KeyVaultConfiguration.GetValue<string>(BuildB2CTeamSecretName(Environment, team, "client-id"));
            var teamClientSecret = KeyVaultConfiguration.GetValue<string>(BuildB2CTeamSecretName(Environment, team, "client-secret"));

            return new B2CSettings(b2cTenantId, backendAppId, teamClientId, teamClientSecret);
        }

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

        private static string BuildApiManagementEnvironmentSecretName(string environment, string secret)
        {
            return $"APIM-{environment}-{secret}";
        }

        private static string BuildB2CTeamSecretName(string environment, string team, string secret)
        {
            return $"B2C-{environment}-{team}-{secret}";
        }

        private static string BuildB2CEnvironmentSecretName(string environment, string secret)
        {
            return $"B2C-{environment}-{secret}";
        }
    }
}
