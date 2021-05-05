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

using NodaTime;
#pragma warning disable 8618

namespace GreenEnergyHub.Charges.Domain.ChangeOfCharges.Transaction
{
    // Non-nullable member is uninitialized is ignored
    // Only properties which is allowed to be null is nullable
    // ChargeEvent integrity is null checked by ChargeCommandNullChecker

    /// <summary>
    /// The ChargeEvent class contains the intend of the charge command, e.g. it's an update of a charge plus an ID provided by the sender.
    /// </summary>
    public class ChargeEvent
    {
        /// <summary>
        /// Contains a unique ID for the specific Charge Event, provided by the sender.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Valid from, of a charge price list. Also known as Effective Date.
        /// </summary>
        public Instant StartDateTime { get; set; }

        /// <summary>
        /// Valid to, of a charge price list.
        /// </summary>
        public Instant? EndDateTime { get; set; }

        public ChargeEventFunction Status { get; set; }

        public string CorrelationId { get; set; }

        /// <summary>
        /// PTA: Is this relevant for an incoming charge command?
        /// </summary>
        public string LastUpdatedBy { get; set; }

        /// <summary>
        ///  Point in time set by the Charges domain
        /// </summary>
        public Instant RequestDate { get; set; } = SystemClock.Instance.GetCurrentInstant();
    }
}
