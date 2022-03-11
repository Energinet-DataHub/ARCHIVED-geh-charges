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
using Microsoft.Extensions.Diagnostics.HealthChecks;
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
            serviceCollection.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy())
                .AddSqlServer(
                    name: "ChargeDb",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeDbConnectionString))
                .AddAzureServiceBusTopic(
                    name: "ChargeCreatedTopicExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataHubManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.ChargeCreatedTopicName))
                .AddAzureServiceBusTopic(
                    name: "DefaultChargeLinksDataAvailableNotifiedTopicExists",
                    connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DomainEventManagerConnectionString),
                    topicName: EnvironmentHelper.GetEnv(EnvironmentSettingNames.DefaultChargeLinksDataAvailableNotifiedTopicName));

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
