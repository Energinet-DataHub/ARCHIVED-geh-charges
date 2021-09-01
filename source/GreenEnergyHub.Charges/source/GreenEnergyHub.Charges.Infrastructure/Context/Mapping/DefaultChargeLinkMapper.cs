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

using System.Diagnostics.CodeAnalysis;
using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.Infrastructure.Context.Model;
using NodaTime;

namespace GreenEnergyHub.Charges.Infrastructure.Context.Mapping
{
    public static class DefaultChargeLinkMapper
    {
        public static DefaultChargeLink Map(
            Instant meteringPointCreatedDateTime,
            [NotNull]DefaultChargeLinkSetting defaultChargeLinkSettings)
        {
            var endDateTime = defaultChargeLinkSettings.EndDateTime != null ?
                Instant.FromDateTimeUtc(defaultChargeLinkSettings.EndDateTime.Value.ToUniversalTime()) :
                (Instant?)null;

            return new DefaultChargeLink(
                meteringPointCreatedDateTime,
                Instant.FromDateTimeUtc(defaultChargeLinkSettings.StartDateTime.ToUniversalTime()),
                endDateTime,
                defaultChargeLinkSettings.ChargeRowId);
        }
    }
}
