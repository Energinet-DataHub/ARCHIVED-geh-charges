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
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.ChargeAcknowledgementSender;
using GreenEnergyHub.Charges.Domain.Events.Local;
using GreenEnergyHub.Charges.Infrastructure.Messaging;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Serialization;
using GreenEnergyHub.Charges.Infrastructure.PostOffice;
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
            builder.Services.AddScoped<IChargeAcknowledgementSender, Infrastructure.ChargeAcknowledgementSender>();

            ConfigurePostOffice(builder);
            ConfigureMessaging(builder);
        }

        private static void ConfigurePostOffice(IFunctionsHostBuilder builder)
        {
            builder.Services.AddScoped<IPostOfficeService, PostOfficeService>();
            builder.Services.AddOptions<PostOfficeConfiguration>().Configure(
                configuration =>
                {
                    configuration.ConnectionString = Environment.GetEnvironmentVariable("POST_OFFICE_SENDER_CONNECTION_STRING");
                    configuration.TopicName = Environment.GetEnvironmentVariable("POST_OFFICE_TOPIC_NAME");
                });
        }

        private static void ConfigureMessaging(IFunctionsHostBuilder builder)
        {
            builder.Services.AddScoped<ICorrelationContext, CorrelationContext>();
            builder.Services.AddScoped<MessageDeserializer, JsonMessageDeserializer<ChargeCommandAcceptedEvent>>();
            builder.Services.AddScoped<MessageExtractor>();
            builder.Services.AddSingleton<IJsonSerializer, GreenEnergyHub.Charges.Core.Json.JsonSerializer>();
        }
    }
}
