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
using GreenEnergyHub.Charges.Core.Enumeration;
using GreenEnergyHub.Charges.Domain.Events.Integration;
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using NodaTime;
using NodaTime.Text;

namespace GreenEnergyHub.Charges.Infrastructure.Context.Mapping
{
    public static class MeteringPointMapper
    {
        public static MeteringPoint MapMeteringPointToDomainModel(
            Model.MeteringPoint meteringPoint)
        {
            if (meteringPoint == null) throw new ArgumentNullException(nameof(meteringPoint));

            return new MeteringPoint(
                meteringPoint.MeteringPointId,
                meteringPoint.MeteringPointType,
                meteringPoint.MeteringGridArea,
                Instant.FromDateTimeUtc(meteringPoint.EffectiveDate),
                meteringPoint.ConnectionState,
                meteringPoint.SettlementMethod);
        }

        public static Model.MeteringPoint MapMeteringPointToEntity(
            MeteringPoint meteringPoint)
        {
            if (meteringPoint == null) throw new ArgumentNullException(nameof(meteringPoint));

            return new Model.MeteringPoint(
                meteringPoint.MeteringPointId,
                meteringPoint.MeteringPointType,
                meteringPoint.MeteringGridArea,
                meteringPoint.EffectiveDate.ToDateTimeUtc(),
                meteringPoint.ConnectionState,
                meteringPoint.SettlementMethod);
        }
    }
}
