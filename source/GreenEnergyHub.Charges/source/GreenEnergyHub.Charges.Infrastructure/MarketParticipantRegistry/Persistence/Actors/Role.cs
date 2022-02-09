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

namespace GreenEnergyHub.Charges.Infrastructure.MarketParticipantRegistry.Persistence.Actors
{
    /// <summary>
    /// IMPORTANT: Do not change the literal names as they must correspond with storage.
    /// </summary>
    public enum Role
    {
        /// <summary>
        /// Balance responsible party.
        /// </summary>
        Ddk,

        /// <summary>
        /// Grid access provider.
        /// </summary>
        Ddm,

        /// <summary>
        /// Energy supplier.
        /// (also known as balance power supplier)
        /// </summary>
        Ddq,

        /// <summary>
        /// Imbalance settlement responsible.
        /// </summary>
        Ddx,

        /// <summary>
        /// Metering point administrator.
        /// </summary>
        Ddz,

        /// <summary>
        /// Metered data aggregator.
        /// </summary>
        Dea,

        /// <summary>
        /// System operator.
        /// </summary>
        Ez,

        /// <summary>
        /// Metered data responsible.
        /// </summary>
        Mdr,

        /// <summary>
        /// Administrator.
        /// </summary>
        Fas,

        /// <summary>
        /// Customer access.
        /// </summary>
        Cas,

        /// <summary>
        /// Danish Energy Agency (in danish: Energistyrelsen).
        /// </summary>
        Sts,
    }
}
