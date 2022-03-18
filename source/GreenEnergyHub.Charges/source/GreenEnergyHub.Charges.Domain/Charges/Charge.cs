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
using System.Collections.Generic;
using System.Linq;
using GreenEnergyHub.Charges.Core.DateTime;
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
        /// Use this method to update the charge periods timeline of a charge upon receiving a charge update request
        /// Please see the persist charge documentation where the update flow is covered:
        /// https://github.com/Energinet-DataHub/geh-charges/tree/main/docs/process-flows#persist-charge
        /// </summary>
        /// <param name="newChargePeriod">New Charge Period from update charge request</param>
        /// <exception cref="ArgumentNullException">Throws when <paramref name="newChargePeriod"/> is empty</exception>
        public void Update(ChargePeriod newChargePeriod)
        {
            if (newChargePeriod == null) throw new ArgumentNullException(nameof(newChargePeriod));

            if (newChargePeriod.EndDateTime != InstantExtensions.GetEndDefault())
            {
                throw new InvalidOperationException("Charge update must not have bound end date.");
            }

            var stopPeriod = _periods.FirstOrDefault(p => p.StartDateTime == p.EndDateTime
                                                          && p.StartDateTime > newChargePeriod.StartDateTime);
            if (stopPeriod != null)
            {
                newChargePeriod = newChargePeriod.WithEndDate(stopPeriod.StartDateTime);
            }

            if (_periods.Exists(p => p.StartDateTime < newChargePeriod.StartDateTime))
            {
                StopExistingPeriod(newChargePeriod.StartDateTime);
            }

            RemoveAllSubsequentPeriodsExceptStop(newChargePeriod.StartDateTime);
            _periods.Add(newChargePeriod);
        }

        /// <summary>
        /// Use this method to stop a charge upon receiving a stop charge request
        /// </summary>
        /// <param name="stopDate"></param>
        /// <exception cref="ArgumentNullException"><paramref name="stopDate"/> is <c>null</c></exception>
        public void Stop(Instant? stopDate)
        {
            if (stopDate == null)
            {
                throw new ArgumentNullException(nameof(stopDate));
            }

            StopExistingPeriod(stopDate.Value);
            RemoveAllSubsequentPeriodsExceptStop(stopDate.Value);
        }

        public void CancelStop()
        {
            var oldLatestPeriod = _periods.OrderByDescending(p => p.StartDateTime).First();
            var newLatestPeriod = oldLatestPeriod.WithEndDate(InstantExtensions.GetEndDefault());
            _periods.Remove(oldLatestPeriod);
            _periods.Add(newLatestPeriod);
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

            // Return if charge already has end date at given stop date
            if (stopDate == previousPeriod.EndDateTime) return;

            var newPreviousPeriod = previousPeriod.WithEndDate(stopDate);
            _periods.Remove(previousPeriod);
            _periods.Add(newPreviousPeriod);
        }

        private void RemoveAllSubsequentPeriodsExceptStop(Instant date)
        {
            _periods.RemoveAll(p => p.StartDateTime >= date && p.StartDateTime != p.EndDateTime);
        }
    }
}
