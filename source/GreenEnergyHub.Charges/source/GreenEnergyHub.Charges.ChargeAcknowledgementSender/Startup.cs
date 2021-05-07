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
using System.Diagnostics.CodeAnalysis;
using Azure.Messaging.ServiceBus;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.ChargeAcknowledgementSender;
using GreenEnergyHub.Charges.Domain.Events.Local;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Serialization;
using GreenEnergyHub.Json;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

[assembly: FunctionsStartup(typeof(Startup))]

namespace GreenEnergyHub.Charges.ChargeAcknowledgementSender
{
    public class Startup : FunctionsStartup
    {
        public override void Configure([NotNull] IFunctionsHostBuilder builder)
        {
            builder.Services.AddScoped(typeof(IClock), _ => SystemClock.Instance);
            builder.Services.AddScoped<IChargeAcknowledgementSender, Application.ChargeAcknowledgementSender>();
            builder.Services.AddScoped<ICorrelationContext, CorrelationContext>();

            ConfigureMessaging(builder);
        }

        private static void ConfigureMessaging(IFunctionsHostBuilder builder)
        {
            builder.Services.AddScoped<MessageExtractor>();
            builder.Services.AddScoped<MessageDispatcher>();
            builder.Services.AddScoped<MessageSerializer, JsonMessageSerializer>();
            builder.Services.AddScoped<IJsonOutboundMapperFactory, DefaultJsonMapperFactory>();
            builder.Services.AddScoped<Channel, ServiceBusChannel>();
            builder.Services.AddScoped<ServiceBusSender>(
                _ =>
                {
                    var connectionString = Environment.GetEnvironmentVariable("POST_OFFICE_SENDER_CONNECTION_STRING");
                    var topicName = Environment.GetEnvironmentVariable("POST_OFFICE_TOPIC_NAME");
                    var client = new ServiceBusClient(connectionString);
                    return client.CreateSender(topicName);
                });
            builder.Services.AddScoped<MessageDeserializer, JsonMessageDeserializer<ChargeCommandAcceptedEvent>>();
            builder.Services.AddSingleton<IJsonSerializer, GreenEnergyHub.Charges.Core.Json.JsonSerializer>();
        }
    }
}
