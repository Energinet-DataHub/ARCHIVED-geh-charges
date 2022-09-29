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
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;

namespace GreenEnergyHub.Charges.Application.MarketParticipants.Handlers
{
    public static class MarketParticipantStatusMapper
    {
        public static MarketParticipantStatus MapActorStatusToMarketParticipantStatus(ActorStatus status)
        {
            return status switch
            {
                ActorStatus.New => MarketParticipantStatus.New,
                ActorStatus.Active => MarketParticipantStatus.Active,
                ActorStatus.Inactive => MarketParticipantStatus.Inactive,
                ActorStatus.Passive => MarketParticipantStatus.Passive,
                ActorStatus.Deleted => MarketParticipantStatus.Deleted,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
            };
        }
    }
}
