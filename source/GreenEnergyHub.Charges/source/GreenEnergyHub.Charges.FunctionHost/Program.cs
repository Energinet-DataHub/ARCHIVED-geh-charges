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
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using EntityFrameworkCore.SqlServer.NodaTime.Extensions;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Application.ChangeOfCharges.Repositories;
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Domain.ChargeLinks.Command;
using GreenEnergyHub.Charges.Domain.ChargeLinks.Events.Local;
using GreenEnergyHub.Charges.Infrastructure.Context;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Registration;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Serialization;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Serialization.Commands;
using GreenEnergyHub.Charges.Infrastructure.Repositories;
using GreenEnergyHub.Messaging.Protobuf;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime;

namespace GreenEnergyHub.Charges.FunctionHost
{
    public static class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(ConfigureServices)
                .Build();

            host.Run();
        }

        private static void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped(typeof(IClock), _ => SystemClock.Instance);
            serviceCollection.AddLogging();

            ConfigureDatabase(serviceCollection);
            ConfigureMessaging(serviceCollection);
            ConfigureHandlers(serviceCollection);
        }

        private static void ConfigureDatabase(IServiceCollection services)
        {
            var connectionString = Environment.GetEnvironmentVariable("CHARGE_DB_CONNECTION_STRING") ??
                                   throw new ArgumentNullException(
                                       "CHARGE_DB_CONNECTION_STRING",
                                       "does not exist in configuration settings");
            services.AddDbContext<ChargesDatabaseContext>(
                options => options.UseSqlServer(connectionString, options => options.UseNodaTime()));
            services.AddScoped<IChargesDatabaseContext, ChargesDatabaseContext>();
            services.AddScoped<IMeteringPointRepository, MeteringPointRepository>();
        }

        private static void ConfigureMessaging(IServiceCollection services)
        {
            services.ReceiveProtobuf<MeteringPointCreated>(
                configuration => configuration.WithParser(() => MeteringPointCreated.Parser));

            services.AddScoped<ChargeLinkCommandConverter>();
            services.AddScoped<MessageExtractor<ChargeLinkCommand>>();
            services.AddScoped<MessageDeserializer<ChargeLinkCommand>, ChargeLinkCommandDeserializer>();
            services.SendProtobuf<ChargeLinkCommandReceivedContract>();
            // services.AddSingleton<Channel, ServiceBusChannel<ChargeLinkCommandReceivedEvent>>();
            services.AddScoped<MessageDispatcher>();

            services.AddMessagingProtobuf().AddMessageDispatcher<ChargeLinkCommandReceivedEvent>(
                GetEnv("DOMAINEVENT_SENDER_CONNECTION_STRING"),
                GetEnv("CHARGE_LINK_RECEIVED_TOPIC_NAME"));
        }

        private static void ConfigureHandlers(IServiceCollection services)
        {
            services.AddScoped<IChargeLinkCommandHandler, ChargeLinkCommandHandler>();
            services.AddScoped<IMeteringPointCreatedEventHandler, MeteringPointCreatedEventHandler>();
        }

        private static string GetEnv(string variableName)
        {
            return Environment.GetEnvironmentVariable(variableName) ??
                   throw new Exception($"Function app is missing required environment variable '{variableName}'");
        }
    }
}
