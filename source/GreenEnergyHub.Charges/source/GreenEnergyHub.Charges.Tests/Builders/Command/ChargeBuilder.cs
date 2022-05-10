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
using System.Linq;
using GreenEnergyHub.Charges.Domain.ChargeInformation;
using GreenEnergyHub.Charges.Domain.Common;

namespace GreenEnergyHub.Charges.Tests.Builders.Command
{
    public class ChargeBuilder
    {
        private List<ChargePeriod> _periods = new();

        public ChargeBuilder WithPeriods(IEnumerable<ChargePeriod> periods)
        {
            _periods = periods.ToList();
            return this;
        }

        public Charge Build()
        {
            return new Charge(
                Guid.NewGuid(),
                "senderProvidedChargeId",
                Guid.NewGuid(),
                ChargeType.Tariff,
                Resolution.PT1H,
                true,
                _periods);
        }
    }
}
