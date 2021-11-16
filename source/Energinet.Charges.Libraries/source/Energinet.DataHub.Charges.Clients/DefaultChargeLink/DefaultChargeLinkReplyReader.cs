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
using Energinet.DataHub.Charges.Libraries.Mappers;

namespace Energinet.DataHub.Charges.Libraries.DefaultChargeLink
{
    /// <inheritdoc/>
    public sealed class DefaultChargeLinkReplyReader : IDefaultChargeLinkReplyReader
    {
        private readonly OnSuccess _handleSuccess;
        private readonly OnFailure _handleFailure;

        /// <param name="handleSuccess">Delegate to handle successful <see cref="CreateDefaultChargeLinks" /> request</param>
        /// <param name="handleFailure">Delegate to handle failed <see cref="CreateDefaultChargeLinks" /> request</param>
        public DefaultChargeLinkReplyReader(
            [DisallowNull] OnSuccess handleSuccess,
            [DisallowNull] OnFailure handleFailure)
        {
            _handleSuccess = handleSuccess;
            _handleFailure = handleFailure;
        }

        /// <inheritdoc/>
        public async Task ReadAsync([DisallowNull] byte[] serializedReplyMessageBody)
        {
            if (serializedReplyMessageBody == null)
                throw new ArgumentNullException(nameof(serializedReplyMessageBody));

            var replyParser = CreateDefaultChargeLinksReply.Parser;
            var createDefaultChargeLinksReply = replyParser.ParseFrom(serializedReplyMessageBody);

            await MapAsync(createDefaultChargeLinksReply).ConfigureAwait(false);
        }

        private async Task MapAsync(CreateDefaultChargeLinksReply createDefaultChargeLinksReply)
        {
            switch (createDefaultChargeLinksReply.ReplyCase)
            {
                case CreateDefaultChargeLinksReply.ReplyOneofCase.None:
                    throw new ArgumentException($"Unknown type: {nameof(createDefaultChargeLinksReply.ReplyCase)}");

                case CreateDefaultChargeLinksReply.ReplyOneofCase.CreateDefaultChargeLinksSucceeded:
                    var succeededDto = CreateDefaultChargeLinksSucceededInboundMapper
                        .Convert(createDefaultChargeLinksReply);

                    await _handleSuccess(succeededDto).ConfigureAwait(false);
                    break;

                case CreateDefaultChargeLinksReply.ReplyOneofCase.CreateDefaultChargeLinksFailed:
                    var failedDto = CreateDefaultChargeLinksFailedInboundMapper
                        .Convert(createDefaultChargeLinksReply);

                    await _handleFailure(failedDto).ConfigureAwait(false);
                    break;

                default:
                    throw new ArgumentException($"Unknown type: {nameof(createDefaultChargeLinksReply.ReplyCase)}");
            }
        }
    }
}
