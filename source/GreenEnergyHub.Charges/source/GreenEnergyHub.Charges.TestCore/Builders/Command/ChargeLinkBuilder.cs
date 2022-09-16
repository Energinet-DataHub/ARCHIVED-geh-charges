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
using GreenEnergyHub.Charges.Domain.ChargeLinks;
using NodaTime;

namespace GreenEnergyHub.Charges.TestCore.Builders.Command
{
    public class ChargeLinkBuilder
    {
        private readonly int _factor = 1;
        private Guid _chargeId = Guid.NewGuid();
        private Guid _meteringPointId = Guid.NewGuid();
        private Instant _startDate = SystemClock.Instance.GetCurrentInstant();
        private Instant _endDate = Instant.MaxValue;

        public ChargeLinkBuilder WithStartDate(Instant startDate)
        {
            _startDate = startDate;
            return this;
        }

        public ChargeLinkBuilder WithEndDate(Instant endDate)
        {
            _endDate = endDate;
            return this;
        }

        public ChargeLink Build()
        {
            return new ChargeLink(_chargeId, _meteringPointId, _startDate, _endDate, _factor);
        }
    }
}
