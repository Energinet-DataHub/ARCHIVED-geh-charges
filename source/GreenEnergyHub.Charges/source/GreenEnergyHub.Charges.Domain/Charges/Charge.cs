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
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Charges
{
    public class Charge
    {
        private readonly List<Point> _points;
        private readonly List<ChargePeriod> _periods;

        public Charge(
            Guid id,
            string senderProvidedChargeId,
            Guid ownerId,
            ChargeType type,
            Resolution resolution,
            bool taxIndicator,
            List<Point> points,
            List<ChargePeriod> periods)
        {
            Id = id;
            SenderProvidedChargeId = senderProvidedChargeId;
            OwnerId = ownerId;
            Type = type;
            Resolution = resolution;
            _points = points;
            _periods = periods;
            TaxIndicator = taxIndicator;
        }

        /// <summary>
        /// Minimal ctor to support EF Core.
        /// </summary>
        // ReSharper disable once UnusedMember.Local - used by EF Core
        private Charge()
        {
            SenderProvidedChargeId = null!;
            _points = new List<Point>();
            _periods = new List<ChargePeriod>();
        }

        /// <summary>
        /// Globally unique identifier of the charge.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Unique ID of a charge (Note, unique per market participants and charge type).
        /// Example: EA-001
        /// </summary>
        public string SenderProvidedChargeId { get; }

        public ChargeType Type { get; }

        /// <summary>
        ///  Aggregate ID reference to the owning market participant.
        /// </summary>
        public Guid OwnerId { get; }

        public Resolution Resolution { get; }

        /// <summary>
        /// Indicates whether the Charge is tax or not.
        /// </summary>
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local - private setter used in unit test
        public bool TaxIndicator { get; private set;  }

        public IReadOnlyCollection<Point> Points => _points;

        public IReadOnlyCollection<ChargePeriod> Periods => _periods;

        /// <summary>
        /// Use this method to stop a charge upon receiving a stop charge request
        /// </summary>
        /// <param name="stopDate"></param>
        /// <exception cref="InvalidOperationException">Throws when no charge periods found on charge.</exception>
        public void StopCharge(Instant stopDate)
        {
            StopExistingPeriod(stopDate);
            RemoveAllSubsequentPeriods(stopDate);
            Validate();
        }

        /// <summary>
        /// Use this method to update the charge periods timeline of a charge upon receiving a charge update request
        /// Please see the persist charge documentation where the update flow is covered:
        /// https://github.com/Energinet-DataHub/geh-charges/tree/main/docs/process-flows#persist-charge
        /// </summary>
        /// <param name="newChargePeriod">New Charge Period from update charge request</param>
        /// <exception cref="ArgumentNullException">Throws when <paramref name="newChargePeriod"/> is empty</exception>
        public void UpdateCharge(ChargePeriod newChargePeriod)
        {
            if (newChargePeriod == null) throw new ArgumentNullException(nameof(newChargePeriod));

            if (_periods.Exists(p => p.StartDateTime < newChargePeriod.StartDateTime))
            {
                StopExistingPeriod(newChargePeriod.StartDateTime);
            }

            RemoveAllSubsequentPeriods(newChargePeriod.StartDateTime);
            _periods.Add(newChargePeriod);
            Validate();
        }

        private void StopExistingPeriod(Instant stopDate)
        {
            var previousPeriod = _periods
                .SingleOrDefault(p =>
                    p.EndDateTime >= stopDate &&
                    p.StartDateTime < stopDate);

            if (previousPeriod == null)
            {
                throw new InvalidOperationException("Cannot stop charge. No period exist on stop date.");
            }

            if (stopDate == previousPeriod.EndDateTime)
            {
                // Charge already stopped
                return;
            }

            if (stopDate > previousPeriod.EndDateTime)
            {
                throw new InvalidOperationException("Cannot stop charge. Charge already stopped on earlier date.");
            }

            var newPreviousPeriod = ChargePeriodFactory.CreateFromExistingPeriodWithNewEndDate(previousPeriod, stopDate);
            _periods.Remove(previousPeriod);
            _periods.Add(newPreviousPeriod);
        }

        private void RemoveAllSubsequentPeriods(Instant date)
        {
            _periods.RemoveAll(p => p.StartDateTime >= date);
        }

        private bool Validate()
        {
            var result = EnsureNoGapsInChargePeriodTimeline();
            return result;
        }

        private bool EnsureNoGapsInChargePeriodTimeline()
        {
            var orderedPeriods = _periods.OrderBy(cp => cp.StartDateTime).ToList();
            var pointInTimeline = orderedPeriods[0].StartDateTime;
            foreach (var p in orderedPeriods)
            {
                if (p.StartDateTime == pointInTimeline)
                {
                    pointInTimeline = p.EndDateTime;
                }
                else
                {
                    throw new InvalidOperationException("Charge validation failed due to a gap in charge period timeline");
                }
            }

            return true;
        }
    }
}
