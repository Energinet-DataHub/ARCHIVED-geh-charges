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
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Application.Charges.Repositories;
using GreenEnergyHub.Charges.Domain.ChargeLinks.Events.Local;
using GreenEnergyHub.Charges.Infrastructure.Context;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandReceived;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Registration;
using GreenEnergyHub.Charges.Infrastructure.Repositories;
using GreenEnergyHub.Messaging.Protobuf;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime;

namespace GreenEnergyHub.Charges.CreateLinkCommandReceiver
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
            serviceCollection.AddLogging();
            serviceCollection.AddScoped(typeof(IClock), _ => SystemClock.Instance);

            ConfigureMessaging(serviceCollection);
            ConfigurePersistence(serviceCollection);

            serviceCollection.AddScoped<ICreateLinkCommandEventHandler, CreateLinkCommandEventHandler>();
        }

        private static void ConfigurePersistence(IServiceCollection services)
        {
            var connectionString = Environment.GetEnvironmentVariable("CHARGE_DB_CONNECTION_STRING") ??
                                   throw new ArgumentNullException(
                                       "CHARGE_DB_CONNECTION_STRING",
                                       "does not exist in configuration settings");
            services.AddDbContext<ChargesDatabaseContext>(
                options => options.UseSqlServer(connectionString));
            services.AddScoped<IChargesDatabaseContext, ChargesDatabaseContext>();
            services.AddScoped<IDefaultChargeLinkRepository, DefaultChargeLinkRepository>();
            services.AddScoped<IChargeRepository, ChargeRepository>();
        }

        private static void ConfigureMessaging(IServiceCollection serviceCollection)
        {
            serviceCollection.AddMessagingProtobuf();
            serviceCollection.ReceiveProtobuf<CreateLinkCommandContract>(
                configuration => configuration.WithParser(() => CreateLinkCommandContract.Parser));

            serviceCollection.SendProtobuf<ChargeLinkCommandReceivedContract>();
            serviceCollection.AddSingleton<Channel, ServiceBusChannel<ChargeLinkCommandReceivedEvent>>();
            serviceCollection.AddScoped<MessageDispatcher>();
            serviceCollection.AddMessagingProtobuf().AddMessageDispatcher<ChargeLinkCommandReceivedEvent>(
                GetEnv("CHARGE_LINK_RECEIVED_SENDER_CONNECTION_STRING"),
                GetEnv("CHARGE_LINK_RECEIVED_TOPIC_NAME"));
        }

        private static string GetEnv(string variableName)
        {
            return Environment.GetEnvironmentVariable(variableName) ??
                   throw new Exception($"Function app is missing required environment variable '{variableName}'");
        }
    }
}
