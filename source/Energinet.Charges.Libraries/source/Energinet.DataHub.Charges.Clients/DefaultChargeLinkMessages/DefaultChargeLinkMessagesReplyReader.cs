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
using Energinet.DataHub.Charges.Libraries.Protobuf;

namespace Energinet.DataHub.Charges.Libraries.DefaultChargeLinkMessages
{
    public sealed class DefaultChargeLinkMessagesReplyReader : DefaultChargeLinkMessagesReplyReaderBase
    {
        private readonly OnSuccess _handleSuccess;
        private readonly OnFailure _handleFailure;

        /// <summary>
        /// Provides functionality to read and map data received from a reply to
        /// a <see cref="CreateDefaultChargeLinkMessages" /> request. Caller must provide
        /// delegates intended to handle handle replies for successful and failed
        /// requests.
        /// </summary>
        /// <param name="handleSuccess">Delegate to handle successful <see cref="CreateDefaultChargeLinkMessages" /> request</param>
        /// <param name="handleFailure">Delegate to handle failed <see cref="CreateDefaultChargeLinkMessages" /> request</param>
        public DefaultChargeLinkMessagesReplyReader([NotNull] OnSuccess handleSuccess, [NotNull] OnFailure handleFailure)
        {
            _handleSuccess = handleSuccess;
            _handleFailure = handleFailure;
        }

        /// <summary>
        /// Read and map data to be handled by provided delegates.
        /// </summary>
        /// <param name="data">Data reply to deserialize.</param>
        public override async Task ReadAsync([NotNull] byte[] data)
        {
            var replyParser = CreateDefaultChargeLinkMessagesReply.Parser;
            var createDefaultChargeLinkMessagesReply = replyParser.ParseFrom(data);

            await MapAsync(createDefaultChargeLinkMessagesReply).ConfigureAwait(false);
        }

        private async Task MapAsync([NotNull] CreateDefaultChargeLinkMessagesReply createDefaultChargeLinkMessagesReply)
        {
            switch (createDefaultChargeLinkMessagesReply.ReplyCase)
            {
                case CreateDefaultChargeLinkMessagesReply.ReplyOneofCase.None:
                    throw new ArgumentException(
                        $"Unknown type: {nameof(createDefaultChargeLinkMessagesReply.ReplyCase)}");

                case CreateDefaultChargeLinkMessagesReply.ReplyOneofCase.CreateDefaultChargeLinkMessagesSucceeded:
                    var succeededDto = CreateDefaultChargeLinkMessagesSucceededInboundMapper
                        .Convert(createDefaultChargeLinkMessagesReply);

                    await _handleSuccess(succeededDto).ConfigureAwait(false);
                    break;

                case CreateDefaultChargeLinkMessagesReply.ReplyOneofCase.CreateDefaultChargeLinkMessagesFailed:
                    var failedDto = CreateDefaultChargeLinkMessagesFailedInboundMapper
                        .Convert(createDefaultChargeLinkMessagesReply);

                    await _handleFailure(failedDto).ConfigureAwait(false);
                    break;

                default:
                    throw new ArgumentException(
                        $"Unknown type: {nameof(createDefaultChargeLinkMessagesReply.ReplyCase)}");
            }
        }
    }
}
