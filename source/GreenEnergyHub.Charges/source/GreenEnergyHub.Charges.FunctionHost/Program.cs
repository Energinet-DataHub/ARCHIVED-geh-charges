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

using GreenEnergyHub.Charges.FunctionHost.Configuration;
using GreenEnergyHub.Charges.Infrastructure.Core.Correlation;
using GreenEnergyHub.Charges.Infrastructure.Core.Function;
using GreenEnergyHub.Charges.Infrastructure.Core.MessageMetaData;
using Grpc.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GreenEnergyHub.Charges.FunctionHost
{
#pragma warning disable SA1402
    public class FunctionsApp
#pragma warning restore SA1402
    {
        public void ConfigureFunctionsWorker(IFunctionsWorkerApplicationBuilder builder)
        {
            builder.UseMiddleware<CorrelationIdMiddleware>();
            builder.UseMiddleware<FunctionTelemetryScopeMiddleware>();
            builder.UseMiddleware<MessageMetaDataMiddleware>();
            builder.UseMiddleware<FunctionInvocationLoggingMiddleware>();
        }

        public void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection serviceCollection)
        {
            SharedConfiguration.ConfigureServices(serviceCollection);

            // Charges
            ChargeIngestionConfiguration.ConfigureServices(serviceCollection);
            ChargeIntegrationEventsPublisherEndpointConfiguration.ConfigureServices(serviceCollection);
            ChargeCommandReceiverConfiguration.ConfigureServices(serviceCollection);
            ChargeConfirmationDataAvailableNotifierEndpointConfiguration.ConfigureServices(serviceCollection);
            ChargeRejectionDataAvailableNotifierEndpointConfiguration.ConfigureServices(serviceCollection);
            ChargeDataAvailableNotifierConfiguration.ConfigureServices(serviceCollection);

            // Charge links
            ChargeLinkIngestionConfiguration.ConfigureServices(serviceCollection);
            ChargeLinkCommandReceiverConfiguration.ConfigureServices(serviceCollection);
            ChargeLinkEventPublisherConfiguration.ConfigureServices(serviceCollection);
            DefaultChargeLinkEventReplierConfiguration.ConfigureServices(serviceCollection);
            CreateChargeLinkReceiverConfiguration.ConfigureServices(serviceCollection);
            ChargeLinkDataAvailableNotifierConfiguration.ConfigureServices(serviceCollection);
            ChargeLinkConfirmationDataAvailableNotifierConfiguration.ConfigureServices(serviceCollection);

            // Metering points
            ConsumptionMeteringPointPersisterConfiguration.ConfigureServices(serviceCollection);

            // Message Hub
            BundleSenderEndpointConfiguration.ConfigureServices(serviceCollection);
        }

        public IHost Build()
            => new HostBuilder()
                .ConfigureFunctionsWorkerDefaults(ConfigureFunctionsWorker)
                .ConfigureServices(ConfigureServices)
                .Build();
    }

    public static class Program
    {
        public static void Main()
        {
            using var host = new FunctionsApp().Build();
            host.Run();
        }
    }
}
