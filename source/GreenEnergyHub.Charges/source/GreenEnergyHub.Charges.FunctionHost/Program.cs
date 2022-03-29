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
using Energinet.DataHub.Core.Logging.RequestResponseMiddleware;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.FunctionHost.Configuration;
using GreenEnergyHub.Charges.FunctionHost.System;
using GreenEnergyHub.Charges.Infrastructure.Core.Authentication;
using GreenEnergyHub.Charges.Infrastructure.Core.Correlation;
using GreenEnergyHub.Charges.Infrastructure.Core.Function;
using GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData;
using GreenEnergyHub.Charges.Infrastructure.Core.Registration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GreenEnergyHub.Charges.FunctionHost
{
    public static class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults(builder =>
                {
                    builder.UseMiddleware<CorrelationIdMiddleware>();
                    builder.UseMiddleware<FunctionTelemetryScopeMiddleware>();
                    builder.UseMiddleware<MessageMetaDataMiddleware>();
                    builder.UseMiddleware<FunctionInvocationLoggingMiddleware>();
                    builder.UseMiddleware<RequestResponseLoggingMiddleware>();
                    builder.UseMiddleware<JwtTokenWrapperMiddleware>();
                })
                .ConfigureServices(ConfigureServices)
                .Build();

            host.Run();
        }

        private static void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection serviceCollection)
        {
            SharedConfiguration.ConfigureServices(serviceCollection);

            // Health check
            serviceCollection.AddScoped<IHealthCheckEndpointHandler, HealthCheckEndpointHandler>();
            serviceCollection.AddHealthChecks()
                .AddLiveCheck()
                .AddSqlServer(
                    name: "ChargeDb",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeDbConnectionString))

                // Integration events - Charges
                .AddAzureServiceBusTopic(
                    name: "ChargeCreatedTopicExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeCreatedTopicName))
                .AddAzureServiceBusTopic(
                    name: "ChargePricesUpdatedTopicExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargePricesUpdatedTopicName))

                // Integration events - Charge Links
                .AddAzureServiceBusTopic(
                    name: "ChargeLinksCreatedTopicExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksCreatedTopicName))

                // Integration events - Metering Point domain
                .AddAzureServiceBusTopic(
                    name: "MeteringPointCreatedTopicExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.MeteringPointCreatedTopicName))
                .AddAzureServiceBusSubscription(
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.MeteringPointCreatedTopicName),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.MeteringPointCreatedSubscriptionName),
                    "MeteringPointCreatedSubscriptionExists")
                .AddAzureEventHub(
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.CreateLinksRequestQueueName),
                    "CreateLinksRequestQueueExists")

                // Integration events - Message Hub
                .AddAzureEventHub(
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.MessageHubDataAvailableQueue),
                    "MessageHubDataAvailableQueueExists")
                .AddAzureEventHub(
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.MessageHubRequestQueue),
                    "MessageHubRequestQueueExists")
                .AddAzureEventHub(
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.MessageHubReplyQueue),
                    "MessageHubResponseQueueExists")

                // Domain events - Default Charge Links
                .AddAzureServiceBusTopic(
                    name: "DefaultChargeLinksDataAvailableNotifiedTopicExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames
                        .DomainEventManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames
                        .DefaultChargeLinksDataAvailableNotifiedTopicName))
                .AddAzureServiceBusSubscription(
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.DefaultChargeLinksDataAvailableNotifiedTopicName),
                    EnvironmentHelper.GetEnv(
                        EnvironmentSettingNames.DefaultChargeLinksDataAvailableNotifiedSubscription),
                    "DefaultChargeLinksDataAvailableNotifiedSubscriptionExists")

                // Domain events - Charges
                .AddAzureServiceBusTopic(
                    name: "ChargeCommandReceivedTopicExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames
                        .DomainEventManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandReceivedTopicName))
                .AddAzureServiceBusSubscription(
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandReceivedTopicName),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandReceivedSubscriptionName),
                    "ChargeCommandReceivedSubscriptionExists")
                .AddAzureServiceBusTopic(
                    name: "ChargeCommandRejectedTopicExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames
                        .DomainEventManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandRejectedTopicName))
                .AddAzureServiceBusSubscription(
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandRejectedTopicName),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandRejectedSubscriptionName),
                    "ChargeCommandRejectedSubscriptionExists")
                .AddAzureServiceBusTopic(
                    name: "ChargeCommandAcceptedTopicExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames
                        .DomainEventManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandAcceptedTopicName))
                .AddAzureServiceBusSubscription(
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandAcceptedTopicName),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandAcceptedSubscriptionName),
                    "ChargeCommandAcceptedSubscriptionExists")

                // Used by ChargeDataAvailableNotifierEndpoint
                .AddAzureServiceBusSubscription(
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandAcceptedTopicName),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeAcceptedSubDataAvailableNotifier),
                    "ChargeCommandDataAvailableNotifierSubscriptionExists")

                // Used by ChargeIntegrationEventsPublisherEndpoint
                .AddAzureServiceBusSubscription(
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandAcceptedTopicName),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.CommandAcceptedReceiverSubscriptionName),
                    "ChargeCommandAcceptedReceiverSubscriptionExists")

                // Domain events - Charge Links
                .AddAzureServiceBusTopic(
                    name: "ChargeLinksAcceptedTopicExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames
                        .DomainEventManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksAcceptedTopicName))
                .AddAzureServiceBusSubscription(
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksAcceptedTopicName),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksAcceptedReplier),
                    "ChargeLinksAcceptedReplierSubscriptionExists")
                .AddAzureServiceBusSubscription(
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksAcceptedTopicName),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksAcceptedSubEventPublisher),
                    "ChargeLinksAcceptedEventPublisherSubscriptionExists")
                .AddAzureServiceBusSubscription(
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksAcceptedTopicName),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksAcceptedSubDataAvailableNotifier),
                    "ChargeLinksAcceptedDataAvailableNotifierSubscriptionExists")
                .AddAzureServiceBusSubscription(
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksAcceptedTopicName),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksAcceptedSubConfirmationNotifier),
                    "ChargeLinksAcceptedConfirmationNotifierSubscriptionExists")
                .AddAzureServiceBusTopic(
                    name: "ChargeLinksRejectedTopicExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames
                        .DomainEventManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksRejectedTopicName))
                .AddAzureServiceBusSubscription(
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksRejectedTopicName),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksRejectedSubscriptionName),
                    "ChargeLinksRejectedSubscriptionExists")
                .AddAzureServiceBusTopic(
                    name: "ChargeLinksReceivedTopicExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames
                        .DomainEventManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksReceivedTopicName))
                .AddAzureServiceBusSubscription(
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksReceivedTopicName),
                    EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeLinksReceivedSubscriptionName),
                    "ChargeLinksReceivedSubscriptionExists");

            // Charges
            ChargeIngestionConfiguration.ConfigureServices(serviceCollection);
            ChargeCommandReceiverConfiguration.ConfigureServices(serviceCollection);
            ChargeConfirmationDataAvailableNotifierEndpointConfiguration.ConfigureServices(serviceCollection);
            ChargeRejectionDataAvailableNotifierEndpointConfiguration.ConfigureServices(serviceCollection);
            ChargeDataAvailableNotifierConfiguration.ConfigureServices(serviceCollection);
            ChargeIntegrationEventsPublisherEndpointConfiguration.ConfigureServices(serviceCollection);

            // Charge links
            ChargeLinkIngestionConfiguration.ConfigureServices(serviceCollection);
            ChargeLinkCommandReceiverConfiguration.ConfigureServices(serviceCollection);
            ChargeLinkEventPublisherConfiguration.ConfigureServices(serviceCollection);
            DefaultChargeLinkEventReplierConfiguration.ConfigureServices(serviceCollection);
            CreateChargeLinkReceiverConfiguration.ConfigureServices(serviceCollection);
            ChargeLinkDataAvailableNotifierConfiguration.ConfigureServices(serviceCollection);
            ChargeLinkConfirmationDataAvailableNotifierConfiguration.ConfigureServices(serviceCollection);
            ChargeLinksRejectionDataAvailableNotifierEndpointConfiguration.ConfigureServices(serviceCollection);

            // Metering points
            MeteringPointPersisterConfiguration.ConfigureServices(serviceCollection);

            // Message Hub
            BundleSenderEndpointConfiguration.ConfigureServices(serviceCollection);

            // Market participant registry
            MarketParticipantRegistryEndpointConfiguration.ConfigureServices(serviceCollection);
        }
    }
}
