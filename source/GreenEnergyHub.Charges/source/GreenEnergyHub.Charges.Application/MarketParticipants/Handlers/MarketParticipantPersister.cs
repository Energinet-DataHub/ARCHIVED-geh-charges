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
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Application.Persistence;
using GreenEnergyHub.Charges.Domain.Dtos.MarketParticipantsUpdatedEvents;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.GridAreaLinks;
using GreenEnergyHub.Charges.Domain.MarketParticipants;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.Application.MarketParticipants.Handlers
{
    public class MarketParticipantPersister : IMarketParticipantPersister
    {
        private readonly IMarketParticipantRepository _marketParticipantRepository;
        private readonly IGridAreaLinkRepository _gridAreaLinkRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;

        public MarketParticipantPersister(
            IMarketParticipantRepository marketParticipantRepository,
            IGridAreaLinkRepository gridAreaLinkRepository,
            ILoggerFactory loggerFactory,
            IUnitOfWork unitOfWork)
        {
            _marketParticipantRepository = marketParticipantRepository;
            _gridAreaLinkRepository = gridAreaLinkRepository;
            _logger = loggerFactory.CreateLogger(nameof(MarketParticipantPersister));
            _unitOfWork = unitOfWork;
        }

        public async Task PersistAsync(MarketParticipantUpdatedEvent marketParticipantUpdatedEvent)
        {
            ArgumentNullException.ThrowIfNull(marketParticipantUpdatedEvent);
            foreach (var businessProcessRole in marketParticipantUpdatedEvent.BusinessProcessRoles)
            {
                var persistMarketParticipant = await _marketParticipantRepository.SingleOrNullAsync(
                    businessProcessRole,
                    marketParticipantUpdatedEvent.MarketParticipantId).ConfigureAwait(false);

                if (persistMarketParticipant is null)
                {
                    var marketParticipantAlreadyExistWithOtherRole = await _marketParticipantRepository
                        .SingleOrNullAsync(marketParticipantUpdatedEvent.MarketParticipantId).ConfigureAwait(false);
                    if (marketParticipantAlreadyExistWithOtherRole != null)
                    {
                        throw new InvalidOperationException(
                            $"Only 1 market participant with ID '{marketParticipantAlreadyExistWithOtherRole.MarketParticipantId}' is allowed, " +
                            $"the current persisted market participant has role {marketParticipantAlreadyExistWithOtherRole.BusinessProcessRole} " +
                            $"and the new market participant to be created has role {businessProcessRole}");
                    }

                    persistMarketParticipant = await AddMarketParticipantAsync(
                        marketParticipantUpdatedEvent,
                        businessProcessRole).ConfigureAwait(false);
                }
                else
                {
                    UpdateMarketParticipant(
                        marketParticipantUpdatedEvent,
                        persistMarketParticipant);
                }

                await ConnectToGridAreaAsync(marketParticipantUpdatedEvent, persistMarketParticipant).ConfigureAwait(false);
            }

            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
        }

        private void UpdateMarketParticipant(
            MarketParticipantUpdatedEvent marketParticipantUpdatedEvent,
            MarketParticipant existingMarketParticipant)
        {
            existingMarketParticipant.IsActive = marketParticipantUpdatedEvent.IsActive;
            _logger.LogInformation(
                "Market participant with ID '{MarketParticipantId}' and role '{BusinessProcessRole}' " +
                "has changed state",
                existingMarketParticipant.MarketParticipantId,
                existingMarketParticipant.BusinessProcessRole);
        }

        private async Task<MarketParticipant> AddMarketParticipantAsync(
            MarketParticipantUpdatedEvent marketParticipantUpdatedEvent,
            MarketParticipantRole businessProcessRole)
        {
            var marketParticipant = new MarketParticipant(
                marketParticipantUpdatedEvent.ActorId,
                marketParticipantUpdatedEvent.MarketParticipantId,
                marketParticipantUpdatedEvent.IsActive,
                businessProcessRole);

            await _marketParticipantRepository.AddAsync(marketParticipant).ConfigureAwait(false);
            _logger.LogInformation(
                "Market participant with ID '{MarketParticipantId}' and role '{BusinessProcessRole}' " +
                "has been persisted",
                marketParticipant.MarketParticipantId,
                marketParticipant.BusinessProcessRole);
            return marketParticipant;
        }

        private async Task ConnectToGridAreaAsync(
            MarketParticipantUpdatedEvent marketParticipantUpdatedEvent,
            MarketParticipant marketParticipant)
        {
            if (!marketParticipant.BusinessProcessRole.Equals(MarketParticipantRole.GridAccessProvider)) return;

            foreach (var gridAreaId in marketParticipantUpdatedEvent.GridAreas)
            {
                var existingGridAreaLink = await _gridAreaLinkRepository.GetGridAreaOrNullAsync(gridAreaId).ConfigureAwait(false);
                if (existingGridAreaLink is null) return;
                if (existingGridAreaLink.OwnerId == marketParticipant.Id) return;

                existingGridAreaLink.OwnerId = marketParticipant.Id;
                _logger.LogInformation(
                    "GridAreaLink ID '{GridAreaLinkId}' has changed Owner ID to '{OwnerId}'",
                    existingGridAreaLink.Id,
                    existingGridAreaLink.OwnerId);
            }
        }
    }
}
