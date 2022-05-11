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
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.ChargePrices
{
    public class ChargePrice
    {
        public ChargePrice(
            Guid id,
            Guid chargeId,
            Instant time,
            decimal price)
        {
            Id = id;
            ChargeId = chargeId;
            Time = time;
            Price = price;
        }

        /// <summary>
        /// Minimal ctor to support EF Core.
        /// </summary>
        // ReSharper disable once UnusedMember.Local - used by EF Core
        private ChargePrice()
        {
            ChargeId = Guid.Empty;
        }

        public Guid Id { get; }

        public Guid ChargeId { get; }

        public Instant Time { get; }

        public decimal Price { get; }
    }
}
