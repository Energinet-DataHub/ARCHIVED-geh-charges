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

using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace GreenEnergyHub.Charges.Infrastructure.Core.MessagingExtensions
{
    /// <summary>
    /// This class enables us to kind of handle <see cref="ServiceBusSender"/>s generically
    /// and thus enabling dependency injection and message handling based on the type of message
    /// being dispatched/send.
    /// </summary>
    // ReSharper disable once UnusedTypeParameter - Generic type parameter is needed
    public class ServiceBusDispatcher : IServiceBusDispatcher
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly string _topicName;

        public ServiceBusDispatcher(ServiceBusClient serviceBusClient, string topicName)
        {
            _serviceBusClient = serviceBusClient;
            _topicName = topicName;
        }

        public async Task DispatchAsync(ServiceBusMessage serviceBusMessage)
        {
            var sender = _serviceBusClient.CreateSender(_topicName);
            await sender.SendMessageAsync(serviceBusMessage).ConfigureAwait(false);
        }
    }
}
