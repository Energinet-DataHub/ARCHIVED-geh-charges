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
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.MarketParticipants;

namespace GreenEnergyHub.Charges.TestCore.Builders.Command
{
    public class MarketParticipantBuilder
    {
        private Guid _actorId = Guid.NewGuid();
        private MarketParticipantStatus _status;
        private string _marketParticipantId;
        private MarketParticipantRole _role;
        private string _marketParticipantName;

        public MarketParticipantBuilder()
        {
            _marketParticipantId = Guid.NewGuid().ToString().Substring(0, 35);
            _role = MarketParticipantRole.GridAccessProvider;
            _status = MarketParticipantStatus.Active;
            _marketParticipantName = "mp name";
        }

        public MarketParticipantBuilder WithStatus(MarketParticipantStatus status)
        {
            _status = status;
            return this;
        }

        public MarketParticipantBuilder WithActorId(Guid actorId)
        {
            _actorId = actorId;
            return this;
        }

        public MarketParticipantBuilder WithMarketParticipantId(string marketParticipantId)
        {
            _marketParticipantId = marketParticipantId;
            return this;
        }

        public MarketParticipantBuilder WithRole(MarketParticipantRole role)
        {
            _role = role;
            return this;
        }

        public MarketParticipantBuilder WithName(string marketParticipantName)
        {
            _marketParticipantName = marketParticipantName;
            return this;
        }

        public MarketParticipant Build()
        {
            return MarketParticipant.Create(
                _actorId,
                _marketParticipantId,
                _marketParticipantName,
                _status,
                _role);
        }
    }
}
