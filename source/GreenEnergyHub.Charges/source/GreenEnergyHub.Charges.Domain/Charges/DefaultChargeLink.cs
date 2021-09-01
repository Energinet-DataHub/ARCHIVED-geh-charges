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

namespace GreenEnergyHub.Charges.Domain.Charges
{
    // Logically there is a MeteringPointType attached to the DefaultChargeLink, but atm. It is not used.
    public class DefaultChargeLink
    {
        /// <summary>
        /// The date the MeteringPoint is created. And the date the charge link should be applied from
        /// if it is greater than the default charge link StartDateTime.
        /// </summary>
        private readonly Instant _meteringPointCreatedDateTime;

        /// <summary>
        /// The default start date the charge link is applicable from.
        /// </summary>
        private readonly Instant _settingStartDateTime;

        /// <summary>
        /// The date the charge is no longer applicable for linking.
        /// The value will be provided by the default charge link setting.
        /// </summary>
        private readonly Instant? _settingEndDateTime;

        public DefaultChargeLink(
            Instant meteringPointCreatedDateTime,
            Instant settingStartDateTime,
            Instant? settingEndDateTime,
            int chargeRowId)
        {
            _meteringPointCreatedDateTime = meteringPointCreatedDateTime;
            _settingStartDateTime = settingStartDateTime;
            _settingEndDateTime = settingEndDateTime;
            ChargeRowId = chargeRowId;
        }

        /// <summary>
        /// The starting date is determined by the latest date when comparing meteringPointCreatedDateTime and
        /// SettingStartDateTime. It is used to dictate when the charge link should start from.
        /// </summary>
        public Instant StartDateTime =>
            _settingStartDateTime > _meteringPointCreatedDateTime ? _settingStartDateTime : _meteringPointCreatedDateTime;

        /// <summary>
        /// If the charge setting has a EndDateTime, it is only applicable for link if it is lesser or equal too StartDateTime.
        /// </summary>
        public bool ApplicableForLinking => _settingEndDateTime == null || _settingEndDateTime.Value > StartDateTime;

        /// <summary>
        /// A reference to the charge in the Charge table
        /// </summary>
        public int ChargeRowId { get; }
    }
}
