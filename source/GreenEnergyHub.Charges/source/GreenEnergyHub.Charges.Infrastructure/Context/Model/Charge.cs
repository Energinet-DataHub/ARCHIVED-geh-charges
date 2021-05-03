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
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenEnergyHub.Charges.Infrastructure.Context.Model
{
    public class Charge
    {
#pragma warning disable 8618
        public Charge()
        {
            ChargePrices = new List<ChargePrice>();
        }
#pragma warning restore 8618

        public int Id { get; set; }

        public string MRid { get; set; }

        public virtual ChargeType ChargeType { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        public byte Status { get; set; }

        public long StartDate { get; set; }

        public long? EndDate { get; set; }

        public string? Currency { get; set; }

        [ForeignKey("ChargeTypeOwnerID")]
        public virtual MarketParticipant ChargeTypeOwner { get; set; }

        public bool TransparentInvoicing { get; set; }

        public bool TaxIndicator { get; set; }

        public virtual ResolutionType ResolutionType { get; set; }

        public virtual VatPayerType VatPayer { get; set; }

        public virtual List<ChargePrice> ChargePrices { get; }

        public string LastUpdatedByCorrelationId { get; set; }

        public string LastUpdatedByTransactionId { get; set; }

        public string LastUpdatedBy { get; set; }

        public long RequestDateTime { get; set; }
    }
}
