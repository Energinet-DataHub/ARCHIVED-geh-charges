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

namespace GreenEnergyHub.Charges.Tests.Builders.Command
{
    public class MarketParticipantBuilder
    {
        private Guid _id = Guid.NewGuid();
        private bool _isActive;
        private string _marketParticipantId;
        private MarketParticipantRole _role;

        public MarketParticipantBuilder()
        {
            _marketParticipantId = Guid.NewGuid().ToString().Substring(0, 35);
            _role = MarketParticipantRole.GridAccessProvider;
            _isActive = true;
        }

        public MarketParticipantBuilder WithIsActive(bool isActive)
        {
            _isActive = isActive;
            return this;
        }

        public MarketParticipantBuilder WithId(Guid id)
        {
            _id = id;
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

        public MarketParticipant Build()
        {
            return new(_id, _marketParticipantId, _isActive, _role);
        }
    }
}
