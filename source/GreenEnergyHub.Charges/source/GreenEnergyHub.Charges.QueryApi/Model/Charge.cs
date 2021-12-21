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
using System.Collections.Generic;

#nullable disable

namespace GreenEnergyHub.Charges.QueryApi.Model
{
    public partial class Charge
    {
        public Charge()
        {
            ChargeLinks = new HashSet<ChargeLink>();
            ChargePoints = new HashSet<ChargePoint>();
            DefaultChargeLinks = new HashSet<DefaultChargeLink>();
        }

        public Guid Id { get; set; }

        public string SenderProvidedChargeId { get; set; }

        public int Type { get; set; }

        public Guid OwnerId { get; set; }

        public bool TaxIndicator { get; set; }

        public int Resolution { get; set; }

        public bool TransparentInvoicing { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }

        public int VatClassification { get; set; }

        public DateTime StartDateTime { get; set; }

        public DateTime? EndDateTime { get; set; }

        public virtual MarketParticipant Owner { get; set; }

        public virtual ICollection<ChargeLink> ChargeLinks { get; set; }

        public virtual ICollection<ChargePoint> ChargePoints { get; set; }

        public virtual ICollection<DefaultChargeLink> DefaultChargeLinks { get; set; }
    }
}
