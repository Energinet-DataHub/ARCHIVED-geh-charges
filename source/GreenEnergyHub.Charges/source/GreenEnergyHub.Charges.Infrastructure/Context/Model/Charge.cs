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

#pragma warning disable 8618
#pragma warning disable CA2227

namespace GreenEnergyHub.Charges.Infrastructure.Context.Model
{
    public class Charge
    {
        public Charge()
        {
            ChargePrices = new List<ChargePrice>();
            ChargePeriodDetails = new List<ChargePeriodDetails>();
        }

        [Key]
        public int RowId { get; set; }

        public string ChargeId { get; set; }

        public int ChargeType { get; set; }

        public string Owner { get; set; }

        public byte TaxIndicator { get; set; }

        public int ResolutionType { get; set; }

        public string Currency { get; set; }

        public byte TransparentInvoicing { get; set; }

        public virtual MarketParticipant MarketParticipant { get; set; }

        public virtual List<ChargePrice> ChargePrices { get; set; }

        public virtual List<ChargePeriodDetails> ChargePeriodDetails { get; set; }
    }
}
