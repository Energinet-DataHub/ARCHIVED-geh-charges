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
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeDbConnectionString),
                    name: "ChargeDb");

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
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeCreatedTopicName),
                    name: "ChargeCreatedTopicExists")
                .AddAzureServiceBusTopic(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargePricesUpdatedTopicName),
                    name: "ChargePricesUpdatedTopicExists");
        }

        private static void ConfigureIntegrationEventsChargeLinks(IServiceCollection serviceCollection)
        {
            serviceCollection.AddHealthChecks()
                .AddAzureServiceBusTopic(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksCreatedTopicName),
                    name: "ChargeLinksCreatedTopicExists");
        }

        private static void ConfigureIntegrationEvents(IServiceCollection serviceCollection)
        {
            serviceCollection.AddHealthChecks()
                .AddAzureServiceBusTopic(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.IntegrationEventTopicName),
                    name: "IntegrationEventsTopicExists")
                .AddAzureServiceBusSubscription(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.IntegrationEventTopicName),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.MeteringPointCreatedSubscriptionName),
                    name: "MeteringPointCreatedSubscriptionExists")
                .AddAzureServiceBusSubscription(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.IntegrationEventTopicName),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.MarketParticipantCreatedSubscriptionName),
                    name: "MarketParticipantCreatedSubscriptionExists")
                .AddAzureServiceBusSubscription(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.IntegrationEventTopicName),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.MarketParticipantStatusChangedSubscriptionName),
                    name: "MarketParticipantStatusChangedSubscriptionExists")
                .AddAzureServiceBusSubscription(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.IntegrationEventTopicName),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.MarketParticipantB2CActorIdChangedSubscriptionName),
                    name: "MarketParticipantB2CActorIdChangedSubscriptionExists")
                .AddAzureServiceBusSubscription(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.IntegrationEventTopicName),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.MarketParticipantNameChangedSubscriptionName),
                    name: "MarketParticipantNameChangedSubscriptionExists")
                .AddAzureServiceBusQueue(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.CreateLinksRequestQueueName),
                    name: "CreateLinksRequestQueueExists");
        }

        private static void ConfigureIntegrationEventsMessageHub(IServiceCollection serviceCollection)
        {
            serviceCollection.AddHealthChecks()
                .AddAzureServiceBusQueue(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.MessageHubDataAvailableQueue),
                    name: "MessageHubDataAvailableQueueExists")
                .AddAzureServiceBusQueue(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.MessageHubRequestQueue),
                    name: "MessageHubRequestQueueExists")
                .AddAzureServiceBusQueue(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.MessageHubReplyQueue),
                    name: "MessageHubResponseQueueExists");
        }

        private static void ConfigureChargesDomainEventsTopic(IServiceCollection serviceCollection)
        {
            serviceCollection.AddHealthChecks()
                .AddAzureServiceBusTopic(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                    name: "ChargesDomainEventsTopicExists");
        }

        private static void ConfigureDomainEventsCharges(IServiceCollection serviceCollection)
        {
            serviceCollection.AddHealthChecks()
                .AddAzureServiceBusSubscription(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeInformationCommandReceivedSubscriptionName),
                    name: "ChargeInformationCommandReceivedSubscriptionExists")
                .AddAzureServiceBusSubscription(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeInformationOperationsRejectedSubscriptionName),
                    name: "ChargeInformationOperationsRejectedSubscriptionExists")
                .AddAzureServiceBusSubscription(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeInformationOperationsAcceptedSubscriptionName),
                    name: "ChargeInformationOperationsAcceptedSubscriptionExists")
                .AddAzureServiceBusSubscription(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeInformationOperationsAcceptedDataAvailableSubscriptionName),
                    name: "ChargeInformationOperationsAcceptedDataAvailableSubscriptionExists")
                .AddAzureServiceBusSubscription(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeInformationOperationsAcceptedPublishSubscriptionName),
                    name: "ChargeInformationOperationsAcceptedPublishSubscriptionExists")
                .AddAzureServiceBusSubscription(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeInformationOperationsAcceptedPersistMessageSubscriptionName),
                    name: "ChargeInformationOperationsAcceptedPersistMessageSubscriptionExists")
                .AddAzureServiceBusSubscription(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeInformationOperationsAcceptedPersistHistorySubscriptionName),
                    name: "ChargeInformationOperationsAcceptedPersistHistorySubscriptionExists")
                // .AddAzureServiceBusSubscription(
                //     _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubListenerConnectionString),
                //     _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                //     _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargePriceCommandReceivedSubscriptionName),
                //     name: "ChargePriceCommandReceivedSubscriptionExists")
                .AddAzureServiceBusSubscription(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargePriceOperationsRejectedSubscriptionName),
                    name: "ChargePriceOperationsRejectedSubscriptionExists")
                .AddAzureServiceBusSubscription(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargePriceOperationsAcceptedSubscriptionName),
                    name: "ChargePriceOperationsAcceptedSubscriptionExists")
                .AddAzureServiceBusSubscription(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargePriceOperationsAcceptedDataAvailableSubscriptionName),
                    name: "ChargePriceOperationsAcceptedDataAvailableSubscriptionExists")
                .AddAzureServiceBusSubscription(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargePriceOperationsAcceptedPublishSubscriptionName),
                    name: "ChargePriceOperationsAcceptedPublishSubscriptionExists")
                .AddAzureServiceBusSubscription(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargePriceOperationsAcceptedPersistMessageSubscriptionName),
                    name: "ChargePriceOperationsAcceptedPersistMessageSubscriptionExists");
        }

        private static void ConfigureDomainEventsChargeLinks(IServiceCollection serviceCollection)
        {
            serviceCollection.AddHealthChecks()
                .AddAzureServiceBusSubscription(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksAcceptedPublishSubscriptionName),
                    name: "ChargeLinksAcceptedPublishSubscriptionExists")
                .AddAzureServiceBusSubscription(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames
                        .ChargeLinksAcceptedDataAvailableSubscriptionName),
                    name: "ChargeLinksAcceptedDataAvailableSubscriptionExists")
                .AddAzureServiceBusSubscription(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksAcceptedSubscriptionName),
                    name: "ChargeLinksAcceptedSubscriptionExists")
                .AddAzureServiceBusSubscription(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksCommandRejectedSubscriptionName),
                    name: "ChargeLinksCommandRejectedSubscriptionExists")
                .AddAzureServiceBusSubscription(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksCommandReceivedSubscriptionName),
                    name: "ChargeLinksCommandReceivedSubscriptionExists");
        }

        private static void ConfigureDomainEventsDefaultChargeLinks(IServiceCollection serviceCollection)
        {
            serviceCollection.AddHealthChecks()
                .AddAzureServiceBusSubscription(
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargesDomainEventTopicName),
                    _ => EnvironmentHelper.GetEnv(EnvironmentSettingNames.DefaultChargeLinksDataAvailableSubscriptionName),
                    name: "DefaultChargeLinksDataAvailableSubscriptionExists");
        }
    }
}
