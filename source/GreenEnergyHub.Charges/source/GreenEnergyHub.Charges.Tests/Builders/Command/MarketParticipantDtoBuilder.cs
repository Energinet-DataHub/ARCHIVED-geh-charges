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

namespace GreenEnergyHub.Charges.Tests.Builders.Command
{
    public class MarketParticipantDtoBuilder
    {
        private string _marketParticipantId = string.Empty;
        private Guid _id = Guid.NewGuid();
        private Guid _b2cActorId = Guid.NewGuid();
        private MarketParticipantRole _marketParticipantRole;

        public MarketParticipantDtoBuilder WithMarketParticipantId(string marketParticipantId)
        {
            _marketParticipantId = marketParticipantId;
            return this;
        }

        public MarketParticipantDtoBuilder WithMarketParticipantRole(MarketParticipantRole marketParticipantRole)
        {
            _marketParticipantRole = marketParticipantRole;
            return this;
        }

        public MarketParticipantDto Build()
        {
            return new MarketParticipantDto
            {
                Id = _id,
                MarketParticipantId = _marketParticipantId,
                BusinessProcessRole = _marketParticipantRole,
                B2CActorId = _b2cActorId,
            };
        }
    }
}
