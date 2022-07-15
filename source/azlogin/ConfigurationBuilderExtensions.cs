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
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace Azlogin
{
    public static class ConfigurationBuilderExtensions
    {
        /// <summary>
        /// Adds an <see cref="IConfigurationProvider"/> that reads configuration values from the Azure KeyVault.
        ///
        /// Using <see cref="DefaultAzureCredential"/> it automatically requests access tokens for reading
        /// from the Key Vault. For this to work the identity under which the tests are executied, must have
        /// Get and List permissions to secrets in the Key Vault.
        /// </summary>
        /// <param name="builder">The configuration builder.</param>
        /// <param name="keyVaultUrl">KeyVault URL eg. https://myexamplekeyvault.vault.azure.net/</param>
        public static IConfigurationBuilder AddAuthenticatedAzureKeyVault(this IConfigurationBuilder builder, string keyVaultUrl, bool excludeManagedIdentityCredential = false)
        {
            if (string.IsNullOrEmpty(keyVaultUrl))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(keyVaultUrl));
            }

            var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ExcludeManagedIdentityCredential = excludeManagedIdentityCredential,
            });
            builder.AddAzureKeyVault(new Uri(keyVaultUrl), credential);

            return builder;
        }
    }
}
