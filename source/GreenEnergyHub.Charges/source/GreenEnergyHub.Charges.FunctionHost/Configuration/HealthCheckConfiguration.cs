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
            ConfigureIntegrationEvents(serviceCollection);
            ConfigureIntegrationEventsMessageHub(serviceCollection);

            // Domain events
            ConfigureChargesDomainEventsTopic(serviceCollection);
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

        private static void ConfigureIntegrationEvents(IServiceCollection serviceCollection)
        {
            serviceCollection.AddHealthChecks()
                .AddAzureServiceBusTopic(
                    name: "IntegrationEventsTopicExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.IntegrationEventTopicName))
                .AddAzureServiceBusSubscription(
                    name: "MeteringPointCreatedSubscriptionExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.IntegrationEventTopicName),
                    subscriptionName: EnvironmentHelper.GetEnv(EnvironmentSettingNames
                        .MeteringPointCreatedSubscriptionName))
                .AddAzureServiceBusSubscription(
                    name: "MarketParticipantChangedSubscriptionExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.IntegrationEventTopicName),
                    subscriptionName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.MarketParticipantChangedSubscriptionName))
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

        private static void ConfigureChargesDomainEventsTopic(IServiceCollection serviceCollection)
        {
            serviceCollection.AddHealthChecks()
                .AddAzureServiceBusTopic(
                    name: "ChargesDomainEventsTopicExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName));
        }

        private static void ConfigureDomainEventsCharges(IServiceCollection serviceCollection)
        {
            serviceCollection.AddHealthChecks()
                .AddAzureServiceBusSubscription(
                    name: "ChargeInformationCommandReceivedSubscriptionExists",
                    connectionString: EnvironmentHelper.GetEnv(
                        EnvironmentSettingNames.DomainEventManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                    subscriptionName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeInformationCommandReceivedSubscriptionName))
                .AddAzureServiceBusSubscription(
                    name: "ChargePriceCommandReceivedSubscriptionExists",
                    connectionString: EnvironmentHelper.GetEnv(
                        EnvironmentSettingNames.DomainEventManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                    subscriptionName: EnvironmentHelper.GetEnv(
                        EnvironmentSettingNames.ChargePriceCommandReceivedSubscriptionName))
                .AddAzureServiceBusSubscription(
                    name: "ChargeInformationOperationsRejectedSubscriptionExists",
                    connectionString: EnvironmentHelper.GetEnv(
                        EnvironmentSettingNames.DomainEventManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                    subscriptionName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeInformationOperationsRejectedSubscriptionName))
                .AddAzureServiceBusSubscription(
                    name: "ChargeInformationOperationsAcceptedSubscriptionExists",
                    connectionString: EnvironmentHelper.GetEnv(
                        EnvironmentSettingNames.DomainEventManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                    subscriptionName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeInformationOperationsAcceptedSubscriptionName))
                .AddAzureServiceBusSubscription(
                    name: "ChargePriceOperationsRejectedSubscriptionExists",
                    connectionString: EnvironmentHelper.GetEnv(
                        EnvironmentSettingNames.DomainEventManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                    subscriptionName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargePriceOperationsRejectedSubscriptionName))
                .AddAzureServiceBusSubscription(
                    name: "ChargePriceOperationsConfirmedSubscriptionExists",
                    connectionString: EnvironmentHelper.GetEnv(
                        EnvironmentSettingNames.DomainEventManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                    subscriptionName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargePriceOperationsConfirmedSubscriptionName))
                .AddAzureServiceBusSubscription(
                    name: "ChargeInformationOperationsAcceptedDataAvailableSubscriptionExists",
                    connectionString: EnvironmentHelper.GetEnv(
                        EnvironmentSettingNames.DomainEventManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                    subscriptionName: EnvironmentHelper.GetEnv(
                        EnvironmentSettingNames.ChargeInformationOperationsAcceptedDataAvailableSubscriptionName))
                .AddAzureServiceBusSubscription(
                    name: "ChargePriceOperationsConfirmedDataAvailableSubscriptionExists",
                    connectionString: EnvironmentHelper.GetEnv(
                        EnvironmentSettingNames.DomainEventManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                    subscriptionName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargePriceOperationsConfirmedDataAvailableSubscriptionName))
                .AddAzureServiceBusSubscription(
                    name: "ChargeInformationOperationsAcceptedPublishSubscriptionExists",
                    connectionString: EnvironmentHelper.GetEnv(
                        EnvironmentSettingNames.DomainEventManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                    subscriptionName: EnvironmentHelper.GetEnv(
                        EnvironmentSettingNames.ChargeInformationOperationsAcceptedPublishSubscriptionName))
                .AddAzureServiceBusSubscription(
                    name: "ChargePriceOperationsConfirmedPublishSubscriptionExists",
                    connectionString: EnvironmentHelper.GetEnv(
                        EnvironmentSettingNames.DomainEventManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                    subscriptionName: EnvironmentHelper.GetEnv(
                        EnvironmentSettingNames.ChargePriceOperationsConfirmedPublishSubscriptionName));
        }

        private static void ConfigureDomainEventsChargeLinks(IServiceCollection serviceCollection)
        {
            serviceCollection.AddHealthChecks()
            .AddAzureServiceBusSubscription(
                name: "ChargeLinksAcceptedPublishSubscriptionExists",
                connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString),
                topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                subscriptionName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksAcceptedPublishSubscriptionName))
            .AddAzureServiceBusSubscription(
                name: "ChargeLinksAcceptedDataAvailableSubscriptionExists",
                connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString),
                topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                subscriptionName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksAcceptedDataAvailableSubscriptionName))
            .AddAzureServiceBusSubscription(
                name: "ChargeLinksAcceptedConfirmationSubscriptionExists",
                connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString),
                topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                subscriptionName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksAcceptedConfirmationSubscriptionName))
            .AddAzureServiceBusSubscription(
                name: "ChargeLinksCommandRejectedSubscriptionExists",
                connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString),
                topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                subscriptionName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksCommandRejectedSubscriptionName))
            .AddAzureServiceBusSubscription(
                name: "ChargeLinksCommandReceivedSubscriptionExists",
                connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString),
                topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                subscriptionName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksCommandReceivedSubscriptionName));
        }

        private static void ConfigureDomainEventsDefaultChargeLinks(IServiceCollection serviceCollection)
        {
            serviceCollection.AddHealthChecks()
                .AddAzureServiceBusSubscription(
                    name: "DefaultChargeLinksDataAvailableSubscriptionExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                    subscriptionName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DefaultChargeLinksDataAvailableSubscriptionName));
        }
    }
}
