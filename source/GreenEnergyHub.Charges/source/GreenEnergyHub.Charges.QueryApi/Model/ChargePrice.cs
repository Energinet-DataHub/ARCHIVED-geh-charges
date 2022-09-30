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

#nullable disable

using System;

namespace GreenEnergyHub.Charges.QueryApi.Model
{
    public partial class ChargePrice
    {
        public Guid Id { get; set; }

        public Guid ChargeId { get; set; }

        public DateTime Time { get; set; }

        public decimal Price { get; set; }

        public bool Retired { get; set; }

        public DateTime? RetiredDateTime { get; set; }

        public Guid ChargeOperationId { get; set; }

        public virtual Charge Charge { get; set; }
    }
}
