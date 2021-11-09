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
using Google.Protobuf;
using GreenEnergyHub.Charges.Application;
using GreenEnergyHub.Charges.Application.ToBeRenamedAndSplitted;
using GreenEnergyHub.Charges.InternalShared;

namespace GreenEnergyHub.Charges.Infrastructure.ToBeRenamedAndSplitted
{
    /// <summary>
    /// This class must be thread safe.
    /// </summary>
    public sealed class DefaultChargeLinkClient : IDefaultChargeLinkClient
    {
        private readonly IServiceBusRequestSenderProvider _serviceBusRequestSenderProvider;
        private readonly ICorrelationContext _correlationContext;

        public DefaultChargeLinkClient(
            [NotNull] IServiceBusRequestSenderProvider serviceBusRequestSenderProvider,
            ICorrelationContext correlationContext)
        {
            _serviceBusRequestSenderProvider = serviceBusRequestSenderProvider;
            _correlationContext = correlationContext;
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
                CreateDefaultChargeLinksSucceeded = new CreateDefaultChargeLinksReply.Types.CreateDefaultChargeLinksSucceeded
                {
                    DidCreateChargeLinks = createDefaultChargeLinksSucceededDto.DidCreateChargeLinks,
                },
            };

            var sender = _serviceBusRequestSenderProvider.GetInstance();

            await sender.SendReplyAsync(createDefaultChargeLinks.ToByteArray(), _correlationContext.Id)
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
                CreateDefaultChargeLinksFailed = new CreateDefaultChargeLinksReply.Types.CreateDefaultChargeLinksFailed
                {
                    ErrorCode = (CreateDefaultChargeLinksReply.Types.CreateDefaultChargeLinksFailed.Types.ErrorCode)createDefaultChargeLinksFailedDto.ErrorCode,
                },
            };

            var sender = _serviceBusRequestSenderProvider.GetInstance();

            await sender.SendReplyAsync(createDefaultChargeLinks.ToByteArray(), _correlationContext.Id)
                .ConfigureAwait(false);
        }
    }
}
