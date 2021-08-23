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

using NodaTime;

namespace GreenEnergyHub.Charges.Domain.MeteringPoints
{
    public class MeteringPoint
    {
        public MeteringPoint(
            int id,
            string meteringPointId,
            MeteringPointType meteringPointType,
            string meteringGridArea,
            Instant effectiveDate,
            int connectionState,
            int settlementMethod)
        {
            Id = id;
            MeteringPointId = meteringPointId;
            MeteringPointType = meteringPointType;
            MeteringGridArea = meteringGridArea;
            EffectiveDate = effectiveDate;
            ConnectionState = connectionState;
            SettlementMethod = settlementMethod;
        }

        public int Id { get; set; }

        public string MeteringPointId { get; set; }

        public MeteringPointType MeteringPointType { get; set; }

        public string MeteringGridArea { get; set; }

        public Instant EffectiveDate { get; set; }

        public int ConnectionState { get; set; }

        public int SettlementMethod { get; set; }
    }
}
