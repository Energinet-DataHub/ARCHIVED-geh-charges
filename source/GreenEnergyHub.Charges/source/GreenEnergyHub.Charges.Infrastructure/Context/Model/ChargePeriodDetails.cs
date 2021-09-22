﻿// Copyright 2020 Energinet DataHub A/S
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
using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable 8618

namespace GreenEnergyHub.Charges.Infrastructure.Context.Model
{
    public class ChargePeriodDetails
    {
        public ChargePeriodDetails()
        {
            Id = Guid.NewGuid();
        }

        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Charge")]
        public Guid ChargeId { get; set; }

        [ForeignKey("ChargeOperation")]
        public Guid ChargeOperationId { get; set; }

        public virtual ChargeOperation ChargeOperation { get; set; }

        public virtual Charge Charge { get; set; }

        public DateTime StartDateTime { get; set; }

        public DateTime? EndDateTime { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int VatClassification { get; set; }

        public bool Retired { get; set; }

        public DateTime? RetiredDateTime { get; set; }
    }
}
