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

using GreenEnergyHub.Charges.IntegrationTest.Core.Authorization;

namespace GreenEnergyHub.Charges.IntegrationTest.Core.TestHelpers
{
    public static class AuthorizationConfigurationData
    {
        public const string Environment = "u001";

        public const string SystemOperator = "endk-tso";
        public const string GridAccessProvider8100000000030 = "volt";

        private const string LocalSettingsJsonFilename = "integrationtest.local.settings.json";
        private const string AzureSecretsKeyVaultUrlKey = "AZURE_SECRETS_KEYVAULT_URL";

        public static AuthorizationConfiguration CreateAuthorizationConfiguration(string clientName)
        {
            return new AuthorizationConfiguration(
                clientName,
                Environment,
                LocalSettingsJsonFilename,
                AzureSecretsKeyVaultUrlKey);
        }
    }
}
