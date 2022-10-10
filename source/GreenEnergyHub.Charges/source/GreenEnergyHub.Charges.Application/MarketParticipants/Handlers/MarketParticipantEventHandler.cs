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

using System.Threading.Tasks;
using Energinet.DataHub.MarketParticipant.Integration.Model.Dtos;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.Application.MarketParticipants.Handlers
{
    public class MarketParticipantEventHandler : IMarketParticipantEventHandler
    {
        private readonly IMarketParticipantPersister _marketParticipantPersister;
        private readonly IGridAreaLinkPersister _gridAreaLinkPersister;
        private readonly ILogger _logger;

        public MarketParticipantEventHandler(
            IMarketParticipantPersister marketParticipantPersister,
            IGridAreaLinkPersister gridAreaLinkPersister,
            ILoggerFactory loggerFactory)
        {
            _marketParticipantPersister = marketParticipantPersister;
            _gridAreaLinkPersister = gridAreaLinkPersister;
            _logger = loggerFactory.CreateLogger(nameof(MarketParticipantEventHandler));
        }

        public async Task HandleAsync(BaseIntegrationEvent message)
        {
            var type = message.GetType();
            _logger.LogInformation("Market Participant integration event received of type {MessageType}", type);
            switch (message)
            {
                case ActorUpdatedIntegrationEvent actorEvent:
                    {
                        foreach (var role in actorEvent.BusinessRoles)
                        {
                            _logger.LogInformation(
                                "ActorUpdatedIntegrationEvent Id {id} contains Actor Number {GLN} with role: {role}",
                                actorEvent.Id,
                                actorEvent.ActorNumber,
                                role.ToString());
                        }

                        var marketParticipantUpdatedEvent =
                            MarketParticipantDomainEventMapper.MapFromActorUpdatedIntegrationEvent(actorEvent);

                        foreach (var role in marketParticipantUpdatedEvent.BusinessProcessRoles)
                        {
                            _logger.LogInformation(
                                "ActorUpdatedIntegrationEvent Id {id} has been mapped to Charges' internal " +
                                "MarketParticipantUpdatedEvent for GLN {GLN} with role {role}, which is a valid role in Charges.",
                                actorEvent.Id,
                                marketParticipantUpdatedEvent.MarketParticipantId,
                                role.ToString());
                        }

                        await _marketParticipantPersister
                            .PersistAsync(marketParticipantUpdatedEvent)
                            .ConfigureAwait(false);
                        break;
                    }

                case GridAreaUpdatedIntegrationEvent gridAreaEvent:
                    {
                        _logger.LogInformation(
                            "GridAreaUpdatedIntegrationEvent received with Event Id {Id}, GridAreaId {GridAreaId}, " +
                            "Name {Name}, Code {Code} and GridAreaLinkId {GridAreaLinkId}.",
                            gridAreaEvent.Id,
                            gridAreaEvent.GridAreaId,
                            gridAreaEvent.Name,
                            gridAreaEvent.Code,
                            gridAreaEvent.GridAreaLinkId);
                        var gridAreaUpdatedEvent =
                            MarketParticipantDomainEventMapper.MapFromGridAreaUpdatedIntegrationEvent(gridAreaEvent);
                        await _gridAreaLinkPersister
                            .PersistAsync(gridAreaUpdatedEvent)
                            .ConfigureAwait(false);
                        break;
                    }
            }
        }
    }
}
