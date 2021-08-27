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
using System.ComponentModel.DataAnnotations;
using GreenEnergyHub.Charges.Domain.MeteringPoints;

namespace GreenEnergyHub.Charges.Infrastructure.Context.Model
{
    public class MeteringPoint
    {
        public MeteringPoint(
            string meteringPointId,
            MeteringPointType meteringPointType,
            string gridAreaId,
            DateTime effectiveDate,
            ConnectionState connectionState,
            SettlementMethod? settlementMethod)
        {
            MeteringPointId = meteringPointId;
            MeteringPointType = meteringPointType;
            GridAreaId = gridAreaId;
            EffectiveDate = effectiveDate;
            ConnectionState = connectionState;
            SettlementMethod = settlementMethod;
        }

        [Key]
        public int RowId { get; set; }

        public string MeteringPointId { get; set; }

        public MeteringPointType MeteringPointType { get; set; }

        public string GridAreaId { get; set; }

        public DateTime EffectiveDate { get; set; }

        public ConnectionState ConnectionState { get; set; }

        public SettlementMethod? SettlementMethod { get; set; }
    }
}
