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
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.MessageHub.Client.Factories;
using Energinet.DataHub.MessageHub.Client.Model;
using Energinet.DataHub.MessageHub.Client.Peek;

namespace GreenEnergyHub.Charges.IntegrationTests.Fixtures
{
    public class MessageHubMock
    {
        private readonly string _requestQueueName;
        private readonly string _replyQueueName;
        private readonly IServiceBusClientFactory _serviceBusClientFactory;
        private ServiceBusClient? _serviceBusClient;

        public MessageHubMock(string connectionString, string requestQueueName, string replyQueueName)
        {
            _requestQueueName = requestQueueName;
            _replyQueueName = replyQueueName;
            _serviceBusClientFactory = new ServiceBusClientFactory(connectionString);
        }

        public async Task RequestBundleAsync(DataAvailableNotificationDto dataAvailableNotification)
        {
            var idempotencyId = Guid.NewGuid().ToString();
            var bundleRequestDto = new DataBundleRequestDto(idempotencyId, new[] { dataAvailableNotification.Uuid });
            var bundleRequest = new RequestBundleParser().Parse(bundleRequestDto);

            var sessionId = Guid.NewGuid().ToString();

            var serviceBusMessage = new ServiceBusMessage(bundleRequest)
            {
                SessionId = sessionId,
                ReplyToSessionId = sessionId,
                ReplyTo = _replyQueueName,
            };

            _serviceBusClient ??= _serviceBusClientFactory.Create();

            await using var sender = _serviceBusClient.CreateSender(_requestQueueName);
            await sender.SendMessageAsync(serviceBusMessage).ConfigureAwait(false);
        }
    }
}
