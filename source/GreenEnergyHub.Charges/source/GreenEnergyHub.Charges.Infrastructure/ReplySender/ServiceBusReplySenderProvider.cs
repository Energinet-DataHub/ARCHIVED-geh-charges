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

using System.Collections.Concurrent;
using Azure.Messaging.ServiceBus;

namespace GreenEnergyHub.Charges.Infrastructure.ReplySender
{
    public class ServiceBusReplySenderProvider : IServiceBusReplySenderProvider
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ConcurrentDictionary<string, ServiceBusReplySender?> _senders;

        public ServiceBusReplySenderProvider(ServiceBusClient serviceBusClient)
        {
            _serviceBusClient = serviceBusClient;
            _senders = new ConcurrentDictionary<string, ServiceBusReplySender?>();
        }

        public IServiceBusReplySender GetInstance(string replyTo)
        {
            _senders.TryGetValue(replyTo, out var serviceBusReplySender);

            if (serviceBusReplySender != null)
                return serviceBusReplySender;

            serviceBusReplySender = new ServiceBusReplySender(_serviceBusClient.CreateSender(replyTo));

            _senders.TryAdd(replyTo, serviceBusReplySender);

            return serviceBusReplySender;
        }
    }
}
