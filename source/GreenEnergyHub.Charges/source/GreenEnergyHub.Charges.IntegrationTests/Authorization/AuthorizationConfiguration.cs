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
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Microsoft.Extensions.Configuration;

namespace GreenEnergyHub.Charges.IntegrationTests.Authorization
{
    /// <summary>
    /// Responsible for extracting secrets for authorization needed for performing endpoint tests.
    ///
    /// On developer machines we use the 'integrationtest.local.settings.json' to set values.
    /// On hosted agents we must set these using environment variables.
    ///
    /// Developers, and the service principal under which the tests are executed, must have access
    /// to the Key Vault so secrets can be extracted.
    /// </summary>
    public class AuthorizationConfiguration
    {
        public AuthorizationConfiguration()
        {
            // Team name and environment is required to get client-id and client-secret for integration tests
            const string teamName = "volt";
            Environment = "u002";
            RootConfiguration = BuildKeyVaultConfigurationRoot();
            SecretsConfiguration = BuildSecretsKeyVaultConfiguration(RootConfiguration.GetValue<string>("AZURE_SECRETS_KEYVAULT_URL"));
            B2cTenantId = SecretsConfiguration.GetValue<string>(BuildB2CEnvironmentSecretName(Environment, "tenant-id"));
            var backendAppId = SecretsConfiguration.GetValue<string>(BuildB2CEnvironmentSecretName(Environment, "backend-app-id"));
            BackendAppScope = new[] { $"{backendAppId}/.default" };

            BackendAppId = SecretsConfiguration.GetValue<string>(BuildB2CBackendAppId(Environment));
            FrontendAppId = SecretsConfiguration.GetValue<string>(BuildB2CFrontendAppId(Environment));
            var teamClientId = SecretsConfiguration.GetValue<string>(BuildB2CTeamSecretName(Environment, teamName, "client-id"));
            var teamClientSecret = SecretsConfiguration.GetValue<string>(BuildB2CTeamSecretName(Environment, teamName, "client-secret"));

            ClientCredentialsSettings = RetrieveB2CTeamClientSettings(teamName, teamClientId, teamClientSecret);

            ApiManagementBaseAddress = SecretsConfiguration.GetValue<Uri>(BuildApiManagementEnvironmentSecretName(Environment, "host-url"));
        }

        public string FrontendAppId { get; }

        /// <summary>
        /// Backend application ID
        /// </summary>
        public string BackendAppId { get; }

        public IConfigurationRoot RootConfiguration { get; }

        public IConfigurationRoot SecretsConfiguration { get; }

        /// <summary>
        /// Environment short name with instance indication.
        /// </summary>
        public string Environment { get; }

        /// <summary>
        /// The B2C tenant id in the configured environment.
        /// </summary>
        public string B2cTenantId { get; }

        /// <summary>
        /// The scope for which we must request an access token, to be authorized by the API Management.
        /// </summary>
        public IEnumerable<string> BackendAppScope { get; }

        /// <summary>
        /// The base address for the API Management in the configured environment.
        /// </summary>
        public Uri ApiManagementBaseAddress { get; }

        public ClientCredentialsSettings ClientCredentialsSettings { get; }

        // /// <summary>
        // /// Can be used to extract secrets from the Key Vault.
        // /// </summary>
        // private IConfigurationRoot KeyVaultConfiguration { get; }

        /// <summary>
        /// Retrieve B2C team client settings necessary for aquiring an access token for a given 'team client app' in the configured environment.
        /// </summary>
        /// <param name="team">Team name or shorthand.</param>
        /// <param name="clientId">Client ID</param>
        /// <param name="clientSecret">Client secret</param>
        /// <returns>Settings for 'team client app'</returns>
        public static ClientCredentialsSettings RetrieveB2CTeamClientSettings(string team, string clientId, string clientSecret)
        {
            if (string.IsNullOrWhiteSpace(team))
                throw new ArgumentException($"'{nameof(team)}' cannot be null or whitespace.", nameof(team));

            return new ClientCredentialsSettings(clientId, clientSecret);
        }

        /// <summary>
        /// Load settings from key vault.
        /// </summary>
        private static IConfigurationRoot BuildSecretsKeyVaultConfiguration(string keyVaultUrl)
        {
            return new ConfigurationBuilder()
                .AddAuthenticatedAzureKeyVault(keyVaultUrl)
                .Build();
        }

        private static IConfigurationRoot BuildKeyVaultConfigurationRoot()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("integrationtest.local.settings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        private static string BuildApiManagementEnvironmentSecretName(string environment, string secret)
        {
            return $"APIM-{environment}-{secret}";
        }

        private static string BuildB2CBackendAppId(string environment)
        {
            return $"B2C-{environment}-backend-app-id";
        }

        private static string BuildB2CFrontendAppId(string environment)
        {
            return $"B2C-{environment}-frontend-app-id";
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
