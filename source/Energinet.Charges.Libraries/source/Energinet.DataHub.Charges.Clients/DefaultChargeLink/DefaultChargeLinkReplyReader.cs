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
using Energinet.DataHub.Charges.Clients.DefaultChargeLink.Mappers;
using Energinet.DataHub.Charges.Clients.DefaultChargeLink.Models;

namespace Energinet.DataHub.Charges.Clients.DefaultChargeLink
{
    /// <summary>
    /// Delegate that will be invoked by IDefaultChargeLinkReplyReader ReadAsync() when the
    /// serializedReplyMessageBody is a reply containing a CreateDefaultChargeLinksSucceeded.
    ///
    /// Consuming domain should implement this delegate to handle further processing following
    /// successful Default Charge Link creation.
    /// </summary>
    public delegate Task OnSuccess(DefaultChargeLinksCreatedSuccessfullyDto defaultChargeLinksCreatedSuccessfully);

    /// <summary>
    /// Delegate that will be invoked by IDefaultChargeLinkReplyReader ReadAsync() when the
    /// serializedReplyMessageBody is a reply containing a CreateDefaultChargeLinksFailed.
    ///
    /// Consuming domain should implement this delegate to handle further processing following
    /// failed Default Charge Link creation.
    /// </summary>
    public delegate Task OnFailure(DefaultChargeLinksCreationFailedStatusDto defaultChargeLinksCreationSucceeded);

    /// <summary>
    /// Provides functionality to read and map data received from a reply to a
    /// <see cref="CreateDefaultChargeLinks" /> request. Caller must provide delegates
    /// intended to handle handle replies for successful and failed requests.
    /// </summary>
    public sealed class DefaultChargeLinkReplyReader
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

        /// <summary>
        /// Read and map data to be handled by provided delegates.
        ///
        /// ReadAsync method will invoke either OnSuccess or OnFailure delegate depending on the content
        /// of the serializedReplyMessageBody.
        /// <param name="serializedReplyMessageBody">Reply message to deserialize</param>
        /// </summary>
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
