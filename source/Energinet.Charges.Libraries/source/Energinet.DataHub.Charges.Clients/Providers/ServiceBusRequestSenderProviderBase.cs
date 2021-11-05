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
using Energinet.DataHub.Charges.Libraries.ServiceBus;

namespace Energinet.DataHub.Charges.Libraries.Providers
{
    public abstract class ServiceBusRequestSenderProviderBase
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly string _replyToQueueName;
        private readonly string _requestQueueName;
        private ServiceBusRequestSender? _sender;

        protected ServiceBusRequestSenderProviderBase(
            ServiceBusClient serviceBusClient,
            string replyToQueueName,
            string requestQueueName)
        {
            _serviceBusClient = serviceBusClient;
            _replyToQueueName = replyToQueueName;
            _requestQueueName = requestQueueName;
            _sender = null;
        }

        public IServiceBusRequestSender GetInstance()
        {
            return _sender ??=
                new ServiceBusRequestSender(_serviceBusClient.CreateSender(_requestQueueName), _replyToQueueName);
        }
    }
}
