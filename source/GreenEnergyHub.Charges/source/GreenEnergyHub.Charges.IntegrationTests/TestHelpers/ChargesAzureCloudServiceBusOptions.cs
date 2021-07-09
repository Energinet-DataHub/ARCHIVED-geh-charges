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
using System.Diagnostics.CodeAnalysis;
using Squadron;
using Squadron.AzureCloud;

namespace GreenEnergyHub.Charges.IntegrationTests.TestHelpers
{
    public class ChargesAzureCloudServiceBusOptions : AzureCloudServiceBusOptions
    {
        public const string ReceivedTopicName = "sbt-received";
        public const string SubscriptionName = "sbs-received";

        public override void Configure([NotNull] ServiceBusOptionsBuilder builder)
        {
            FunctionHostConfigurationHelper.ConfigureEnvironmentVariables();

            var projectName = Environment.GetEnvironmentVariable("PROJECT_NAME") ?? string.Empty;
            var organisationName = Environment.GetEnvironmentVariable("ORGANISATION_NAME") ?? string.Empty;
            var environmentShort = Environment.GetEnvironmentVariable("ENVIRONMENT_SHORT") ?? string.Empty;
            var serviceBusNamespace = $"sbn-{projectName}-{organisationName}-{environmentShort}";

            builder.SetConfigResolver(ChargesAzureResourceConfigurationResolver);

            builder
                .Namespace(serviceBusNamespace)
                .AddTopic(ReceivedTopicName)
                .AddSubscription(SubscriptionName);
        }

        private AzureResourceConfiguration ChargesAzureResourceConfigurationResolver()
        {
            var secret = Environment.GetEnvironmentVariable("CLIENT_SECRET") ?? string.Empty;
            var clientId = Environment.GetEnvironmentVariable("CLIENT_ID") ?? string.Empty;
            var tenantId = Environment.GetEnvironmentVariable("TENANT_ID") ?? string.Empty;
            var defaultLocation = Environment.GetEnvironmentVariable("DEFAULT_LOCATION") ?? string.Empty;
            var resourceGroup = Environment.GetEnvironmentVariable("RESOURCE_GROUP_NAME") ?? string.Empty;
            var subscriptionId = Environment.GetEnvironmentVariable("SUBSCRIPTION_ID") ?? string.Empty;

            if (string.IsNullOrWhiteSpace(secret)
                || string.IsNullOrWhiteSpace(clientId)
                || string.IsNullOrWhiteSpace(tenantId)
                || string.IsNullOrWhiteSpace(defaultLocation)
                || string.IsNullOrWhiteSpace(resourceGroup)
                || string.IsNullOrWhiteSpace(subscriptionId))
            {
                var errormessage = $"{nameof(ChargesAzureCloudServiceBusOptions)} Missing configuration: " +
                                   $"Length of {nameof(secret)}: {secret.Length}," +
                                   $"Length of {nameof(clientId)}: {clientId.Length}," +
                                   $"Length of {nameof(tenantId)}: {tenantId.Length}," +
                                   $"Length of {nameof(subscriptionId)}: {subscriptionId.Length}," +
                                   $"Default location: {defaultLocation}, {resourceGroup}";

                throw new ArgumentException(errormessage);
            }

            var azureResourceConfiguration = new AzureResourceConfiguration
            {
                Credentials = new AzureCredentials { Secret = secret, ClientId = clientId, TenantId = tenantId },
                DefaultLocation = defaultLocation,
                ResourceGroup = resourceGroup,
                SubscriptionId = subscriptionId,
            };

            return azureResourceConfiguration;
        }
    }
}
