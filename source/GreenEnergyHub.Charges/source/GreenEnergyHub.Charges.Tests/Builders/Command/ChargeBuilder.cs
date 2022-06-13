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
using GreenEnergyHub.Charges.TestCore;
using NodaTime;

namespace GreenEnergyHub.Charges.Tests.Builders.Command
{
    public class ChargeBuilder
    {
        private Guid _ownerId = Guid.NewGuid();
        private string _name = "ChargeBuilderDefaultName";
        private Instant _stopDate = InstantHelper.GetEndDefault();
        private List<Point> _points = new();
        private List<ChargePeriod> _periods = new();
        private string _senderProvidedChargeId = "SenderProvidedChargeId";
        private Instant _startDate = InstantHelper.GetStartDefault();
        private Resolution _resolution = Resolution.PT1H;
        private TaxIndicator _taxIndicator = TaxIndicator.Tax;

        public ChargeBuilder WithTaxIndicator(TaxIndicator taxIndicator)
        {
            _taxIndicator = taxIndicator;
            return this;
        }

        public ChargeBuilder WithResolution(Resolution resolution)
        {
            _resolution = resolution;
            return this;
        }

        public ChargeBuilder WithPoints(IEnumerable<Point> points)
        {
            _points = points.ToList();
            return this;
        }

        public ChargeBuilder AddPeriod(ChargePeriod period)
        {
            _periods.Add(period);
            return this;
        }

        public ChargeBuilder AddPeriods(IEnumerable<ChargePeriod> periods)
        {
            _periods.AddRange(periods);
            return this;
        }

        public ChargeBuilder WithStopDate(Instant stopDate)
        {
            _stopDate = stopDate;
            return this;
        }

        public ChargeBuilder WithSenderProvidedChargeId(string senderProvidedChargeId)
        {
            _senderProvidedChargeId = senderProvidedChargeId;
            return this;
        }

        public ChargeBuilder WithOwnerId(Guid ownerId)
        {
            _ownerId = ownerId;
            return this;
        }

        public ChargeBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public ChargeBuilder WithStartDate(Instant startDate)
        {
            _startDate = startDate;
            return this;
        }

        public Charge Build()
        {
            var operationId = Guid.NewGuid().ToString();

            var charge = Charge.Create(
                _name,
                "chargeDescription",
                _senderProvidedChargeId,
                _ownerId,
                ChargeType.Tariff,
                _resolution,
                _taxIndicator,
                VatClassification.Unknown,
                true,
                _startDate);

            if (_periods.Any())
            {
                foreach (var period in _periods.OrderBy(p => p.StartDateTime))
                {
                    if (period.EndDateTime != InstantHelper.GetEndDefault())
                    {
                        throw new InvalidOperationException(
                            $"Could not create charge periods in {nameof(ChargeBuilder)}. Period must have infinite end date.");
                    }

                    charge.Update(period, _taxIndicator, _resolution, operationId);
                }
            }

            if (_stopDate != InstantHelper.GetEndDefault())
            {
                charge.Stop(_stopDate);
            }

            if (_points.Any())
            {
                charge.UpdatePrices(
                    _points.Min(p => p.Time),
                    _points.Max(p => p.Time),
                    _points,
                    operationId);
            }

            return charge;
        }
    }
}
