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
using GreenEnergyHub.DataHub.Charges.Libraries.Enums;
using GreenEnergyHub.DataHub.Charges.Libraries.Models;

namespace GreenEnergyHub.DataHub.Charges.Libraries.DefaultChargeLinkReply
{
    public delegate Task OnSuccess(CreateDefaultChargeLinksSucceededDto createDefaultChargeLinksSucceeded);

    public delegate Task OnFailure(CreateDefaultChargeLinksFailedDto createDefaultChargeLinksSucceeded);

    public sealed class DefaultChargeLinkReplyDeserializer : IDefaultChargeLinkReplyDeserializer
    {
        private readonly OnSuccess _handleSuccess;
        private readonly OnFailure _handleFailure;

        public DefaultChargeLinkReplyDeserializer(
            [NotNull] OnSuccess handleSuccess,
            [NotNull] OnFailure handleFailure)
        {
            _handleSuccess = handleSuccess;
            _handleFailure = handleFailure;
        }

        public async Task DeserializeMessageAsync(byte[] data, MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.RequestSucceeded:

                    const string fakeKnownMeteringPointIdFromProto = "knownMeteringPointId1234";
                    const bool fakeDidCreateChargeLinks = true;

                    var succeededDto = new CreateDefaultChargeLinksSucceededDto(
                        fakeKnownMeteringPointIdFromProto,
                        fakeDidCreateChargeLinks);

                    await _handleSuccess(succeededDto).ConfigureAwait(false);
                    break;

                case MessageType.RequestFailed:
                    const string fakeUnknownMeteringPointIdFromProto = "unknownMeteringPointId9876";
                    const ErrorCode errorCode = ErrorCode.MeteringPointUnknown;

                    var failedDto = new CreateDefaultChargeLinksFailedDto(
                        fakeUnknownMeteringPointIdFromProto,
                        errorCode);

                    await _handleFailure(failedDto).ConfigureAwait(false);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null);
            }
        }
    }
}
