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
using Energinet.Charges.Contracts;
using Energinet.DataHub.Charges.Libraries.Models;
using Energinet.DataHub.Charges.Libraries.Providers;
using Energinet.DataHub.Charges.Libraries.ServiceBus;
using Google.Protobuf;
using CreateDefaultChargeLinksFailed =
    Energinet.Charges.Contracts.CreateDefaultChargeLinksReply.Types.CreateDefaultChargeLinksFailed;
using CreateDefaultChargeLinksSucceeded =
    Energinet.Charges.Contracts.CreateDefaultChargeLinksReply.Types.CreateDefaultChargeLinksSucceeded;

namespace Energinet.DataHub.Charges.Libraries.DefaultChargeLink
{
    public sealed class DefaultChargeLinkClient : IDefaultChargeLinkClient
    {
        private readonly IServiceBusRequestSender _serviceBusRequestSender;

        public DefaultChargeLinkClient(
            [NotNull] IDefaultChargeLinkClientServiceBusRequestSenderProvider serviceBusRequestSenderProvider)
        {
            _serviceBusRequestSender = serviceBusRequestSenderProvider.GetInstance();
        }

        public async Task CreateDefaultChargeLinksRequestAsync(
            [NotNull] CreateDefaultChargeLinksDto createDefaultChargeLinksDto,
            [NotNull] string correlationId)
        {
            if (createDefaultChargeLinksDto == null)
                throw new ArgumentNullException(nameof(createDefaultChargeLinksDto));

            if (string.IsNullOrWhiteSpace(correlationId))
                throw new ArgumentNullException(nameof(correlationId));

            var createDefaultChargeLinks = new CreateDefaultChargeLinks
            {
                MeteringPointId = createDefaultChargeLinksDto.MeteringPointId,
            };

            await _serviceBusRequestSender.SendRequestAsync(
                createDefaultChargeLinks.ToByteArray(), correlationId)
                .ConfigureAwait(false);
        }

        public async Task CreateDefaultChargeLinksSucceededReplyAsync(
            [NotNull] CreateDefaultChargeLinksSucceededDto createDefaultChargeLinksSucceededDto,
            [NotNull] string correlationId,
            [NotNull] string replyQueueName)
        {
            if (createDefaultChargeLinksSucceededDto == null)
                throw new ArgumentNullException(nameof(createDefaultChargeLinksSucceededDto));

            if (string.IsNullOrWhiteSpace(correlationId))
                throw new ArgumentNullException(nameof(correlationId));

            if (string.IsNullOrWhiteSpace(replyQueueName))
                throw new ArgumentNullException(nameof(replyQueueName));

            var createDefaultChargeLinks = new CreateDefaultChargeLinksReply
            {
                MeteringPointId = createDefaultChargeLinksSucceededDto.MeteringPointId,
                CreateDefaultChargeLinksSucceeded = new CreateDefaultChargeLinksSucceeded
                {
                    DidCreateChargeLinks = createDefaultChargeLinksSucceededDto.DidCreateChargeLinks,
                },
            };

            await _serviceBusRequestSender.SendRequestAsync(
                    createDefaultChargeLinks.ToByteArray(), correlationId)
                .ConfigureAwait(false);
        }

        public async Task CreateDefaultChargeLinksFailedReplyAsync(
            [NotNull] CreateDefaultChargeLinksFailedDto createDefaultChargeLinksFailedDto,
            [NotNull] string correlationId,
            [NotNull] string replyQueueName)
        {
            if (createDefaultChargeLinksFailedDto == null)
                throw new ArgumentNullException(nameof(createDefaultChargeLinksFailedDto));

            if (string.IsNullOrWhiteSpace(correlationId))
                throw new ArgumentNullException(nameof(correlationId));

            if (string.IsNullOrWhiteSpace(replyQueueName))
                throw new ArgumentNullException(nameof(replyQueueName));

            var createDefaultChargeLinks = new CreateDefaultChargeLinksReply
            {
                MeteringPointId = createDefaultChargeLinksFailedDto.MeteringPointId,
                CreateDefaultChargeLinksFailed = new CreateDefaultChargeLinksFailed
                {
                    ErrorCode = (CreateDefaultChargeLinksFailed.Types.ErrorCode)createDefaultChargeLinksFailedDto.ErrorCode,
                },
            };

            await _serviceBusRequestSender.SendRequestAsync(
                    createDefaultChargeLinks.ToByteArray(), correlationId)
                .ConfigureAwait(false);
        }
    }
}
