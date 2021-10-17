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
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace GreenEnergyHub.DataHub.Charges.Libraries.ServiceBus
{
    public sealed class ServiceBusRequestSender : IServiceBusRequestSender
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly string _replyToQueueName;

        public ServiceBusRequestSender([NotNull] ServiceBusClient serviceBusClient, [NotNull] string replyToQueueName)
        {
            _serviceBusClient = serviceBusClient;
            _replyToQueueName = replyToQueueName;
        }

        public async Task SendRequestAsync([NotNull] byte[] data, [NotNull] string requestQueueName, [NotNull] string correlationId)
        {
            await using var sender = _serviceBusClient.CreateSender(requestQueueName);

            await sender.SendMessageAsync(new ServiceBusMessage
            {
                Body = new BinaryData(data),
                ReplyTo = _replyToQueueName,
                CorrelationId = correlationId,
            }).ConfigureAwait(false);
        }
    }
}
