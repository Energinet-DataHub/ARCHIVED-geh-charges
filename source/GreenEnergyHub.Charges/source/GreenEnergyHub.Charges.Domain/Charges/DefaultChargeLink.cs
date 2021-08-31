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
        /// The date the charge is applicable for linking.
        /// </summary>
        public Instant ApplicableDate { get; set; }

        /// <summary>
        /// The date the charge is no longer applicable for linking.
        /// </summary>
        public Instant EndDate { get; set; }

        /// <summary>
        /// A reference to the charge in the Charge table
        /// </summary>
        public int ChargeRowId { get; set; }
    }
}
