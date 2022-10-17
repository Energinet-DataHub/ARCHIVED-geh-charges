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
using Energinet.DataHub.MarketParticipant.Integration.Model.Parsers;
using GreenEnergyHub.Charges.Domain.Dtos.Events;

namespace GreenEnergyHub.Charges.Application.MarketParticipants.Handlers
{
    public class ActorIntegrationEventMapper : IActorIntegrationEventMapper
    {
        private readonly ISharedIntegrationEventParser _sharedIntegrationEventParser;

        public ActorIntegrationEventMapper(ISharedIntegrationEventParser sharedIntegrationEventParser)
        {
            _sharedIntegrationEventParser = sharedIntegrationEventParser;
        }

        public static MarketParticipantUpdatedCommand MapFromActorUpdated(ActorUpdatedIntegrationEvent actorUpdatedIntegrationEvent)
        {
            var status = MarketParticipantStatusMapper.Map(actorUpdatedIntegrationEvent.Status);
            var roles = MarketParticipantRoleMapper
                .MapMany(actorUpdatedIntegrationEvent.BusinessRoles);

            return new MarketParticipantUpdatedCommand(
                actorUpdatedIntegrationEvent.ActorId,
                actorUpdatedIntegrationEvent.ExternalActorId,
                actorUpdatedIntegrationEvent.ActorNumber,
                roles,
                status,
                actorUpdatedIntegrationEvent.ActorMarketRoles
                    .SelectMany(amr => amr.GridAreas)
                        .DistinctBy(o => o.Id)
                        .Select(a => a.Id));
        }

        public MarketParticipantCreatedCommand MapFromActorCreated(byte[] message)
        {
            var actorCreatedIntegrationEvent = (ActorCreatedIntegrationEvent)_sharedIntegrationEventParser.Parse(message);

            var status = MarketParticipantStatusMapper.Map(actorCreatedIntegrationEvent.Status);

            var roles = MarketParticipantRoleMapper
                .MapMany(actorCreatedIntegrationEvent.BusinessRoles);

            return new MarketParticipantCreatedCommand(
                actorCreatedIntegrationEvent.ActorId,
                actorCreatedIntegrationEvent.ActorNumber,
                roles,
                status,
                actorCreatedIntegrationEvent.ActorMarketRoles
                    .SelectMany(amr => amr.GridAreas)
                    .DistinctBy(o => o.Id)
                    .Select(a => a.Id));
        }

        public MarketParticipantStatusChangedCommand MapFromActorStatusChanged(byte[] message)
        {
            var actorStatusChanged = (ActorStatusChangedIntegrationEvent)_sharedIntegrationEventParser.Parse(message);
            var status = MarketParticipantStatusMapper.Map(actorStatusChanged.Status);
            return new MarketParticipantStatusChangedCommand(actorStatusChanged.ActorId, status);
        }

        public MarketParticipantB2CActorIdChangedCommand MapFromActorExternalIdChanged(byte[] message)
        {
            var externalIdChanged = (ActorExternalIdChangedIntegrationEvent)_sharedIntegrationEventParser.Parse(message);
            return new MarketParticipantB2CActorIdChangedCommand(
                externalIdChanged.ActorId,
                externalIdChanged.ExternalActorId);
        }

        public static MarketParticipantGridAreaUpdatedCommand MapFromGridAreaUpdatedIntegrationEvent(
            GridAreaUpdatedIntegrationEvent gridUpdatedIntegrationEvent)
        {
            return new MarketParticipantGridAreaUpdatedCommand(
                gridUpdatedIntegrationEvent.GridAreaId,
                gridUpdatedIntegrationEvent.GridAreaLinkId);
        }
    }
}
