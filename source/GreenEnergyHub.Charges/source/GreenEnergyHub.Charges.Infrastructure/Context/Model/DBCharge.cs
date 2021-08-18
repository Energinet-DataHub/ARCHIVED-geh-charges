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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable 8618
#pragma warning disable CA2227

namespace GreenEnergyHub.Charges.Infrastructure.Context.Model
{
    [Table("Charge")]
    public class DBCharge
    {
        public DBCharge()
        {
            ChargePrices = new List<DBChargePrice>();
            ChargePeriodDetails = new List<DBChargePeriodDetails>();
        }

        [Key]
        public int RowId { get; set; }

        public string ChargeId { get; set; }

        public int ChargeType { get; set; }

        public int MarketParticipantRowId { get; set; }

        public byte TaxIndicator { get; set; }

        public int Resolution { get; set; }

        public string Currency { get; set; }

        public byte TransparentInvoicing { get; set; }

        [NotMapped]
        public virtual DBChargeOperation ChargeOperation { get; set; }

        public virtual DBMarketParticipant MarketParticipant { get; set; }

        public virtual List<DBChargePrice> ChargePrices { get; set; }

        public virtual List<DBChargePeriodDetails> ChargePeriodDetails { get; set; }
    }
}
