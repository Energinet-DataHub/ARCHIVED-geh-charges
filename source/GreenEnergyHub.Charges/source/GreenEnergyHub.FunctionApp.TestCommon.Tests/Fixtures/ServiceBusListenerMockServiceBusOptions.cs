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
using GreenEnergyHub.Charges.IntegrationTests.TestHelpers;
using Squadron;
using Squadron.AzureCloud;

namespace GreenEnergyHub.FunctionApp.TestCommon.Tests.Fixtures
{
    public class ServiceBusListenerMockServiceBusOptions : AzureCloudServiceBusOptions
    {
        public override void Configure(ServiceBusOptionsBuilder builder)
        {
            builder.SetConfigResolver(ConfigurationResolver);

            // TODO: Maybe use another namespace on build agent?
            ////var serviceBusNamespace = "integrationtest-sb-dev-datahub";
            ////builder.Namespace(serviceBusNamespace);

            var queueName = "queue";
            builder
                .AddQueue(queueName);

            var topicName = "topic";
            builder
                .AddTopic(topicName)
                .AddSubscription("defaultSubscription");
        }

        private AzureResourceConfiguration ConfigurationResolver()
        {
            // TODO: Bad dependency/reference !!! (<ProjectReference Include="..\GreenEnergyHub.Charges.IntegrationTests\GreenEnergyHub.Charges.IntegrationTests.csproj" />)
            FunctionHostConfigurationHelper.ConfigureEnvironmentVariables();

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
                var errormessage =
                    $"{nameof(ServiceBusListenerMockServiceBusOptions)} Missing configuration: " +
                    $"Length of {nameof(secret)}: {secret.Length}," +
                    $"Length of {nameof(clientId)}: {clientId.Length}," +
                    $"Length of {nameof(tenantId)}: {tenantId.Length}," +
                    $"Length of {nameof(subscriptionId)}: {subscriptionId.Length}," +
                    $"Default location: {defaultLocation}, {resourceGroup}";

                throw new InvalidOperationException(errormessage);
            }

            var azureResourceConfiguration = new AzureResourceConfiguration
            {
                Credentials = new AzureCredentials
                {
                    Secret = secret,
                    ClientId = clientId,
                    TenantId = tenantId,
                },
                DefaultLocation = defaultLocation,
                ResourceGroup = resourceGroup,
                SubscriptionId = subscriptionId,
            };

            return azureResourceConfiguration;
        }
    }
}
