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
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace Energinet.DataHub.Charges.Clients.ServiceBus
{
    internal sealed class ServiceBusRequestSender : IServiceBusRequestSender
    {
        private readonly ServiceBusSender _serviceBusSender;
        private readonly string _replyToQueueName;

        public ServiceBusRequestSender(ServiceBusSender serviceBusSender, string replyToQueueName)
        {
            _serviceBusSender = serviceBusSender;
            _replyToQueueName = replyToQueueName;
        }

        public async Task SendRequestAsync(byte[] data, string correlationId)
        {
            await _serviceBusSender.SendMessageAsync(new ServiceBusMessage
            {
                Body = new BinaryData(data),
                ApplicationProperties = { new KeyValuePair<string, object>("ReplyTo", _replyToQueueName) },
                CorrelationId = correlationId,
            }).ConfigureAwait(false);
        }
    }
}
