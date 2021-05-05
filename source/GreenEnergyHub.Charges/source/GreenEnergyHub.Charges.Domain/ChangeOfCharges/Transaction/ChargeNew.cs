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
using NodaTime;
#pragma warning disable 8618

namespace GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction
{
    public class ChargeNew
    {
        public ChargeNew()
        {
            Points = new List<Point>();
        }

        public string Id { get; set; }

        public ChargeType Type { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public Vat Vat { get; set; }

        public bool TransparentInvoicing { get; set; }

        public bool Tax { get; set; }

        public string Owner { get; set; }

        public Resolution Resolution { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227", Justification = "JSON deserialization")]
        public List<Point> Points { get; set; }
    }
}
