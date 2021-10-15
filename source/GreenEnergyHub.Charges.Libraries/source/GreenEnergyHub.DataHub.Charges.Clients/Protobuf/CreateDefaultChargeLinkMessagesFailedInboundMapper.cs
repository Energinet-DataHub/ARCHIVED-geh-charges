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
    internal class CreateDefaultChargeLinkMessagesFailedInboundMapper
    {
        protected internal static CreateDefaultChargeLinkMessagesFailedDto Convert(
            [NotNull] CreateDefaultChargeLinkMessagesFailed createDefaultChargeLinkMessagesFailed)
        {
            if (createDefaultChargeLinkMessagesFailed == null)
                throw new ArgumentNullException(nameof(createDefaultChargeLinkMessagesFailed));

            return new CreateDefaultChargeLinkMessagesFailedDto(
                createDefaultChargeLinkMessagesFailed.MeteringPointId,
                ConvertErrorCode(createDefaultChargeLinkMessagesFailed.ErrorCode));
        }

        private static ErrorCode ConvertErrorCode(CreateDefaultChargeLinkMessagesFailed.Types.ErrorCode errorCode)
        {
            return errorCode switch
            {
                CreateDefaultChargeLinkMessagesFailed.Types.ErrorCode.EcUnspecified => ErrorCode.Unspecified,
                CreateDefaultChargeLinkMessagesFailed.Types.ErrorCode.EcMeteringPointUnknown => ErrorCode.MeteringPointUnknown,
                _ => throw new ArgumentOutOfRangeException(nameof(errorCode), $"Value: {errorCode.ToString()}")
            };
        }
    }
}
