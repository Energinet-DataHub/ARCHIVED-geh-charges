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

using System.Linq;
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using GreenEnergyHub.Charges.Domain.Dtos.GridAreas;
using GreenEnergyHub.Charges.Domain.Dtos.MarketParticipantsUpdatedEvents;

namespace GreenEnergyHub.Charges.Application.MarketParticipants.Handlers
{
    public static class MarketParticipantDomainEventMapper
    {
        public static MarketParticipantUpdatedEvent MapFromActorUpdatedIntegrationEvent(
            ActorUpdatedIntegrationEvent actorUpdatedIntegrationEvent)
        {
            var isActive = actorUpdatedIntegrationEvent.Status is ActorStatus.Active;

            var rolesUsedInChargesDomain = actorUpdatedIntegrationEvent.BusinessRoles
                .Select(MarketParticipantRoleMapper.Map).ToList();

            return new MarketParticipantUpdatedEvent(
                actorUpdatedIntegrationEvent.Gln,
                rolesUsedInChargesDomain,
                isActive,
                actorUpdatedIntegrationEvent.GridAreas);
        }

        public static GridAreaUpdatedEvent MapFromGridAreaUpdatedIntegrationEvent(
            GridAreaUpdatedIntegrationEvent gridUpdatedIntegrationEvent)
        {
            return new GridAreaUpdatedEvent(
                gridUpdatedIntegrationEvent.Id,
                gridUpdatedIntegrationEvent.GridAreaId,
                gridUpdatedIntegrationEvent.GridAreaLinkId);
        }
    }
}
