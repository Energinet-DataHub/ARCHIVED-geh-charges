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
using Energinet.Charges.Contracts;
using Energinet.DataHub.Charges.Libraries.Factories;
using Energinet.DataHub.Charges.Libraries.Models;
using Energinet.DataHub.Charges.Libraries.ServiceBus;
using Google.Protobuf;

namespace Energinet.DataHub.Charges.Libraries.DefaultChargeLinkMessages
{
    public sealed class DefaultChargeLinkMessagesRequestClient : IAsyncDisposable, IDefaultChargeLinkMessagesRequestClient
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly string _createLinkMessagesRequestQueueName;
        private readonly IServiceBusRequestSender _serviceBusRequestSender;

        public DefaultChargeLinkMessagesRequestClient(
            [NotNull] ServiceBusClient serviceBusClient,
            [NotNull] IServiceBusRequestSenderFactory serviceBusRequestSenderFactory,
            [NotNull] string replyToQueueName,
            [NotNull] string createLinkMessagesRequestQueueName = "create-link-messages-request")
        {
            _serviceBusClient = serviceBusClient;
            _createLinkMessagesRequestQueueName = createLinkMessagesRequestQueueName;
            _serviceBusRequestSender = serviceBusRequestSenderFactory.Create(serviceBusClient, replyToQueueName);
        }

        public async Task CreateDefaultChargeLinkMessagesRequestAsync(
            [NotNull] CreateDefaultChargeLinkMessagesDto createDefaultChargeLinkMessagesDto,
            [NotNull] string correlationId)
        {
            if (createDefaultChargeLinkMessagesDto == null)
                throw new ArgumentNullException(nameof(createDefaultChargeLinkMessagesDto));

            if (string.IsNullOrWhiteSpace(correlationId))
                throw new ArgumentNullException(nameof(correlationId));

            var createDefaultChargeLinkMessages = new CreateDefaultChargeLinkMessages
            {
                MeteringPointId = createDefaultChargeLinkMessagesDto.MeteringPointId,
            };

            await _serviceBusRequestSender.SendRequestAsync(
                    createDefaultChargeLinkMessages.ToByteArray(), _createLinkMessagesRequestQueueName, correlationId)
                .ConfigureAwait(false);
        }

        public async Task CreateDefaultChargeLinkMessagesSucceededRequestAsync(
            [NotNull] CreateDefaultChargeLinkMessagesSucceededDto createDefaultChargeLinkMessagesSucceededDto,
            [NotNull] string correlationId,
            [NotNull] string replyQueueName)
        {
            if (createDefaultChargeLinkMessagesSucceededDto == null)
                throw new ArgumentNullException(nameof(createDefaultChargeLinkMessagesSucceededDto));

            if (string.IsNullOrWhiteSpace(correlationId))
                throw new ArgumentNullException(nameof(correlationId));

            if (string.IsNullOrWhiteSpace(replyQueueName))
                throw new ArgumentNullException(nameof(replyQueueName));

            var createDefaultChargeLinks = new CreateDefaultChargeLinkMessagesSucceeded
            {
                MeteringPointId = createDefaultChargeLinkMessagesSucceededDto.MeteringPointId,
            };

            await _serviceBusRequestSender.SendRequestAsync(
                    createDefaultChargeLinks.ToByteArray(), replyQueueName, correlationId)
                .ConfigureAwait(false);
        }

        public async Task CreateDefaultChargeLinkMessagesFailedRequestAsync(
            [NotNull] CreateDefaultChargeLinkMessagesFailedDto createDefaultChargeLinkMessagesFailedDto,
            [NotNull] string correlationId,
            [NotNull] string replyQueueName)
        {
            if (createDefaultChargeLinkMessagesFailedDto == null)
                throw new ArgumentNullException(nameof(createDefaultChargeLinkMessagesFailedDto));

            if (string.IsNullOrWhiteSpace(correlationId))
                throw new ArgumentNullException(nameof(correlationId));

            if (string.IsNullOrWhiteSpace(replyQueueName))
                throw new ArgumentNullException(nameof(replyQueueName));

            var createDefaultChargeLinks = new CreateDefaultChargeLinkMessagesFailed
            {
                MeteringPointId = createDefaultChargeLinkMessagesFailedDto.MeteringPointId,
                ErrorCode =
                    (CreateDefaultChargeLinkMessagesFailed.Types.ErrorCode)createDefaultChargeLinkMessagesFailedDto.ErrorCode,
            };

            await _serviceBusRequestSender.SendRequestAsync(
                    createDefaultChargeLinks.ToByteArray(), replyQueueName, correlationId)
                .ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            await _serviceBusClient.DisposeAsync().ConfigureAwait(false);
        }
    }
}
