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
using GreenEnergyHub.Charges.Domain.Charges;

namespace GreenEnergyHub.Charges.Tests.Builders.Command
{
    public class ChargeBuilder
    {
        private List<Point> _points = new();
        private List<ChargePeriod> _periods = new();
        private string _senderProvidedChargeId = "senderProvidedChargeId";
        private Guid _id = Guid.NewGuid();
        private Guid _ownerId = Guid.NewGuid();
        private ChargeType _chargeType = ChargeType.Tariff;

        public ChargeBuilder WithPoints(IEnumerable<Point> points)
        {
            _points = points.ToList();
            return this;
        }

        public ChargeBuilder WithPeriods(IEnumerable<ChargePeriod> periods)
        {
            _periods = periods.ToList();
            return this;
        }

        public ChargeBuilder WithSenderProvidedChargeId(string senderProvidedChargeId)
        {
            _senderProvidedChargeId = senderProvidedChargeId;
            return this;
        }

        public ChargeBuilder WithId(Guid id)
        {
            _id = id;
            return this;
        }

        public ChargeBuilder WithOwnerId(Guid ownerId)
        {
            _ownerId = ownerId;
            return this;
        }

        public ChargeBuilder WithChargeType(ChargeType chargeType)
        {
            _chargeType = chargeType;
            return this;
        }

        public Charge Build()
        {
            return new Charge(
                _id,
                _senderProvidedChargeId,
                _ownerId,
                _chargeType,
                Resolution.PT1H,
                true,
                _points,
                _periods);
        }
    }
}
