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
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using NodaTime;
using DefaultChargeLink = GreenEnergyHub.Charges.Infrastructure.Context.Model.DefaultChargeLink;

namespace GreenEnergyHub.Charges.Infrastructure.Context.Mapping
{
    public static class DefaultChargeLinkMapper
    {
        public static Domain.DefaultChargeLinks.DefaultChargeLink Map([NotNull]DefaultChargeLink defaultChargeLink)
        {
            return new Domain.DefaultChargeLinks.DefaultChargeLink(
                Instant.FromDateTimeUtc(defaultChargeLink.StartDateTime.ToUniversalTime()),
                Instant.FromDateTimeUtc(defaultChargeLink.EndDateTime.ToUniversalTime()),
                defaultChargeLink.ChargeRowId,
                (MeteringPointType)defaultChargeLink.MeteringPointType);
        }
    }
}
