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
using Energinet.Charges.Contracts;
using Energinet.DataHub.Charges.Libraries.Enums;
using Energinet.DataHub.Charges.Libraries.Models;

namespace Energinet.DataHub.Charges.Libraries.Protobuf
{
    // Todo: Not in use?
    public static class CreateDefaultChargeLinksFailedOutboundMapper
    {
        internal static CreateDefaultChargeLinksReply Convert(
            [NotNull] CreateDefaultChargeLinksFailedDto createDefaultChargeLinksFailedDto)
        {
            return new()
            {
                MeteringPointId = createDefaultChargeLinksFailedDto.MeteringPointId,
                CreateDefaultChargeLinksFailed = new CreateDefaultChargeLinksFailed()
                {
                    ErrorCode = ConvertErrorCode(createDefaultChargeLinksFailedDto.ErrorCode),
                },
            };
        }

        private static CreateDefaultChargeLinksFailed.Types.ErrorCode ConvertErrorCode([NotNull] ErrorCode errorCode)
        {
            return errorCode switch
            {
                ErrorCode.Unspecified => CreateDefaultChargeLinksFailed.Types.ErrorCode.EcUnspecified,
                ErrorCode.MeteringPointUnknown => CreateDefaultChargeLinksFailed.Types.ErrorCode.EcMeteringPointUnknown,
                _ => throw new ArgumentOutOfRangeException(nameof(errorCode), $"Value: {errorCode.ToString()}"),
            };
        }
    }
}
