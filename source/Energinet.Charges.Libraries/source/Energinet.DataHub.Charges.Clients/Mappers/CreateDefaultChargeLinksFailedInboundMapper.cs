﻿// Copyright 2020 Energinet DataHub A/S
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
using Energinet.DataHub.Charges.Libraries.Models;
using GreenEnergyHub.Charges.Contracts;
using CreateDefaultChargeLinksFailed =
    Energinet.Charges.Contracts.CreateDefaultChargeLinksReply.Types.CreateDefaultChargeLinksFailed;

namespace Energinet.DataHub.Charges.Libraries.Mappers
{
    internal class CreateDefaultChargeLinksFailedInboundMapper
    {
        protected internal static CreateDefaultChargeLinksFailedDto Convert(
            [NotNull] CreateDefaultChargeLinksReply createDefaultChargeLinksReply)
        {
            return new(
                createDefaultChargeLinksReply.MeteringPointId,
                ConvertErrorCode(createDefaultChargeLinksReply.CreateDefaultChargeLinksFailed.ErrorCode));
        }

        private static ErrorCode ConvertErrorCode([NotNull] CreateDefaultChargeLinksFailed.Types.ErrorCode errorCode)
        {
            return errorCode switch
            {
                CreateDefaultChargeLinksFailed.Types.ErrorCode.EcMeteringPointUnknown => ErrorCode.MeteringPointUnknown,
                _ => throw new ArgumentOutOfRangeException(nameof(errorCode), $"Value: {errorCode.ToString()}"),
            };
        }
    }
}
