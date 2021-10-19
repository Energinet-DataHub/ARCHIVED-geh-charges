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
using Energinet.DataHub.Charges.Libraries.Enums;
using Energinet.DataHub.Charges.Libraries.Protobuf;

namespace Energinet.DataHub.Charges.Libraries.DefaultChargeLink
{
    public sealed class DefaultChargeLinkReplyReader : DefaultChargeLinkReplyReaderBase
    {
        private readonly OnSuccess _handleSuccess;
        private readonly OnFailure _handleFailure;

        /// <summary>
        /// Provides functionality to read and map data received from a reply to
        /// a <see cref="CreateDefaultChargeLinks" /> request. Caller must provide
        /// delegates intended to handle handle replies for successful and failed
        /// requests.
        /// </summary>
        /// <param name="handleSuccess">Delegate to handle successful <see cref="CreateDefaultChargeLinks" /> request</param>
        /// <param name="handleFailure">Delegate to handle failed <see cref="CreateDefaultChargeLinks" /> request</param>
        public DefaultChargeLinkReplyReader([NotNull] OnSuccess handleSuccess, [NotNull] OnFailure handleFailure)
        {
            _handleSuccess = handleSuccess;
            _handleFailure = handleFailure;
        }

        /// <summary>
        /// Read and map data to be handled by provided delegates.
        /// </summary>
        /// <param name="data">Data reply to deserialized</param>
        /// <param name="requestStatus">
        /// MessageType contains information on whether data contains
        /// a reply to a succeeded failed <see cref="CreateDefaultChargeLinks" /> request
        /// </param>
        public override async Task ReadAsync([NotNull] byte[] data, [NotNull] RequestStatus requestStatus)
        {
            switch (requestStatus)
            {
                case RequestStatus.Succeeded:
                    var succeededParser = CreateDefaultChargeLinksSucceeded.Parser;
                    var createDefaultChargeLinksSucceeded = succeededParser.ParseFrom(data);
                    var succeededDto = CreateDefaultChargeLinksSucceededInboundMapper
                        .Convert(createDefaultChargeLinksSucceeded);

                    await _handleSuccess(succeededDto).ConfigureAwait(false);
                    break;

                case RequestStatus.Failed:
                    var failedParser = CreateDefaultChargeLinksFailed.Parser;
                    var createDefaultChargeLinksFailed = failedParser.ParseFrom(data);
                    var failedDto = CreateDefaultChargeLinksFailedInboundMapper
                        .Convert(createDefaultChargeLinksFailed);

                    await _handleFailure(failedDto).ConfigureAwait(false);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(requestStatus), requestStatus, null);
            }
        }
    }
}
