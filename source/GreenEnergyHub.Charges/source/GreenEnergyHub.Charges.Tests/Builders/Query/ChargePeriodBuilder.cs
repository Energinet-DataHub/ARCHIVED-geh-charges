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
using GreenEnergyHub.Charges.Core.DateTime;
using GreenEnergyHub.Charges.QueryApi.Model;

namespace GreenEnergyHub.Charges.Tests.Builders.Query
{
    public class ChargePeriodBuilder
    {
        private DateTime _startDateTime = DateTime.Now.Date.ToUniversalTime();
        private DateTime _endDateTime = InstantExtensions.GetEndDefault().ToDateTimeUtc();
        private string _name = "Charge name";
        private bool _transparentInvoicing;

        public ChargePeriodBuilder WithStartDateTime(DateTime startDateTime)
        {
            _startDateTime = startDateTime;
            return this;
        }

        public ChargePeriodBuilder WithEndDateTime(DateTime endDateTime)
        {
            _endDateTime = endDateTime;
            return this;
        }

        public ChargePeriodBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public ChargePeriodBuilder WithTransparentInvoicing(bool transparentInvoicing)
        {
            _transparentInvoicing = transparentInvoicing;
            return this;
        }

        public ChargePeriod Build(Charge charge)
        {
            return new ChargePeriod
            {
                Id = Guid.NewGuid(),
                Charge = charge,
                ChargeId = charge.Id,
                TransparentInvoicing = _transparentInvoicing,
                Description = "Charge description",
                Name = _name,
                VatClassification = 0,
                StartDateTime = _startDateTime,
                EndDateTime = _endDateTime,
            };
        }
    }
}
