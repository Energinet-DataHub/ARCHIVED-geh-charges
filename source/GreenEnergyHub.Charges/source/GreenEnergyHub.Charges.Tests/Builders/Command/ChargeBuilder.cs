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
using NodaTime;

namespace GreenEnergyHub.Charges.Tests.Builders.Command
{
    public class ChargeBuilder
    {
        private const string Description = "description";
        private const bool TransparentInvoicing = false;
        private string _name = "name";
        private Instant _startDateTime = Instant.MinValue;
        private Instant _receivedDateTime = SystemClock.Instance.GetCurrentInstant();
        private int _receivedOrder = 0;
        private bool _isStop;
        private List<Point> _points = new();

        /*private List<ChargePeriod> _periods = new();*/

        public ChargeBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public ChargeBuilder WithStartDateTime(Instant startDateTime)
        {
            _startDateTime = startDateTime;
            return this;
        }

        public ChargeBuilder WithIsStop(bool isStop)
        {
            _isStop = isStop;
            return this;
        }

        public ChargeBuilder WithPoints(IEnumerable<Point> points)
        {
            _points = points.ToList();
            return this;
        }

        /*public ChargeBuilder WithPeriods(IEnumerable<ChargePeriod> periods)
        {
            _periods = periods.ToList();
            return this;
        }*/

        public Charge Build()
        {
            return new Charge(
                Guid.NewGuid(),
                "senderProvidedChargeId",
                Guid.NewGuid(),
                ChargeType.Tariff,
                Resolution.PT1H,
                true,
                _name,
                Description,
                VatClassification.Vat25,
                TransparentInvoicing,
                _startDateTime,
                _receivedDateTime,
                _receivedOrder,
                _isStop,
                _points);
        }
    }
}
