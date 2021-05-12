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

#pragma warning disable 8618
namespace GreenEnergyHub.Charges.Domain.Common
{
    // Non-nullable member is uninitialized is ignored
    // Only properties which is allowed to be null is nullable
    // MarketParticipant integrity is null checked by ChargeCommandNullChecker

    /// <summary>
    /// A market participant, e.g. a Grid Access Provider, whom may submit a charge message.
    /// </summary>
    public class MarketParticipant
    {
        /// <summary>
        /// Contains an ID that identifies the Market Participants. In Denmark this would be the GLN or EIC number.
        /// </summary>
        public string Id { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Contains the role a market participant uses when initiating and communicating with Green Energy Hub
        /// about a specific business process, e.g. Grid Access Provider use 'DDM' when creating Charge price lists.
        /// </summary>
        public MarketParticipantRole BusinessProcessRole { get; set; }
    }
}
