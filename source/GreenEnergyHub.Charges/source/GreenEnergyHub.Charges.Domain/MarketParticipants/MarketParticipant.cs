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

namespace GreenEnergyHub.Charges.Domain.MarketParticipants
{
    /// <summary>
    /// A market participant, e.g. a Grid Access Provider, whom may submit a charge message.
    /// </summary>
    public class MarketParticipant
    {
        private readonly List<MarketParticipantRole> _roles;

        public MarketParticipant(Guid id, string marketParticipantId, bool isActive, IEnumerable<MarketParticipantRole> roles)
        {
            Id = id;
            MarketParticipantId = marketParticipantId;
            IsActive = isActive;
            _roles = new(roles);
        }

        // ReSharper disable once UnusedMember.Local - Required by persistence framework
        private MarketParticipant()
        {
            MarketParticipantId = null!;
            _roles = new();
        }

        public Guid Id { get; }

        /// <summary>
        /// The ID that identifies the market participant. In Denmark this would be the GLN number or EIC code.
        /// </summary>
        public string MarketParticipantId { get; }

        /// <summary>
        /// The roles of the market participant.
        /// </summary>
        public IReadOnlyCollection<MarketParticipantRole> Roles => _roles;

        public bool IsActive { get; }
    }
}
