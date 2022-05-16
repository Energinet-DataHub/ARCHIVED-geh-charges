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
using GreenEnergyHub.Charges.Domain.ChargePrices;
using NodaTime;

namespace GreenEnergyHub.Charges.Tests.Builders.Command
{
    public class ChargePriceBuilder
    {
        private Instant _time;
        private decimal _price;

        public ChargePriceBuilder WithTimeAndPrice(Instant time, decimal price)
        {
            _time = time;
            _price = price;
            return this;
        }

        public ChargePrice Build()
        {
            return new ChargePrice(
                Guid.NewGuid(),
                Guid.NewGuid(),
                _time,
                _price);
        }
    }
}
