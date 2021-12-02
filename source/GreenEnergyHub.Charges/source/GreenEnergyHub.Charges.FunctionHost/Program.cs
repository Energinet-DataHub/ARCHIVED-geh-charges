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
using EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using GreenEnergyHub.Charges.FunctionHost.Common;
using GreenEnergyHub.Charges.FunctionHost.Configuration;
using GreenEnergyHub.Charges.Infrastructure.Context;
using GreenEnergyHub.Charges.Infrastructure.Correlation;
using GreenEnergyHub.Charges.Infrastructure.Function;
using GreenEnergyHub.Charges.Infrastructure.MessageMetaData;
using GreenEnergyHub.Charges.Infrastructure.SimpleInjector;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SimpleInjector;

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
                    builder.UseMiddleware<SimpleInjectorScopedRequest>();
                })
                .ConfigureServices(ConfigureServices)
                .Build();

            host.Run();
        }

        private static void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection serviceCollection)
        {
            var container = new Container();
            var descriptor = new ServiceDescriptor(
                typeof(IFunctionActivator),
                typeof(SimpleInjectorActivator),
                ServiceLifetime.Singleton);

            serviceCollection.Replace(descriptor); // Replace existing activator

            serviceCollection.AddLogging();

            serviceCollection.AddApplicationInsightsTelemetryWorkerService(
                EnvironmentHelper.GetEnv(EnvironmentSettingNames.AppInsightsInstrumentationKey));

            serviceCollection.AddSimpleInjector(container, options =>
            {
                options.AddLogging(); // Allow use non-generic ILogger interface
            });

            var connectionString = Environment.GetEnvironmentVariable(EnvironmentSettingNames.ChargeDbConnectionString) ??
                                   throw new ArgumentNullException(
                                       EnvironmentSettingNames.ChargeDbConnectionString,
                                       "does not exist in configuration settings");

            serviceCollection.AddDbContext<ChargesDatabaseContext>(
                options => options.UseSqlServer(connectionString, o => o.UseNodaTime()));

            SharedConfiguration.ConfigureServices(container);

            // Charges
            ChargeIngestionConfiguration.ConfigureServices(container);
            ChargeCommandAcceptedReceiverConfiguration.ConfigureServices(container);
            ChargeCommandReceiverConfiguration.ConfigureServices(container);
            ChargeConfirmationSenderConfiguration.ConfigureServices(container);
            ChargeRejectionSenderConfiguration.ConfigureServices(container);
            ChargeDataAvailableNotifierConfiguration.ConfigureServices(container);

            // Charge links
            ChargeLinkIngestionConfiguration.ConfigureServices(container);
            ChargeLinkCommandReceiverConfiguration.ConfigureServices(container);
            ChargeLinkEventPublisherConfiguration.ConfigureServices(container);
            DefaultChargeLinkEventReplierConfiguration.ConfigureServices(container);
            CreateChargeLinkReceiverConfiguration.ConfigureServices(container);
            ChargeLinkDataAvailableNotifierConfiguration.ConfigureServices(container);
            ChargeLinkConfirmationDataAvailableNotifierConfiguration.ConfigureServices(container);

            // Metering points
            ConsumptionMeteringPointPersisterConfiguration.ConfigureServices(container);

            // Message Hub
            BundleSenderEndpointConfiguration.ConfigureServices(container);
        }
    }
}
