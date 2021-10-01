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
using GreenEnergyHub.Charges.Commands;

namespace GreenEnergyHub.DataHub.Charges.Libraries.DefaultChargeLinkRequest
{
    public sealed class DefaultChargeLinkRequestClient : IAsyncDisposable, IDefaultChargeLinkRequestClient
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly string _respondQueue;

        public DefaultChargeLinkRequestClient(string serviceBusConnectionString, string respondQueue)
        {
            _serviceBusClient = new ServiceBusClient(serviceBusConnectionString);
            _respondQueue = respondQueue;
        }

        public async Task CreateDefaultChargeLinksRequestAsync(
            string meteringPointId,
            string correlationId)
        {
            if (meteringPointId == null)
                throw new ArgumentNullException(nameof(meteringPointId));

            await using var sender = _serviceBusClient.CreateSender("create-link-command");

            var createDefaultChargeLinks = new CreateDefaultChargeLinks { MeteringPointId = meteringPointId };

            await sender.SendMessageAsync(new ServiceBusMessage
            {
                Body = new BinaryData(createDefaultChargeLinks.ToByteArray()),
                ReplyTo = _respondQueue,
                CorrelationId = correlationId,
            }).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            await _serviceBusClient.DisposeAsync().ConfigureAwait(false);
        }
    }
}
