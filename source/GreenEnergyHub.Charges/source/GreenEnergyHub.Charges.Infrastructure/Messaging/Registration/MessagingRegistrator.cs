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
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Infrastructure.Messaging.Serialization;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace GreenEnergyHub.Charges.Infrastructure.Messaging.Registration
{
    public class MessagingRegistrator
    {
        private readonly Container _container;

        internal MessagingRegistrator(Container container)
        {
            _container = container;
        }

        /// <summary>
        /// Register services required to resolve a <see cref="MessageExtractor{TInboundMessage}"/>.
        /// </summary>
        public MessagingRegistrator AddMessageExtractor<TInboundMessage>()
            where TInboundMessage : IInboundMessage
        {
            _container.Register<MessageExtractor<TInboundMessage>>(Lifestyle.Scoped);
            _container.Register<MessageDeserializer<TInboundMessage>, JsonMessageDeserializer<TInboundMessage>>(Lifestyle.Scoped);

            return this;
        }

        /// <summary>
        /// Register services required to resolve a <see cref="IMessageDispatcher{TInboundMessage}"/>.
        /// </summary>
        public MessagingRegistrator AddMessageDispatcher<TOutboundMessage>(
            string serviceBusConnectionString,
            string serviceBusTopicName)
            where TOutboundMessage : IOutboundMessage
        {
            _container.Register<IMessageDispatcher<TOutboundMessage>, MessageDispatcher<TOutboundMessage>>(Lifestyle.Scoped);
            _container.Register<Channel<TOutboundMessage>, ServiceBusChannel<TOutboundMessage>>(Lifestyle.Scoped);

            // Must be a singleton as per documentation of ServiceBusClient and ServiceBusSender
            _container.Register<IServiceBusSender<TOutboundMessage>>(
                () =>
                {
                    var client = new ServiceBusClient(serviceBusConnectionString);
                    var instance = client.CreateSender(serviceBusTopicName);
                    return new ServiceBusSender<TOutboundMessage>(instance);
                });

            return this;
        }
    }
}
