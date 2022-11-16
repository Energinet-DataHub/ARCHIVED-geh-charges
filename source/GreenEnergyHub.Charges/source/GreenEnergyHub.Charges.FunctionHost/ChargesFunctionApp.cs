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

using System.Threading.Tasks;
using Energinet.DataHub.Core.App.FunctionApp.FunctionTelemetryScope;
using Energinet.DataHub.Core.App.FunctionApp.Middleware;
using Energinet.DataHub.Core.App.FunctionApp.Middleware.CorrelationId;
using Energinet.DataHub.Core.Logging.RequestResponseMiddleware;
using GreenEnergyHub.Charges.FunctionHost.Configuration;
using GreenEnergyHub.Charges.Infrastructure.Core.Function;
using GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;

namespace GreenEnergyHub.Charges.FunctionHost
{
    public static class ChargesFunctionApp
    {
        public static async Task Main()
        {
            using var host = ConfigureApplication();
            await host.RunAsync().ConfigureAwait(false);
        }

        public static IHost ConfigureApplication()
            => new HostBuilder()
                .ConfigureFunctionsWorkerDefaults(worker =>
                {
                    worker.AddApplicationInsights().AddApplicationInsightsLogger();
                    worker.UseMiddleware<CorrelationIdMiddleware>();
                    worker.UseMiddleware<FunctionTelemetryScopeMiddleware>();
                    worker.UseMiddleware<MessageMetaDataMiddleware>();
                    worker.UseMiddleware<FunctionInvocationLoggingMiddleware>();
                    worker.UseMiddleware<RequestResponseLoggingMiddleware>();
                    worker.UseMiddleware<JwtTokenMiddleware>();
                    worker.UseMiddleware<ActorMiddleware>();
                }).ConfigureLogging(builder =>
                {
                    builder.AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Information);
                    builder.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Error);
                    builder.AddSystemdConsole();
                })
                .ConfigureServices(ConfigureServices)
                .Build();

        private static void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection serviceCollection)
        {
            SharedConfiguration.ConfigureServices(serviceCollection);
            HealthCheckConfiguration.ConfigureServices(serviceCollection);

            // Outbox messages
            OutboxMessageProcessorConfiguration.ConfigureServices(serviceCollection);

            // Charges
            ChargeIngestionConfiguration.ConfigureServices(serviceCollection);
            ChargeInformationCommandReceivedConfiguration.ConfigureServices(serviceCollection);
            ChargePriceCommandReceivedConfiguration.ConfigureServices(serviceCollection);
            ChargeConfirmationDataAvailableNotifierEndpointConfiguration.ConfigureServices(serviceCollection);
            ChargeRejectionDataAvailableNotifierEndpointConfiguration.ConfigureServices(serviceCollection);
            ChargeDataAvailableNotifierConfiguration.ConfigureServices(serviceCollection);
            ChargePriceConfirmedDataAvailableNotifierEndpointConfiguration.ConfigureServices(serviceCollection);
            ChargePriceRejectedDataAvailableNotifierEndpointConfiguration.ConfigureServices(serviceCollection);
            ChargeInformationMessagePersisterEndpointConfiguration.ConfigureServices(serviceCollection);
            ChargePriceMessagePersisterEndpointConfiguration.ConfigureServices(serviceCollection);

            // Integration events
            ChargeIntegrationEventsPublisherEndpointConfiguration.ConfigureServices(serviceCollection);
            ChargePriceIntegrationEventsPublisherEndpointConfiguration.ConfigureServices(serviceCollection);

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

            // Integration test
            IntegrationEventHandlersConfiguration.ConfigureServices(serviceCollection);
        }
    }
}
