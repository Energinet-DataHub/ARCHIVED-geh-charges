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
using GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers;
using Microsoft.Extensions.Configuration;

namespace GreenEnergyHub.Charges.IntegrationTest.Core.Authorization
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
        public AuthorizationConfiguration(
            IEnumerable<string> clientNames,
            string environment,
            string localSettingsJsonFilename,
            string azureSecretsKeyVaultUrlKey)
        {
            // Team name and environment is required to get client-id and client-secret for integration tests
            Environment = environment;
            RootConfiguration = BuildKeyVaultConfigurationRoot(localSettingsJsonFilename);
            SecretsConfiguration = BuildSecretsKeyVaultConfiguration(RootConfiguration.GetValue<string>(azureSecretsKeyVaultUrlKey));
            TestClients = CreateTestClients(clientNames);
            B2cTenantId = SecretsConfiguration.GetValue<string>(BuildB2CEnvironmentSecretName(Environment, "tenant-id"));
            var backendAppId = SecretsConfiguration.GetValue<string>(BuildB2CEnvironmentSecretName(Environment, "backend-app-id"));
            var frontendAppId = SecretsConfiguration.GetValue<string>(BuildB2CEnvironmentSecretName(Environment, "frontend-app-id"));
            BackendAppScope = new[] { $"{backendAppId}/.default" };
            FrontendAppScope = new[] { $"{frontendAppId}/.default" };

            BackendAppId = SecretsConfiguration.GetValue<string>(BuildB2CBackendAppId(Environment));
            FrontendAppId = SecretsConfiguration.GetValue<string>(BuildB2CFrontendAppId(Environment));

            ApiManagementBaseAddress = SecretsConfiguration.GetValue<Uri>(BuildApiManagementEnvironmentSecretName(Environment, "host-url"));
            FrontendOpenIdUrl = SecretsConfiguration.GetValue<string>(BuildB2CFrontendOpenIdUrl(Environment));
        }

        public IEnumerable<TestClient> TestClients { get; }

        public IEnumerable<string> FrontendAppScope { get; }

        public string FrontendOpenIdUrl { get; }

        public string FrontendAppId { get; }

        /// <summary>
        /// Backend application ID
        /// </summary>
        public string BackendAppId { get; }

        public IConfigurationRoot RootConfiguration { get; }

        /// <summary>
        /// Can be used to extract secrets from the Key Vault.
        /// </summary>
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

        /// <summary>
        /// Create a list of 'test client apps' each with own settings necessary to acquire an access token in a configured environment.
        /// </summary>
        /// <param name="clientNames">List of team names or shorthands</param>
        /// <returns>A list of test clients apps</returns>
        /// <exception cref="ArgumentException">When string value is null or whitespace</exception>
        private IEnumerable<TestClient> CreateTestClients(IEnumerable<string> clientNames)
        {
            ArgumentNullException.ThrowIfNull(clientNames);

            var testClients = new List<TestClient>();

            foreach (var clientName in clientNames)
            {
                if (string.IsNullOrEmpty(clientName))
                    throw new ArgumentException($"'{nameof(clientName)}' cannot be null or whitespace.", nameof(clientName));

                testClients.Add(new TestClient(
                    clientName,
                    SecretsConfiguration.GetValue<string>(BuildB2CTeamSecretName(Environment, clientName, "client-id")),
                    SecretsConfiguration.GetValue<string>(BuildB2CTeamSecretName(Environment, clientName, "client-secret"))));
            }

            return testClients;
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

        private static IConfigurationRoot BuildKeyVaultConfigurationRoot(string localSettingsJsonFilename)
        {
            return new ConfigurationBuilder()
                .AddJsonFile(localSettingsJsonFilename, optional: true)
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

        private static string BuildB2CFrontendOpenIdUrl(string environment)
        {
            return $"B2C-{environment}-frontend-open-id-url";
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
