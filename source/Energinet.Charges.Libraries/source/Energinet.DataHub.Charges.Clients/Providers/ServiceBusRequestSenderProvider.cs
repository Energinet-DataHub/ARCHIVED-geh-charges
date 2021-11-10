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
using Energinet.DataHub.Charges.Libraries.Models;
using Energinet.DataHub.Charges.Libraries.ServiceBus;

namespace Energinet.DataHub.Charges.Libraries.Providers
{
    public class ServiceBusRequestSenderProvider : IServiceBusRequestSenderProvider
    {
        private const string RequestQueueName = "create-link-request";
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IServiceBusRequestSenderConfiguration _serviceBusRequestSenderConfiguration;
        private readonly bool _useTestConfiguration;
        private ServiceBusRequestSender? _sender;

        public ServiceBusRequestSenderProvider(
            ServiceBusClient serviceBusClient,
            IServiceBusRequestSenderConfiguration serviceBusRequestSenderConfiguration)
        {
            _serviceBusClient = serviceBusClient;
            _serviceBusRequestSenderConfiguration = serviceBusRequestSenderConfiguration;
            _sender = null;
            if (_serviceBusRequestSenderConfiguration is ServiceBusRequestSenderTestConfiguration)
                _useTestConfiguration = true;
        }

        public IServiceBusRequestSender GetInstance()
        {
            if (_useTestConfiguration)
            {
                var serviceBusRequestSenderTestConfiguration =
                    (ServiceBusRequestSenderTestConfiguration)_serviceBusRequestSenderConfiguration;
                _sender ??=
                    new ServiceBusRequestSender(
                        _serviceBusClient.CreateSender(serviceBusRequestSenderTestConfiguration.RequestQueueName),
                        serviceBusRequestSenderTestConfiguration.ReplyQueueName);
            }

            return _sender ??=
                new ServiceBusRequestSender(
                    _serviceBusClient.CreateSender(RequestQueueName),
                    _serviceBusRequestSenderConfiguration.ReplyQueueName);
        }
    }
}
