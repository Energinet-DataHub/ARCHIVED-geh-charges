﻿// Copyright 2020 Energinet DataHub A/S
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
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.ChargeLinks.InternalContracts;
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Serialization.Commands;
using GreenEnergyHub.Messaging.Protobuf;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime;

namespace GreenEnergyHub.Charges.ChargeLinkReceiver
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

            ConfigureMessaging(serviceCollection);
        }

        private static void ConfigureMessaging(IServiceCollection services)
        {
            services.AddScoped<ICorrelationContext, CorrelationContext>();
            services.AddScoped<MessageExtractor>();
            services.AddScoped<ChargeLinkCommandConverter>();
            services.AddScoped<MessageDeserializer, ChargeLinkCommandDeserializer>();
            services.SendProtobuf<ChargeLinkCommandDomain>();
            services.AddSingleton<Channel, ServiceBusChannel<ChargeLinkCommand>>();
            services.AddScoped<MessageDispatcher>();

            var connectionString = Environment.GetEnvironmentVariable("CHARGE_LINK_RECEIVED_SENDER_CONNECTION_STRING");
            var topicName = Environment.GetEnvironmentVariable("CHARGE_LINK_RECEIVED_TOPIC_NAME");
            services.AddScoped(sp => new ServiceBusClient(connectionString).CreateSender(topicName));
        }
    }
}
