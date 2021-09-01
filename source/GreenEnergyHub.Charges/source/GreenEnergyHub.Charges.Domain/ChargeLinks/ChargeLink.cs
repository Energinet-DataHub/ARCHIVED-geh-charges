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

using System.Collections.Generic;

namespace GreenEnergyHub.Charges.Domain.ChargeLinks
{
    /// <summary>
    /// The link between a metering point and a charge.
    /// </summary>
    public class ChargeLink
    {
        public ChargeLink(
            int chargeRowId,
            int meteringPointRowId,
            IReadOnlyCollection<ChargeLinkOperation> operations,
            IReadOnlyCollection<ChargeLinkPeriodDetails> periodDetails)
        {
            ChargeRowId = chargeRowId;
            MeteringPointRowId = meteringPointRowId;
            Operations = operations;
            PeriodDetails = periodDetails;
        }

        // Temporary workaround to silence EFCore until persistence is finished in upcoming PR
#pragma warning disable 8618
        private ChargeLink()
#pragma warning restore 8618
        {
        }

        /// <summary>
        /// Globally unique identifier of the charge link.
        /// </summary>
        public int? RowId { get; private set; }

        /// <summary>
        /// The charge that is linked to the metering point (<see cref="MeteringPointRowId"/>).
        /// </summary>
        public int ChargeRowId { get; private set; }

        /// <summary>
        /// The metering point that is linked to the charge (<see cref="ChargeRowId"/>).
        /// </summary>
        public int MeteringPointRowId { get; private set; }

        public IReadOnlyCollection<ChargeLinkOperation> Operations { get; private set; }

        public IReadOnlyCollection<ChargeLinkPeriodDetails> PeriodDetails { get; private set; }
    }
}
