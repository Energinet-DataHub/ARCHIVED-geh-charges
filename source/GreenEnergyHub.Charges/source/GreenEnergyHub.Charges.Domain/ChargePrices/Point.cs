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

using NodaTime;

namespace GreenEnergyHub.Charges.Domain.ChargePrices
{
    /// <summary>
    /// This Point class is used for handling each individual price of charge price list.
    /// </summary>
    public class Point
    {
        public Point(int position, decimal price, Instant time)
        {
            Position = position;
            Price = price;
            Time = time;
        }

        /// <summary>
        /// The position of the price in the price list it was delivered.
        /// </summary>
        public int Position { get; }

        public decimal Price { get; }

        /// <summary>
        /// A point in time, where the price applies.
        /// </summary>
        public Instant Time { get; }
    }
}
