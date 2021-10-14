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
using GreenEnergyHub.Charges.Contracts;
using GreenEnergyHub.DataHub.Charges.Libraries.Enums;
using GreenEnergyHub.DataHub.Charges.Libraries.Models;

namespace GreenEnergyHub.DataHub.Charges.Libraries.Protobuf
{
    public static class CreateDefaultChargeLinksFailedOutboundMapper
    {
        internal static CreateDefaultChargeLinksFailed Convert(
            [NotNull] CreateDefaultChargeLinksFailedDto createDefaultChargeLinksFailedDto)
        {
            if (createDefaultChargeLinksFailedDto == null)
                throw new ArgumentNullException(nameof(createDefaultChargeLinksFailedDto));

            return new CreateDefaultChargeLinksFailed
            {
                MeteringPointId = createDefaultChargeLinksFailedDto.meteringPointId,
                ErrorCode = ConvertErrorCode(createDefaultChargeLinksFailedDto.errorCode),
            };
        }

        private static CreateDefaultChargeLinksFailed.Types.ErrorCode ConvertErrorCode(ErrorCode errorCode)
        {
            return errorCode switch
            {
                ErrorCode.ErrorNotDetermined => CreateDefaultChargeLinksFailed.Types.ErrorCode.EcErrorNotDetermined,
                ErrorCode.MeteringPointUnknown => CreateDefaultChargeLinksFailed.Types.ErrorCode.EcMeteringPointUnknown,
                _ => throw new ArgumentOutOfRangeException(nameof(errorCode), $"Value: {errorCode.ToString()}")
            };
        }
    }
}
