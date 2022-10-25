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

using GreenEnergyHub.Charges.Domain.Charges;
using GreenEnergyHub.Charges.TestCore.TestHelpers;
using NodaTime;

namespace GreenEnergyHub.Charges.TestCore.Builders.Command
{
    public class PointBuilder
    {
        private Instant _time = InstantHelper.GetStartDefault();
        private decimal _price = 1.00m;

        public PointBuilder WithTime(Instant time)
        {
            _time = time;
            return this;
        }

        public PointBuilder WithPrice(decimal price)
        {
            _price = price;
            return this;
        }

        public Point Build()
        {
            return new Point(
                _price,
                _time);
        }
    }
}
