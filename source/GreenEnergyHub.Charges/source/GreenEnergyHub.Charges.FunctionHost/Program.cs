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
using GreenEnergyHub.Charges.Infrastructure.Correlation;
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
                })
                .ConfigureServices(ConfigureServices)
                .Build();

            host.Run();
        }

        private static void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection serviceCollection)
        {
            SharedConfiguration.ConfigureServices(serviceCollection);

            // Charges
            ChargeIngestionConfiguration.ConfigureServices(serviceCollection);
            ChargeCommandAcceptedReceiverConfiguration.ConfigureServices(serviceCollection);
            ChargeCommandReceiverConfiguration.ConfigureServices(serviceCollection);
            ChargeConfirmationSenderConfiguration.ConfigureServices(serviceCollection);
            ChargeRejectionSenderConfiguration.ConfigureServices(serviceCollection);

            // Charge links
            ChargeLinkIngestionConfiguration.ConfigureServices(serviceCollection);
            ChargeLinkCommandReceiverConfiguration.ConfigureServices(serviceCollection);
            ChargeLinkEventPublisherConfiguration.ConfigureServices(serviceCollection);
            CreateChargeLinkReceiverConfiguration.ConfigureServices(serviceCollection);
            ChargeLinkCreatedDataAvailableNotifierConfiguration.ConfigureServices(serviceCollection);
            ChargeLinkCreatedBundleSenderConfiguration.ConfigureServices(serviceCollection);

            // Metering points
            ConsumptionMeteringPointPersisterConfiguration.ConfigureServices(serviceCollection);
        }
    }
}
