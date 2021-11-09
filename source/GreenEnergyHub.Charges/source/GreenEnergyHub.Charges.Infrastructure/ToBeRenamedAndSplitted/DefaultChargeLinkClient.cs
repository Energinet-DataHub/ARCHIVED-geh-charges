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
using GreenEnergyHub.Charges.Application.ToBeRenamedAndSplitted;
using GreenEnergyHub.Charges.InternalShared;

namespace GreenEnergyHub.Charges.Infrastructure.ToBeRenamedAndSplitted
{
    /// <summary>
    /// This class must be thread safe.
    /// </summary>
    public sealed class DefaultChargeLinkClient : IDefaultChargeLinkClient
    {
        private readonly IServiceBusReplySenderProvider _serviceBusReplySenderProvider;

        public DefaultChargeLinkClient([NotNull] IServiceBusReplySenderProvider serviceBusReplySenderProvider)
        {
            _serviceBusReplySenderProvider = serviceBusReplySenderProvider;
        }

        public async Task CreateDefaultChargeLinksSucceededReplyAsync(
            [NotNull] string meteringPointId,
            bool didCreateChargeLinks,
            string replyTo,
            string correlationId)
        {
            if (meteringPointId == null)
                throw new ArgumentNullException(nameof(meteringPointId));

            var createDefaultChargeLinks = new CreateDefaultChargeLinksReply
            {
                MeteringPointId = meteringPointId,
                CreateDefaultChargeLinksSucceeded =
                    new CreateDefaultChargeLinksReply.Types.CreateDefaultChargeLinksSucceeded
                {
                    DidCreateChargeLinks = didCreateChargeLinks,
                },
            };

            await SendReplyAsync(replyTo, correlationId, createDefaultChargeLinks);
        }

        public async Task CreateDefaultChargeLinksFailedReplyAsync(
            [NotNull] string meteringPointId,
            ErrorCode errorCode,
            string replyTo,
            string correlationId)
        {
            if (meteringPointId == null)
                throw new ArgumentNullException(nameof(meteringPointId));

            var createDefaultChargeLinks = new CreateDefaultChargeLinksReply
            {
                MeteringPointId = meteringPointId,
                CreateDefaultChargeLinksFailed =
                    new CreateDefaultChargeLinksReply.Types.CreateDefaultChargeLinksFailed
                    {
                        ErrorCode =
                            (CreateDefaultChargeLinksReply.Types.CreateDefaultChargeLinksFailed.Types.ErrorCode)errorCode,
                    },
            };

            await SendReplyAsync(replyTo, correlationId, createDefaultChargeLinks);
        }

        private async Task SendReplyAsync(
            string replyTo,
            string correlationId,
            CreateDefaultChargeLinksReply createDefaultChargeLinks)
        {
            var sender = _serviceBusReplySenderProvider.GetInstance(replyTo);

            await sender.SendReplyAsync(createDefaultChargeLinks.ToByteArray(), correlationId)
                .ConfigureAwait(false);
        }
    }
}
