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
using Google.Protobuf;
using GreenEnergyHub.Charges.Contracts;
using GreenEnergyHub.DataHub.Charges.Libraries.Factories;
using GreenEnergyHub.DataHub.Charges.Libraries.Models;

namespace GreenEnergyHub.DataHub.Charges.Libraries.DefaultChargeLinkRequest
{
    public sealed class DefaultChargeLinkRequestClient : IAsyncDisposable, IDefaultChargeLinkRequestClient
    {
        private const string CreateLinkRequestQueueName = "create-link-request";
        private readonly IServiceBusClientFactory _serviceBusClientFactory;
        private readonly string _responseQueue;
        private ServiceBusClient? _serviceBusClient;

        public DefaultChargeLinkRequestClient(IServiceBusClientFactory serviceBusClientFactory, string responseQueue)
        {
            _serviceBusClientFactory = serviceBusClientFactory;
            _responseQueue = responseQueue;
        }

        public async Task CreateDefaultChargeLinksRequestAsync(
            CreateDefaultChargeLinksDto createDefaultChargeLinksDto,
            string correlationId)
        {
            if (createDefaultChargeLinksDto == null)
                throw new ArgumentNullException(nameof(createDefaultChargeLinksDto));

            if (string.IsNullOrWhiteSpace(correlationId))
                throw new ArgumentNullException(nameof(correlationId));

            _serviceBusClient ??= _serviceBusClientFactory.Create();

            await using var sender = _serviceBusClient.CreateSender(CreateLinkRequestQueueName);

            var createDefaultChargeLinks = new CreateDefaultChargeLinks
            {
                MeteringPointId = createDefaultChargeLinksDto.meteringPointId,
            };

            await sender.SendMessageAsync(new ServiceBusMessage
            {
                Body = new BinaryData(createDefaultChargeLinks.ToByteArray()),
                ReplyTo = _responseQueue,
                CorrelationId = correlationId,
            }).ConfigureAwait(false);
        }

        // public async Task CreateDefaultChargeLinksReplyAsync()
        // {
        //
        // }
        public async ValueTask DisposeAsync()
        {
            if (_serviceBusClient != null)
            {
                await _serviceBusClient.DisposeAsync().ConfigureAwait(false);
                _serviceBusClient = null;
            }
        }
    }
}
