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
using Google.Protobuf;
using GreenEnergyHub.Charges.Commands;

namespace GreenEnergyHub.DataHub.Charges.Libraries.DefaultChargeLinkRequest
{
    public sealed class DefaultChargeLinkRequestClient : IAsyncDisposable, IDefaultChargeLinkRequestClient
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly string _respondQueueName;

        public DefaultChargeLinkRequestClient(
            [NotNull] string serviceBusConnectionString,
            [NotNull] string respondQueueName)
        {
            _serviceBusClient = new ServiceBusClient(serviceBusConnectionString);
            _respondQueueName = respondQueueName;
        }

        public async Task CreateDefaultChargeLinksRequestAsync(
            [NotNull] string meteringPointId,
            [NotNull] string correlationId)
        {
            await using var sender = _serviceBusClient.CreateSender("sbt-create-link-command");

            var createDefaultChargeLinks = new CreateDefaultChargeLinks { MeteringPointId = meteringPointId };

            await sender.SendMessageAsync(new ServiceBusMessage
            {
                Body = new BinaryData(createDefaultChargeLinks.ToByteArray()),
                ReplyTo = _respondQueueName,
                CorrelationId = correlationId,
            }).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            await _serviceBusClient.DisposeAsync().ConfigureAwait(false);
        }
    }
}
