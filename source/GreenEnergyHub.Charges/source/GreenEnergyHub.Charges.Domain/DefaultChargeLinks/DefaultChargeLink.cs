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
using GreenEnergyHub.Charges.Domain.MeteringPoints;
using NodaTime;

namespace GreenEnergyHub.Charges.Domain.DefaultChargeLinks
{
    /// <summary>
    /// Default charge links represent charge links that must be created when requested externally.
    ///
    /// In reality this happens in the business process where a new metering point is connected.
    ///
    /// Charge links are created when the metering point type of the metering point matches that of the
    /// default charge link. The process has some additional period logic controlling the creation.
    /// </summary>
    public class DefaultChargeLink
    {
        /// <summary>
        /// The default start date the charge link is applicable from.
        /// </summary>
        private readonly Instant _startDateTime;

        /// <summary>
        /// The metering point type that must match to be applicable for linking.
        /// </summary>
        private readonly MeteringPointType _meteringPointType;

        public DefaultChargeLink(
            Instant startDateTime,
            Instant endDateTime,
            Guid chargeId,
            MeteringPointType meteringPointType)
        {
            _startDateTime = startDateTime;
            EndDateTime = endDateTime;
            _meteringPointType = meteringPointType;
            ChargeId = chargeId;
        }

        /// <summary>
        /// The starting date is determined by the latest date when comparing meteringPointCreatedDateTime and
        /// SettingStartDateTime. It is used to dictate when the charge link should start from.
        /// </summary>
        public Instant GetStartDateTime(Instant meteringPointCreatedDateTime)
        {
            return _startDateTime > meteringPointCreatedDateTime ? _startDateTime : meteringPointCreatedDateTime;
        }

        /// <summary>
        /// If the DefaultChargeLink has an EndDateTime,
        /// it is only applicable for linking when EndDateTime is later than the StartDateTime
        /// </summary>
        ///
        public bool ApplicableForLinking(Instant meteringPointCreatedDateTime, MeteringPointType meteringPointType)
        {
            return EndDateTime > GetStartDateTime(meteringPointCreatedDateTime)
                   && _meteringPointType == meteringPointType;
        }

        /// <summary>
        /// A reference to the charge in the Charge table
        /// </summary>
        public Guid ChargeId { get; }

        /// <summary>
        /// If the DefaultChargeLink is only applicable for linking when EndDateTime is later than the StartDateTime
        /// </summary>
        public Instant EndDateTime { get; }

        /// <summary>
        /// All default charge links are anticipated to have a factor of 1.
        /// </summary>
        public static int Factor => 1;
    }
}
