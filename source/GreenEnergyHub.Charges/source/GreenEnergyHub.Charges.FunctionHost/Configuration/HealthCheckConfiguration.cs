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

using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.FunctionApp.Diagnostics.HealthChecks;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.Infrastructure.Core.Registration;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.FunctionHost.Configuration
{
    internal static class HealthCheckConfiguration
    {
        internal static void ConfigureServices(IServiceCollection serviceCollection)
        {
            // Health check
            serviceCollection.AddScoped<IHealthCheckEndpointHandler, HealthCheckEndpointHandler>();
            serviceCollection.AddHealthChecks()
                .AddLiveCheck()
                .AddSqlServer(
                    name: "ChargeDb",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeDbConnectionString));

            // Integration events
            ConfigureIntegrationEventsCharges(serviceCollection);
            ConfigureIntegrationEventsChargeLinks(serviceCollection);
            ConfigureIntegrationEventsMeteringPointDomain(serviceCollection);
            ConfigureIntegrationEventsMessageHub(serviceCollection);

            // Domain events
            ConfigureDomainEventsCharges(serviceCollection);
            ConfigureDomainEventsChargeLinks(serviceCollection);
            ConfigureDomainEventsDefaultChargeLinks(serviceCollection);
        }

        private static void ConfigureIntegrationEventsCharges(IServiceCollection serviceCollection)
        {
            serviceCollection.AddHealthChecks()
                .AddAzureServiceBusTopic(
                    name: "ChargeCreatedTopicExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeCreatedTopicName))
                .AddAzureServiceBusTopic(
                    name: "ChargePricesUpdatedTopicExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargePricesUpdatedTopicName));
        }

        private static void ConfigureIntegrationEventsChargeLinks(IServiceCollection serviceCollection)
        {
            serviceCollection.AddHealthChecks()
                .AddAzureServiceBusTopic(
                    name: "ChargeLinksCreatedTopicExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksCreatedTopicName));
        }

        private static void ConfigureIntegrationEventsMeteringPointDomain(IServiceCollection serviceCollection)
        {
            serviceCollection.AddHealthChecks()
                .AddAzureServiceBusTopic(
                    name: "MeteringPointCreatedTopicExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.MeteringPointCreatedTopicName))
                .AddAzureServiceBusSubscription(
                    name: "MeteringPointCreatedSubscriptionExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.MeteringPointCreatedTopicName),
                    subscriptionName: EnvironmentHelper.GetEnv(EnvironmentSettingNames
                        .MeteringPointCreatedSubscriptionName))
                .AddAzureServiceBusQueue(
                    name: "CreateLinksRequestQueueExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    queueName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.CreateLinksRequestQueueName));
        }

        private static void ConfigureIntegrationEventsMessageHub(IServiceCollection serviceCollection)
        {
            serviceCollection.AddHealthChecks()
                .AddAzureServiceBusQueue(
                    name: "MessageHubDataAvailableQueueExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    queueName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.MessageHubDataAvailableQueue))
                .AddAzureServiceBusQueue(
                    name: "MessageHubRequestQueueExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    queueName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.MessageHubRequestQueue))
                .AddAzureServiceBusQueue(
                    name: "MessageHubResponseQueueExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    queueName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.MessageHubReplyQueue));
        }

        private static void ConfigureDomainEventsCharges(IServiceCollection serviceCollection)
        {
            serviceCollection.AddHealthChecks()
                .AddAzureServiceBusTopic(
                    name: "ChargeCommandReceivedTopicExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames
                        .DomainEventManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandReceivedTopicName))
                .AddAzureServiceBusSubscription(
                    name: "ChargeCommandReceivedSubscriptionExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames
                        .DomainEventManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandReceivedTopicName),
                    subscriptionName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandReceivedSubscriptionName))
                .AddAzureServiceBusTopic(
                    name: "ChargeCommandRejectedTopicExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames
                        .DomainEventManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandRejectedTopicName))
                .AddAzureServiceBusSubscription(
                    name: "ChargeCommandRejectedSubscriptionExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames
                        .DomainEventManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandRejectedTopicName),
                    subscriptionName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandRejectedSubscriptionName))
                .AddAzureServiceBusTopic(
                    name: "ChargeCommandAcceptedTopicExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames
                        .DomainEventManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandAcceptedTopicName))
                .AddAzureServiceBusSubscription(
                    name: "ChargeCommandAcceptedSubscriptionExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames
                        .DomainEventManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandAcceptedTopicName),
                    subscriptionName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandAcceptedSubscriptionName))

                // Used by ChargeDataAvailableNotifierEndpoint
                .AddAzureServiceBusSubscription(
                    name: "ChargeCommandDataAvailableNotifierSubscriptionExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames
                        .DomainEventManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandAcceptedTopicName),
                    subscriptionName: EnvironmentHelper.GetEnv(EnvironmentSettingNames
                        .ChargeAcceptedSubDataAvailableNotifier))

                // Used by ChargeIntegrationEventsPublisherEndpoint
                .AddAzureServiceBusSubscription(
                    name: "ChargeCommandAcceptedReceiverSubscriptionExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames
                        .DomainEventManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandAcceptedTopicName),
                    subscriptionName: EnvironmentHelper.GetEnv(EnvironmentSettingNames
                        .CommandAcceptedReceiverSubscriptionName));
        }

        private static void ConfigureDomainEventsChargeLinks(IServiceCollection serviceCollection)
        {
            serviceCollection.AddHealthChecks()
                .AddAzureServiceBusTopic(
                name: "ChargeLinksAcceptedTopicExists",
                connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString),
                topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksAcceptedTopicName))
            .AddAzureServiceBusSubscription(
                name: "ChargeLinksAcceptedReplierSubscriptionExists",
                connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString),
                topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksAcceptedTopicName),
                subscriptionName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksAcceptedReplier))
            .AddAzureServiceBusSubscription(
                name: "ChargeLinksAcceptedEventPublisherSubscriptionExists",
                connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString),
                topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksAcceptedTopicName),
                subscriptionName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksAcceptedSubEventPublisher))
            .AddAzureServiceBusSubscription(
                name: "ChargeLinksAcceptedDataAvailableNotifierSubscriptionExists",
                connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString),
                topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksAcceptedTopicName),
                subscriptionName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksAcceptedSubDataAvailableNotifier))
            .AddAzureServiceBusSubscription(
                name: "ChargeLinksAcceptedConfirmationNotifierSubscriptionExists",
                connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString),
                topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksAcceptedTopicName),
                subscriptionName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksAcceptedSubConfirmationNotifier))
            .AddAzureServiceBusTopic(
                name: "ChargeLinksRejectedTopicExists",
                connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString),
                topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksRejectedTopicName))
            .AddAzureServiceBusSubscription(
                name: "ChargeLinksRejectedSubscriptionExists",
                connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString),
                topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksRejectedTopicName),
                subscriptionName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksRejectedSubscriptionName))
            .AddAzureServiceBusTopic(
                name: "ChargeLinksReceivedTopicExists",
                connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString),
                topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksReceivedTopicName))
            .AddAzureServiceBusSubscription(
                name: "ChargeLinksReceivedSubscriptionExists",
                connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString),
                topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksReceivedTopicName),
                subscriptionName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksReceivedSubscriptionName));
        }

        private static void ConfigureDomainEventsDefaultChargeLinks(IServiceCollection serviceCollection)
        {
            serviceCollection.AddHealthChecks()
                .AddAzureServiceBusTopic(
                    name: "DefaultChargeLinksDataAvailableNotifiedTopicExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames
                        .DomainEventManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames
                        .DefaultChargeLinksDataAvailableNotifiedTopicName))
                .AddAzureServiceBusSubscription(
                    name: "DefaultChargeLinksDataAvailableNotifiedSubscriptionExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames
                        .DomainEventManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames
                        .DefaultChargeLinksDataAvailableNotifiedTopicName),
                    subscriptionName: EnvironmentHelper.GetEnv(EnvironmentSettingNames
                        .DefaultChargeLinksDataAvailableNotifiedSubscription));
        }
    }
}
