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
using GreenEnergyHub.Charges.Domain.Charges;

namespace GreenEnergyHub.Charges.WebApi.Models
{
    public class ChargeLinkDto
    {
        public ChargeType ChargeType { get; set; }

        public string ChargeId { get; set; }

        public string ChargeName { get; set; }

        public string ChargeOwnerId { get; set; }

        public string ChargeOwnerName { get; set; }

        public bool TaxIndicator { get; set; }

        public bool TransparentInvoicing { get; set; }

        public int Factor { get; set; }

        public DateTime StartDateTime { get; set; }

        public DateTime? EndDateTime { get; set; }
    }
}
