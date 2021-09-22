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
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.MeteringPoints
{
    public class MeteringPoint
    {
        /// <summary>
        /// Used implicitly by persistence.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="meteringPointId"></param>
        /// <param name="meteringPointType"></param>
        /// <param name="gridAreaId"></param>
        /// <param name="effectiveDate"></param>
        /// <param name="connectionState"></param>
        /// <param name="settlementMethod"></param>
        private MeteringPoint(
            Guid id,
            string meteringPointId,
            MeteringPointType meteringPointType,
            string gridAreaId,
            Instant effectiveDate,
            ConnectionState connectionState,
            SettlementMethod? settlementMethod)
        {
            Id = id;
            MeteringPointId = meteringPointId;
            MeteringPointType = meteringPointType;
            GridAreaId = gridAreaId;
            EffectiveDate = effectiveDate;
            ConnectionState = connectionState;
            SettlementMethod = settlementMethod;
        }

        public static MeteringPoint Create(
            string meteringPointId,
            MeteringPointType meteringPointType,
            string gridAreaId,
            Instant effectiveDate,
            ConnectionState connectionState,
            SettlementMethod? settlementMethod)
        {
            var id = Guid.NewGuid();
            return new MeteringPoint(
                id,
                meteringPointId,
                meteringPointType,
                gridAreaId,
                effectiveDate,
                connectionState,
                settlementMethod);
        }

        public Guid Id { get; set; }

        public string MeteringPointId { get; }

        public MeteringPointType MeteringPointType { get; }

        public string GridAreaId { get; }

        public Instant EffectiveDate { get; }

        public ConnectionState ConnectionState { get; }

        public SettlementMethod? SettlementMethod { get; }
    }
}
