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
using GreenEnergyHub.Charges.Contracts;
using GreenEnergyHub.DataHub.Charges.Libraries.Factories;
using GreenEnergyHub.DataHub.Charges.Libraries.Models;
using GreenEnergyHub.DataHub.Charges.Libraries.ServiceBus;

namespace GreenEnergyHub.DataHub.Charges.Libraries.DefaultChargeLinkMessages
{
    public sealed class DefaultChargeLinkMessagesRequestClient : IAsyncDisposable, IDefaultChargeLinkMessagesRequestClient
    {
        private const string CreateLinkMessagesRequestQueueName = "create-link-messages-request";
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IServiceBusRequestSender _serviceBusRequestSender;

        public DefaultChargeLinkMessagesRequestClient(
            [NotNull] ServiceBusClient serviceBusClient,
            [NotNull] IServiceBusRequestSenderFactory serviceBusRequestSenderFactory,
            string replyToQueueName)
        {
            _serviceBusClient = serviceBusClient;
            _serviceBusRequestSender = serviceBusRequestSenderFactory.Create(
                serviceBusClient, CreateLinkMessagesRequestQueueName, replyToQueueName);
        }

        public async Task CreateDefaultChargeLinkMessagesRequestAsync(
            CreateDefaultChargeLinkMessagesDto createDefaultChargeLinkMessagesDto,
            string correlationId)
        {
            if (createDefaultChargeLinkMessagesDto == null)
                throw new ArgumentNullException(nameof(createDefaultChargeLinkMessagesDto));

            if (string.IsNullOrWhiteSpace(correlationId))
                throw new ArgumentNullException(nameof(correlationId));

            var createDefaultChargeLinkMessages = new CreateDefaultChargeLinkMessages
            {
                MeteringPointId = createDefaultChargeLinkMessagesDto.meteringPointId,
            };

            await _serviceBusRequestSender.SendRequestAsync(
                    createDefaultChargeLinkMessages.ToByteArray(), correlationId).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            await _serviceBusClient.DisposeAsync().ConfigureAwait(false);
        }
    }
}
