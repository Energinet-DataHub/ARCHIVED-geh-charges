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
using GreenEnergyHub.Charges.Domain.MarketParticipants;

namespace GreenEnergyHub.Charges.Domain.HubSenderMarketParticipant
{
    public class HubSenderMarketParticipant : MarketParticipant
    {
        public HubSenderMarketParticipant(Guid id, string marketParticipantId, bool isActive, IEnumerable<MarketParticipantRole> roles)
            : base(id, marketParticipantId, isActive, roles)
        {
            // TODO BJARKE: Unit test
            var role = roles.Single();
            if (role != MarketParticipantRole.MeteringPointAdministrator)
                throw new ArgumentException($"The hub sender market participant must have the role {SenderRole}.");
        }

        /// <summary>
        /// The role that the hub sender market participant uses when sending outbound documents.
        /// </summary>
        // TODO BJARKE: Replace all Roles.Single() for hub sender in code (after rebase on main)
        public MarketParticipantRole SenderRole => MarketParticipantRole.MeteringPointAdministrator;
    }
}
