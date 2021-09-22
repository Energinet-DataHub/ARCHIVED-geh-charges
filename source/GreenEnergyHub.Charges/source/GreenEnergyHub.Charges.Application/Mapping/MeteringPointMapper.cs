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
using GreenEnergyHub.Charges.Domain.Charges.Events.Integration;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using NodaTime.Text;

namespace GreenEnergyHub.Charges.Application.Mapping
{
    public static class MeteringPointMapper
    {
        public static MeteringPoint MapMeteringPointCreatedEventToMeteringPoint(
            MeteringPointCreatedEvent meteringPointCreatedEvent)
        {
            if (meteringPointCreatedEvent == null)
                throw new ArgumentNullException(nameof(meteringPointCreatedEvent));

            var effectiveDate = InstantPattern.General.Parse(meteringPointCreatedEvent.EffectiveDate).Value;
            var meteringPointType = Enum.Parse<MeteringPointType>(meteringPointCreatedEvent.MeteringPointType);
            var connectionState = Enum.Parse<ConnectionState>(meteringPointCreatedEvent.ConnectionState);
            var settlementMethod = meteringPointCreatedEvent.SettlementMethod == null
                ? null as SettlementMethod?
                : Enum.Parse<SettlementMethod>(meteringPointCreatedEvent.SettlementMethod);

            return MeteringPoint.Create(
                meteringPointCreatedEvent.MeteringPointId,
                meteringPointType,
                meteringPointCreatedEvent.GridAreaId,
                effectiveDate,
                connectionState,
                settlementMethod);
        }
    }
}
