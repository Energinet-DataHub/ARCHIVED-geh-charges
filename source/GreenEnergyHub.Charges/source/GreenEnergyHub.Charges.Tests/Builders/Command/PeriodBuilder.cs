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
using GreenEnergyHub.Charges.Domain.Charges;
using NodaTime;
using Period = GreenEnergyHub.Charges.Domain.Charges.Period;

namespace GreenEnergyHub.Charges.Tests.Builders.Command
{
    public class PeriodBuilder
    {
        private const string Description = "description";
        private const VatClassification VatClassification = Charges.Domain.Charges.VatClassification.Unknown;
        private const bool TransparentInvoicing = false;
        private string _name = "name";
        private Instant _startDateTime = Instant.MinValue;
        private Instant _endDateTime = Instant.FromUtc(9999, 12, 31, 23, 59, 59);

        public PeriodBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public PeriodBuilder WithStartDateTime(Instant startDateTime)
        {
            _startDateTime = startDateTime;
            return this;
        }

        public PeriodBuilder WithEndDateTime(Instant endDateTime)
        {
            _endDateTime = endDateTime;
            return this;
        }

        public Period Build()
        {
            return new Period(
                Guid.NewGuid(),
                _name,
                Description,
                VatClassification,
                TransparentInvoicing,
                _startDateTime,
                _endDateTime);
        }
    }
}
