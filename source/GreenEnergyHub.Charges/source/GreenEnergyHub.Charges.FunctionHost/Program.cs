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
using GreenEnergyHub.Charges.Application.ChargeLinks.Handlers;
using GreenEnergyHub.Charges.Application.MeteringPoints.Handlers;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandAcceptedEvents;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommandReceivedEvents;
using GreenEnergyHub.Charges.Domain.ChargeLinkCommands;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Domain.DefaultChargeLinks;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using GreenEnergyHub.Charges.Infrastructure.Context;
using GreenEnergyHub.Charges.Infrastructure.Internal.ChargeLinkCommandAccepted;
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
            ConfigureSharedServices(serviceCollection);

            ConfigureChargeLinkReceiver(serviceCollection);
            ConfigureChargeLinkCommandReceiver(serviceCollection);
            ConfigureMeteringPointCreatedReceiver(serviceCollection);
            ConfigureCreateChargeLinkReceiver(serviceCollection);
        }

        private static void ConfigureSharedServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped(typeof(IClock), _ => SystemClock.Instance);
            serviceCollection.AddLogging();

            ConfigureSharedDatabase(serviceCollection);
            ConfigureSharedMessaging(serviceCollection);
        }

        private static void ConfigureSharedDatabase(IServiceCollection serviceCollection)
        {
            var connectionString = Environment.GetEnvironmentVariable("CHARGE_DB_CONNECTION_STRING") ??
                                   throw new ArgumentNullException(
                                       "CHARGE_DB_CONNECTION_STRING",
                                       "does not exist in configuration settings");
            serviceCollection.AddDbContext<ChargesDatabaseContext>(
                options => options.UseSqlServer(connectionString, options => options.UseNodaTime()));
            serviceCollection.AddScoped<IChargesDatabaseContext, ChargesDatabaseContext>();

            serviceCollection.AddScoped<IChargeRepository, ChargeRepository>();
            serviceCollection.AddScoped<IMeteringPointRepository, MeteringPointRepository>();
        }

        private static void ConfigureSharedMessaging(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<MessageDispatcher>();
            serviceCollection.ConfigureProtobufReception();

            serviceCollection.SendProtobuf<ChargeLinkCommandReceivedContract>();
            serviceCollection.AddMessagingProtobuf().AddMessageDispatcher<ChargeLinkCommandReceivedEvent>(
                GetEnv("DOMAINEVENT_SENDER_CONNECTION_STRING"),
                GetEnv("CHARGE_LINK_RECEIVED_TOPIC_NAME"));
        }

        private static void ConfigureChargeLinkReceiver(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ChargeLinkCommandConverter>();
            serviceCollection.AddScoped<MessageExtractor<ChargeLinkCommand>>();
            serviceCollection.AddScoped<MessageDeserializer<ChargeLinkCommand>, ChargeLinkCommandDeserializer>();

            serviceCollection.AddScoped<IChargeLinkCommandHandler, ChargeLinkCommandHandler>();
        }

        private static void ConfigureChargeLinkCommandReceiver(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IChargeLinkCommandReceivedHandler, ChargeLinkCommandReceivedHandler>();
            serviceCollection.AddScoped<IChargeLinkFactory, ChargeLinkFactory>();
            serviceCollection.AddSingleton<IChargeLinkCommandAcceptedEventFactory, ChargeLinkCommandAcceptedEventFactory>();

            serviceCollection.ReceiveProtobufMessage<ChargeLinkCommandReceivedContract>(
                configuration => configuration.WithParser(() => ChargeLinkCommandReceivedContract.Parser));
            serviceCollection.SendProtobuf<ChargeLinkCommandAcceptedContract>();
            serviceCollection.AddMessagingProtobuf().AddMessageDispatcher<ChargeLinkCommandAcceptedEvent>(
                GetEnv("DOMAINEVENT_SENDER_CONNECTION_STRING"),
                GetEnv("CHARGE_LINK_ACCEPTED_TOPIC_NAME"));

            serviceCollection.AddScoped<IChargeLinkRepository, ChargeLinkRepository>();
        }

        private static void ConfigureCreateChargeLinkReceiver(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ICreateLinkCommandEventHandler, CreateLinkCommandEventHandler>();
            serviceCollection.AddScoped<IChargeLinkCommandFactory, ChargeLinkCommandFactory>();

            serviceCollection.ReceiveProtobufMessage<CreateLinkCommandContract>(
                configuration => configuration.WithParser(() => CreateLinkCommandContract.Parser));

            serviceCollection.AddScoped<IDefaultChargeLinkRepository, DefaultChargeLinkRepository>();
        }

        private static void ConfigureMeteringPointCreatedReceiver(IServiceCollection serviceCollection)
        {
            serviceCollection.ReceiveProtobufMessage<MeteringPointCreated>(
                configuration => configuration.WithParser(() => MeteringPointCreated.Parser));

            serviceCollection.AddScoped<IMeteringPointCreatedEventHandler, MeteringPointCreatedEventHandler>();
        }

        private static string GetEnv(string variableName)
        {
            return Environment.GetEnvironmentVariable(variableName) ??
                   throw new Exception($"Function app is missing required environment variable '{variableName}'");
        }
    }
}
