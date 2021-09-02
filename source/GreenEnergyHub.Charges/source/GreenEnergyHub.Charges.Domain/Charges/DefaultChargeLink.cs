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

using GreenEnergyHub.Charges.Domain.MeteringPoints;
using NodaTime;

#pragma warning disable 8618

namespace GreenEnergyHub.Charges.Domain.Charges
{
    // Logically there is a MeteringPointType attached to the DefaultChargeLink, but atm. It is not used.
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
            Instant? endDateTime,
            int chargeRowId,
            MeteringPointType meteringPointType)
        {
            _startDateTime = startDateTime;
            EndDateTime = endDateTime;
            _meteringPointType = meteringPointType;
            ChargeRowId = chargeRowId;
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
        /// If the charge setting has a EndDateTime, it is only applicable for link if it is lesser or equal too StartDateTime.
        /// </summary>
        ///
        public bool ApplicableForLinking(Instant meteringPointCreatedDateTime, MeteringPointType meteringPointType)
        {
            return (EndDateTime == null || EndDateTime.Value > GetStartDateTime(meteringPointCreatedDateTime))
                   && _meteringPointType == meteringPointType;
        }

        /// <summary>
        /// A reference to the charge in the Charge table
        /// </summary>
        public int ChargeRowId { get; }

        /// <summary>
        /// If the DefaultChargeLink has an EndDateTime,
        /// it is only applicable for linking when EndDateTime is later than the StartDateTime
        /// </summary>
        public Instant? EndDateTime { get; }
    }
}
