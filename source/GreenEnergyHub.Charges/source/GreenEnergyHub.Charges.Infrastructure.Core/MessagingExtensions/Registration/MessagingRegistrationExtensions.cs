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

using Azure.Messaging.ServiceBus;
using Energinet.DataHub.Core.Messaging.Transport;
using GreenEnergyHub.Charges.Application.Messaging;
using GreenEnergyHub.Charges.Infrastructure.Core.Registration;
using Microsoft.Extensions.DependencyInjection;

namespace GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions.Registration
{
    public static class MessagingRegistrationExtensions
    {
        public static IServiceCollection AddExternalMessageDispatcher<TOutboundMessage>(this IServiceCollection serviceCollection, string serviceBusTopicEnvName)
            where TOutboundMessage : IOutboundMessage
        {
            serviceCollection.AddScoped<IMessageDispatcher<TOutboundMessage>, MessageDispatcher<TOutboundMessage>>();
            serviceCollection.AddScoped<Channel<TOutboundMessage>, ServiceBusChannel<TOutboundMessage>>();

            // Must be a singleton as per documentation of ServiceBusClient and ServiceBusSender
            serviceCollection.AddSingleton<IServiceBusSender<TOutboundMessage>>(
                sp =>
                {
                    var client = sp.GetRequiredService<ServiceBusClient>();
                    var serviceBusTopicName = EnvironmentHelper.GetEnv(serviceBusTopicEnvName);
                    var instance = client.CreateSender(serviceBusTopicName);
                    return new ServiceBusSender<TOutboundMessage>(instance);
                });

            return serviceCollection;
        }
    }
}
