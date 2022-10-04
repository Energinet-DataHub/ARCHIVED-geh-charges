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
        private readonly IChargesUnitOfWork _chargesUnitOfWork;
        private readonly ILogger _logger;

        public MarketParticipantPersister(
            IMarketParticipantRepository marketParticipantRepository,
            IGridAreaLinkRepository gridAreaLinkRepository,
            ILoggerFactory loggerFactory,
            IChargesUnitOfWork chargesUnitOfWork)
        {
            _marketParticipantRepository = marketParticipantRepository;
            _gridAreaLinkRepository = gridAreaLinkRepository;
            _logger = loggerFactory.CreateLogger(nameof(MarketParticipantPersister));
            _chargesUnitOfWork = chargesUnitOfWork;
        }

        public async Task PersistAsync(MarketParticipantUpdatedEvent marketParticipantUpdatedEvent)
        {
            ArgumentNullException.ThrowIfNull(marketParticipantUpdatedEvent);

            foreach (var businessProcessRole in marketParticipantUpdatedEvent.BusinessProcessRoles)
                await HandleBusinessProcessRoleAsync(marketParticipantUpdatedEvent, businessProcessRole).ConfigureAwait(false);

            await _chargesUnitOfWork.SaveChangesAsync().ConfigureAwait(false);
        }

        private async Task HandleBusinessProcessRoleAsync(
            MarketParticipantUpdatedEvent marketParticipantUpdatedEvent,
            MarketParticipantRole businessProcessRole)
        {
            var marketParticipant = await _marketParticipantRepository.SingleOrNullAsync(
                businessProcessRole, marketParticipantUpdatedEvent.MarketParticipantId).ConfigureAwait(false);

            if (marketParticipant is null)
                marketParticipant = await _marketParticipantRepository.GetByActorIdAsync(marketParticipantUpdatedEvent.ActorId).ConfigureAwait(false);

            if (marketParticipant is null)
            {
                marketParticipant = await CreateMarketParticipantAsync(
                    marketParticipantUpdatedEvent, businessProcessRole).ConfigureAwait(false);
            }
            else
            {
                UpdateMarketParticipant(marketParticipantUpdatedEvent, marketParticipant);
            }

            await ConnectToGridAreaAsync(marketParticipantUpdatedEvent, marketParticipant!).ConfigureAwait(false);
        }

        private async Task<MarketParticipant?> CreateMarketParticipantAsync(
            MarketParticipantUpdatedEvent marketParticipantUpdatedEvent,
            MarketParticipantRole businessProcessRole)
        {
            var marketParticipantAlreadyExistWithOtherRole = await _marketParticipantRepository
                .SingleOrNullAsync(marketParticipantUpdatedEvent.MarketParticipantId).ConfigureAwait(false);

            if (marketParticipantAlreadyExistWithOtherRole != null)
            {
                throw new InvalidOperationException(
                    $"Only 1 market participant with MarketParticipantId '{marketParticipantAlreadyExistWithOtherRole.MarketParticipantId}' is allowed, " +
                    $"the current persisted market participant has role {marketParticipantAlreadyExistWithOtherRole.BusinessProcessRole} " +
                    $"and the new market participant to be created has role {businessProcessRole}");
            }

            var marketParticipant = await AddMarketParticipantAsync(
                marketParticipantUpdatedEvent, businessProcessRole).ConfigureAwait(false);

            return marketParticipant;
        }

        private async Task<MarketParticipant> AddMarketParticipantAsync(
            MarketParticipantUpdatedEvent marketParticipantUpdatedEvent,
            MarketParticipantRole businessProcessRole)
        {
            var marketParticipant = new MarketParticipant(
                Guid.NewGuid(),
                marketParticipantUpdatedEvent.ActorId,
                marketParticipantUpdatedEvent.B2CActorId,
                marketParticipantUpdatedEvent.MarketParticipantId,
                marketParticipantUpdatedEvent.Status,
                businessProcessRole);

            await _marketParticipantRepository.AddAsync(marketParticipant).ConfigureAwait(false);

            _logger.LogInformation(
                "Market participant with MarketParticipantId '{MarketParticipantId}', ActorId '{ActorId}', B2CActorId " +
                "'{B2CActorId}' and role '{BusinessProcessRole}' has been persisted",
                marketParticipant.MarketParticipantId,
                marketParticipant.ActorId,
                marketParticipant.B2CActorId,
                marketParticipant.BusinessProcessRole);

            return marketParticipant;
        }

        private void UpdateMarketParticipant(
            MarketParticipantUpdatedEvent marketParticipantUpdatedEvent,
            MarketParticipant existingMarketParticipant)
        {
            existingMarketParticipant.ActorId = marketParticipantUpdatedEvent.ActorId;
            existingMarketParticipant.B2CActorId = marketParticipantUpdatedEvent.B2CActorId;
            existingMarketParticipant.Status = marketParticipantUpdatedEvent.Status;

            _logger.LogInformation(
                "Market participant with MarketParticipantId '{MarketParticipantId}' " +
                "and role '{BusinessProcessRole}' has changed state",
                existingMarketParticipant.MarketParticipantId,
                existingMarketParticipant.BusinessProcessRole);
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
                    "GridAreaLink ID '{GridAreaLinkId}' has changed Owner ID to '{OwnerId}' " +
                    "with MarketParticipantId {MarketParticipantId} and B2CActorId {B2CActorId}",
                    existingGridAreaLink.Id,
                    existingGridAreaLink.OwnerId,
                    marketParticipant.MarketParticipantId,
                    marketParticipant.B2CActorId);
            }
        }
    }
}
