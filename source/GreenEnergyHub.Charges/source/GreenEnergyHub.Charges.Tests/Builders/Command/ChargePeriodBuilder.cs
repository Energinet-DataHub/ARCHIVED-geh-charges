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

using System;
using GreenEnergyHub.Charges.Domain.Charges;
using NodaTime;

namespace GreenEnergyHub.Charges.Tests.Builders.Command
{
    public class ChargePeriodBuilder
    {
        private const string Description = "description";
        private const bool TransparentInvoicing = false;
        private string _name = "name";
        private Instant _startDateTime = Instant.MinValue;
        private Instant _receivedDateTime = SystemClock.Instance.GetCurrentInstant();
        private int _receivedOrder;
        private bool _isStop;

        public ChargePeriodBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public ChargePeriodBuilder WithStartDateTime(Instant startDateTime)
        {
            _startDateTime = startDateTime;
            return this;
        }

        public ChargePeriodBuilder WithReceivedDateTime(Instant receivedDateTime)
        {
            _receivedDateTime = receivedDateTime;
            return this;
        }

        public ChargePeriodBuilder WithReceivedOrder(int receivedOrder)
        {
            _receivedOrder = receivedOrder;
            return this;
        }

        public ChargePeriodBuilder WithIsStop(bool isStop)
        {
            _isStop = isStop;
            return this;
        }

        public ChargePeriod Build()
        {
            return new ChargePeriod(
                Guid.NewGuid(),
                _name,
                Description,
                VatClassification.Vat25,
                TransparentInvoicing,
                _startDateTime,
                _receivedDateTime,
                _receivedOrder,
                _isStop);
        }
    }
}
