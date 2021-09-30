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
using GreenEnergyHub.Charges.Domain.MeteringPointCreatedEvents;
using NodaTime.Text;

namespace GreenEnergyHub.Charges.Domain.MeteringPoints
{
    public static class MeteringPointFactory
    {
        public static MeteringPoint Create(
            ConsumptionMeteringPointCreatedEvent consumptionMeteringPointCreatedEvent)
        {
            if (consumptionMeteringPointCreatedEvent == null)
                throw new ArgumentNullException(nameof(consumptionMeteringPointCreatedEvent));

            var effectiveDate = InstantPattern.General.Parse(consumptionMeteringPointCreatedEvent.EffectiveDate).Value;
            var meteringPointType = Enum.Parse<MeteringPointType>(consumptionMeteringPointCreatedEvent.MeteringPointType);
            var connectionState = Enum.Parse<ConnectionState>(consumptionMeteringPointCreatedEvent.ConnectionState);
            var settlementMethod = consumptionMeteringPointCreatedEvent.SettlementMethod == null
                ? null as SettlementMethod?
                : Enum.Parse<SettlementMethod>(consumptionMeteringPointCreatedEvent.SettlementMethod);

            return MeteringPoint.Create(
                consumptionMeteringPointCreatedEvent.MeteringPointId,
                meteringPointType,
                consumptionMeteringPointCreatedEvent.GridAreaId,
                effectiveDate,
                connectionState,
                settlementMethod);
        }
    }
}
