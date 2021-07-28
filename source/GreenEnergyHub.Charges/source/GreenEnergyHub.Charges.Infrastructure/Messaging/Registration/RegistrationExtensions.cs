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

using GreenEnergyHub.Charges.Domain.Events.Local;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Serialization;
using GreenEnergyHub.Json;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.Infrastructure.Messaging.Registration
{
    public static class RegistrationExtensions
    {
        public static MessagingRegistrator AddMessaging(this IServiceCollection services)
        {
            services.AddScoped<ICorrelationContext, CorrelationContext>();
            services.AddScoped<MessageExtractor>();
            services.AddSingleton<IJsonSerializer, Core.Json.JsonSerializer>();
            services.AddScoped<MessageDeserializer, JsonMessageDeserializer<ChargeCommandAcceptedEvent>>();
            return new MessagingRegistrator(services);
        }

        public static MessagingRegistrator AddMessagingProtobuf(this IServiceCollection services)
        {
            services.AddScoped<ICorrelationContext, CorrelationContext>();
            services.AddScoped<MessageExtractor>();

            return new MessagingRegistrator(services);
        }
    }
}
