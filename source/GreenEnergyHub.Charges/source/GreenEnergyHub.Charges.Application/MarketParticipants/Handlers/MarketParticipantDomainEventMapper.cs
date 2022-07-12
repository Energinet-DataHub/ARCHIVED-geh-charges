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
using System.Linq;
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using GreenEnergyHub.Charges.Domain.Dtos.GridAreas;
using GreenEnergyHub.Charges.Domain.Dtos.MarketParticipantsUpdatedEvents;
using GreenEnergyHub.Charges.Domain.MarketParticipants;

namespace GreenEnergyHub.Charges.Application.MarketParticipants.Handlers
{
    public static class MarketParticipantDomainEventMapper
    {
        public static MarketParticipantUpdatedEvent MapFromActorUpdatedIntegrationEvent(
            ActorUpdatedIntegrationEvent actorUpdatedIntegrationEvent)
        {
            var isActive = actorUpdatedIntegrationEvent.Status is ActorStatus.Active;

            var rolesUsedInChargesDomain = actorUpdatedIntegrationEvent.BusinessRoles
                .Select(MarketParticipantRoleMapper.Map)
                .Intersect(MarketParticipant._validRoles)
                .ToList();

            if (rolesUsedInChargesDomain.Count > 1)
            {
                throw new InvalidOperationException(
                    $"Only 1 role per market participant with ID '{actorUpdatedIntegrationEvent.ActorNumber}' is allowed, " +
                    $"the current market participant has {rolesUsedInChargesDomain.Count} roles associated in the " +
                    $"integration event with id '{actorUpdatedIntegrationEvent.Id}'");
            }

            return new MarketParticipantUpdatedEvent(
                actorUpdatedIntegrationEvent.ActorId,
                actorUpdatedIntegrationEvent.ExternalActorId,
                actorUpdatedIntegrationEvent.ActorNumber,
                rolesUsedInChargesDomain,
                isActive,
                actorUpdatedIntegrationEvent.ActorMarketRoles.SelectMany(amr => amr.GridAreas.Select(ga => ga.Id)));
        }

        public static GridAreaUpdatedEvent MapFromGridAreaUpdatedIntegrationEvent(
            GridAreaUpdatedIntegrationEvent gridUpdatedIntegrationEvent)
        {
            return new GridAreaUpdatedEvent(
                gridUpdatedIntegrationEvent.GridAreaId,
                gridUpdatedIntegrationEvent.GridAreaLinkId);
        }
    }
}
