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
using System.IO;
using Squadron;
using Squadron.AzureCloud;

namespace GreenEnergyHub.FunctionApp.TestCommon.Tests.Fixtures
{
    public class ServiceBusListenerMockServiceBusOptions : AzureCloudServiceBusOptions
    {
        public const string QueueName = "queue";

        public const string TopicName = "topic";
        public const string SubscriptionName = "defaultSubscription";

        public override void Configure(ServiceBusOptionsBuilder builder)
        {
            builder.SetConfigResolver(ConfigurationResolver);

            builder
                .AddQueue(QueueName);

            builder
                .AddTopic(TopicName)
                .AddSubscription(SubscriptionName);
        }

        private AzureResourceConfiguration ConfigurationResolver()
        {
            ConfigureEnvironmentVariables();

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

        /// <summary>
        /// EnvironmentVariables are not automatically loaded when running XUnit integrationstests.
        /// This method follows the suggested workaround mentioned here:
        /// https://github.com/Azure/azure-functions-host/issues/6953
        /// </summary>
        private static void ConfigureEnvironmentVariables()
        {
            var path = Path.GetDirectoryName(typeof(ServiceBusListenerMockServiceBusOptions).Assembly.Location);
            var settingsFile = Path.Join(path, "integrationtest.local.settings.json");
            if (!File.Exists(settingsFile))
                return;

            var json = File.ReadAllText(settingsFile);
            var parsed = Newtonsoft.Json.Linq.JObject.Parse(json).Value<Newtonsoft.Json.Linq.JObject>("Values");

            foreach (var item in parsed!)
            {
                Environment.SetEnvironmentVariable(item.Key, item.Value?.ToString());
            }
        }
    }
}
