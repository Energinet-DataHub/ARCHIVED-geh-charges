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
using System.Text.RegularExpressions;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using GreenEnergyHub.Charges.FunctionHost.Common;
using Squadron;
using Squadron.AzureCloud;

namespace GreenEnergyHub.Charges.IntegrationTests.Fixtures
{
    public class ChargesFunctionAppServiceBusOptions : AzureCloudServiceBusOptions
    {
        public const string PostOfficeTopicKey = "sbt-post-office";
        public const string PostOfficeSubscriptionName = "defaultSubscription";

        public const string ChargeLinkAcceptedTopicKey = "sbt-link-command-accepted";
        public const string ChargeLinkAcceptedDataAvailableNotifierSubscriptionName = "sbs-chargelinkaccepted-sub-dataavailablenotifier";
        public const string ChargeLinkAcceptedEventPublisherSubscriptionName = "sbs-chargelinkaccepted-sub-eventpublisher";
        public const string ChargeLinkAcceptedEventReplierSubscriptionName = "sbs-chargelinkaccepted-sub-replier";

        public const string ChargeLinkCreatedTopicKey = "charge-link-created";

        public const string ChargeLinkReceivedTopicKey = "sbt-link-command-received";
        public const string ChargeLinkReceivedSubscriptionName = "sbs-link-command-received-receiver";

        public const string CommandAcceptedTopicKey = "sbt-command-accepted";
        public const string CommandAcceptedSubscriptionName = "sbs-command-accepted";
        public const string CommandAcceptedReceiverSubscriptionName = "sbs-charge-command-accepted-receiver";

        public const string CommandReceivedTopicKey = "sbt-command-received";
        public const string CommandReceivedSubscriptionName = "sbs-command-received";

        public const string CommandRejectedTopicKey = "sbt-command-rejected";
        public const string CommandRejectedSubscriptionName = "sbs-command-rejected";

        public const string CreateLinkRequestQueueKey = "create-link-request";

        public const string CreateLinkReplyQueueKey = "create-link-reply";

        public const string ConsumptionMeteringPointCreatedTopicKey = "consumption-metering-point-created";
        public const string ConsumptionMeteringPointCreatedSubscriptionName = "consumption-metering-point-created-sub-charges";

        public const string ChargeCreatedTopicKey = "charge-created";

        public const string ChargePricesUpdatedTopicKey = "charge-prices-updated";

        public const string MessageHubDataAvailableQueueKey = "message-hub-data-available";
        public const string MessageHubRequestQueueKey = "message-hub-request";
        public const string MessageHubReplyQueueKey = "message-hub-reply";
        public const string MessageHubStorageConnectionString = "UseDevelopmentStorage=true";
        public const string MessageHubStorageContainerName = "messagehub-bundles";

        public override void Configure(ServiceBusOptionsBuilder builder)
        {
            builder.SetConfigResolver(ConfigurationResolver);

            // We extract the service bus namespace from one of the connection strings
            var serviceBusNamespace = GetNamespaceFromSetting(EnvironmentSettingNames.DomainEventSenderConnectionString);

            builder.Namespace(serviceBusNamespace);

            builder
                .AddTopic(PostOfficeTopicKey)
                .AddSubscription(PostOfficeSubscriptionName);

            builder
                .AddTopic(ChargeLinkAcceptedTopicKey)
                .AddSubscription(ChargeLinkAcceptedDataAvailableNotifierSubscriptionName)
                .AddSubscription(ChargeLinkAcceptedEventPublisherSubscriptionName)
                .AddSubscription(ChargeLinkAcceptedEventReplierSubscriptionName);

            builder.AddTopic(ChargeLinkCreatedTopicKey);

            builder
                .AddTopic(ChargeLinkReceivedTopicKey)
                .AddSubscription(ChargeLinkReceivedSubscriptionName);

            builder
                .AddTopic(CommandAcceptedTopicKey)
                .AddSubscription(CommandAcceptedSubscriptionName)
                .AddSubscription(CommandAcceptedReceiverSubscriptionName);

            builder
                .AddTopic(CommandReceivedTopicKey)
                .AddSubscription(CommandReceivedSubscriptionName);

            builder
                .AddTopic(CommandRejectedTopicKey)
                .AddSubscription(CommandRejectedSubscriptionName);

            builder.AddQueue(CreateLinkRequestQueueKey);

            builder.AddQueue(CreateLinkReplyQueueKey);

            builder
                .AddTopic(ConsumptionMeteringPointCreatedTopicKey)
                .AddSubscription(ConsumptionMeteringPointCreatedSubscriptionName);

            builder.AddTopic(ChargeCreatedTopicKey);

            builder.AddTopic(ChargePricesUpdatedTopicKey);

            builder.AddQueue(MessageHubDataAvailableQueueKey);
            builder.AddQueue(MessageHubRequestQueueKey);
            builder.AddQueue(MessageHubReplyQueueKey);
        }

        private static string GetNamespaceFromSetting(string settingName)
        {
            var localSettingsSnapshot = new FunctionAppHostConfigurationBuilder().BuildLocalSettingsConfiguration();
            var domainEventListenerConnectionString = localSettingsSnapshot.GetValue(settingName);

            // Example connection string: 'Endpoint=sb://xxx.servicebus.windows.net/;'
            var namespaceMatchPattern = @"Endpoint=sb://(.*?).servicebus.windows.net/";
            var match = Regex.Match(domainEventListenerConnectionString, namespaceMatchPattern, RegexOptions.IgnoreCase);
            var domainEventListenerNamespace = match.Groups[1].Value;
            return domainEventListenerNamespace;
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
                    $"{nameof(ChargesFunctionAppServiceBusOptions)} Missing configuration: " +
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
        /// EnvironmentVariables are not automatically loaded when running xUnit integrationstests.
        /// This method follows the suggested workaround mentioned here:
        /// https://github.com/Azure/azure-functions-host/issues/6953
        /// </summary>
        private static void ConfigureEnvironmentVariables()
        {
            var path = Path.GetDirectoryName(typeof(ChargesFunctionAppServiceBusOptions).Assembly.Location);
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
