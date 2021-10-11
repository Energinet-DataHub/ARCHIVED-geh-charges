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
using GreenEnergyHub.Charges.Contracts;
using GreenEnergyHub.DataHub.Charges.Libraries.Enums;
using GreenEnergyHub.DataHub.Charges.Libraries.Protobuf;

namespace GreenEnergyHub.DataHub.Charges.Libraries.DefaultChargeLinkReply
{
    public sealed class DefaultChargeLinkReplyDeserializer : DefaultChargeLinkReplyDeserializerBase
    {
        private readonly OnSuccess _handleSuccess;
        private readonly OnFailure _handleFailure;

        public DefaultChargeLinkReplyDeserializer([NotNull] OnSuccess handleSuccess, [NotNull] OnFailure handleFailure)
        {
            _handleSuccess = handleSuccess;
            _handleFailure = handleFailure;
        }

        public override async Task DeserializeMessageAsync(byte[] data, MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.RequestSucceeded:
                    var succeededParser = CreateDefaultChargeLinksSucceeded.Parser;
                    var createDefaultChargeLinksSucceeded = succeededParser.ParseFrom(data);
                    var succeededDto = CreateDefaultChargeLinksSucceededInboundMapper
                        .Convert(createDefaultChargeLinksSucceeded);

                    await _handleSuccess(succeededDto).ConfigureAwait(false);
                    break;

                case MessageType.RequestFailed:
                    var failedParser = CreateDefaultChargeLinksFailed.Parser;
                    var createDefaultChargeLinksFailed = failedParser.ParseFrom(data);
                    var failedDto = CreateDefaultChargeLinksFailedInboundMapper
                        .Convert(createDefaultChargeLinksFailed);

                    await _handleFailure(failedDto).ConfigureAwait(false);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null);
            }
        }
    }
}
