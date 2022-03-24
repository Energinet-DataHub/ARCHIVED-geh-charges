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
using GreenEnergyHub.Charges.Core.DateTime;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.Charges
{
    public class Charge
    {
        private readonly List<Point> _points;

        public Charge(
            Guid id,
            string senderProvidedChargeId,
            Guid ownerId,
            ChargeType type,
            Resolution resolution,
            bool taxIndicator,
            string name,
            string description,
            VatClassification vatClassification,
            bool transparentInvoicing,
            Instant startDateTime,
            Instant receivedDateTime,
            int receivedOrder,
            bool isStop,
            List<Point> points)
        {
            Id = id;
            SenderProvidedChargeId = senderProvidedChargeId;
            OwnerId = ownerId;
            Type = type;
            Resolution = resolution;
            TaxIndicator = taxIndicator;
            Name = name;
            Description = description;
            VatClassification = vatClassification;
            TransparentInvoicing = transparentInvoicing;
            StartDateTime = startDateTime;
            ReceivedDateTime = receivedDateTime;
            ReceivedOrder = receivedOrder;
            IsStop = isStop;
            _points = points;
        }

        /// <summary>
        /// Minimal ctor to support EF Core.
        /// </summary>
        // ReSharper disable once UnusedMember.Local - used by EF Core
        private Charge()
        {
            SenderProvidedChargeId = null!;
            Name = null!;
            Description = null!;
            _points = new List<Point>();
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

        /// <summary>
        /// The charge name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The charge description
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// VAT classification for charge
        /// </summary>
        public VatClassification VatClassification { get; }

        /// <summary>
        /// In Denmark the Energy Supplier invoices the customer, including the charges from the Grid Access Provider and the System Operator.
        /// This boolean can be use to indicate that a charge must be visible on the invoice sent to the customer.
        /// </summary>
        public bool TransparentInvoicing { get; }

        /// <summary>
        /// Valid from, of a charge period. Also known as Effective Date.
        /// </summary>
        public Instant StartDateTime { get; }

        /// <summary>
        /// Received date and time, used for ordering periods together with ReceivedOrder
        /// </summary>
        public Instant ReceivedDateTime { get; }

        /// <summary>
        /// Order of period when received in bundle, used for ordering periods together with ReceivedDateTime
        /// </summary>
        public int ReceivedOrder { get; }

        /// <summary>
        /// Indicates a charge stop.
        /// </summary>
        public bool IsStop { get; }

        public IReadOnlyCollection<Point> Points => _points;

        /// <summary>
        /// Use this method to update the charge periods timeline of a charge upon receiving a charge update request
        /// Please see the persist charge documentation where the update flow is covered:
        /// https://github.com/Energinet-DataHub/geh-charges/tree/main/docs/process-flows#persist-charge
        /// </summary>
        /// <param name="newChargePeriod">New Charge Period from update charge request</param>
        /// <exception cref="ArgumentNullException">Throws when <paramref name="newChargePeriod"/> is empty</exception>
        public void Update(Charge newChargePeriod)
        {
            if (newChargePeriod == null) throw new ArgumentNullException(nameof(newChargePeriod));

            /*if (_periods.Exists(p => p.StartDateTime < newChargePeriod.StartDateTime))
            {
                StopExistingPeriod(newChargePeriod.StartDateTime);
            }

            RemoveAllSubsequentPeriods(newChargePeriod.StartDateTime);*/

            //_periods.Add(newChargePeriod);
        }

        /// <summary>
        /// Use this method to stop a charge upon receiving a stop charge request
        /// </summary>
        /// <param name="stopCharge"></param>
        /// <exception cref="ArgumentNullException"><paramref name="stopCharge"/> is <c>null</c></exception>
        public void Stop(Charge stopCharge)
        {
            GuardForInvalidStop(stopCharge);

            // _periods.Add(stopCharge);
            _points.RemoveAll(p => p.Time >= stopCharge.StartDateTime);

            /*StopExistingPeriod(newChargePeriod.Value);
            RemoveAllSubsequentPeriods(newChargePeriod.Value);
            _points.RemoveAll(p => p.Time >= newChargePeriod);*/
        }

        public void CancelStop()
        {
            // new implementation:
            /*var stopPeriod = _periods.OrderedByReceivedDateTimeAndOrder().SingleOrDefault(p => p.IsStop);

            if (stopPeriod == null)
            {
                throw new InvalidOperationException("Cannot cancel charge stop. No stop period exists on charge.");
            }

            var newLatestPeriod = stopPeriod.AsNewPeriod();
            _periods.Remove(stopPeriod);
            _periods.Add(newLatestPeriod);*/

            // Original implementation:
            /*var oldLatestPeriod = _periods.OrderByDescending(p => p.StartDateTime).First();
            var newLatestPeriod = oldLatestPeriod.AsChargeStop(InstantExtensions.GetEndDefault());
            _periods.Remove(oldLatestPeriod);
            _periods.Add(newLatestPeriod);*/
        }

        private void GuardForInvalidStop(Charge stopCharge)
        {
            if (stopCharge == null) throw new ArgumentNullException(nameof(stopCharge));
            /*if (!_charge.Any()) throw new InvalidOperationException("Cannot stop charge. No periods exists.");

            var previousPeriod = _periods.OrderBy(p => p.StartDateTime)
                .FirstOrDefault(p => p.StartDateTime < stopCharge.StartDateTime);
            var dayBeforeAtMidnight = stopCharge.StartDateTime.Minus(Duration.FromDays(1));
            var validChargePeriodAtDayBefore = _periods.GetValidChargePeriodAsOf(dayBeforeAtMidnight);

            if (!_periods.Any() || validChargePeriodAtDayBefore?.IsStop == true || previousPeriod == null)
                throw new InvalidOperationException("Cannot stop charge. No period exist on stop date.");*/
        }

        /*private void StopExistingPeriod(Instant newChargePeriod)
        {
            var previousPeriod = _periods.SingleOrDefault(p => p.EndDateTime >= newChargePeriod && p.StartDateTime < newChargePeriod);

            if (previousPeriod == null)
            {
                throw new InvalidOperationException("Cannot stop charge. No period exist on stop date.");
            }

            // Return if charge already has end date at given stop date
            if (newChargePeriod == previousPeriod.EndDateTime) return;

            var newPreviousPeriod = previousPeriod.AsChargeStop(newChargePeriod);
            _periods.Remove(previousPeriod);
            _periods.Add(newPreviousPeriod);
        }

        private void RemoveAllSubsequentPeriods(Instant date)
        {
            _periods.RemoveAll(p => p.StartDateTime >= date);
        }*/
    }
}
