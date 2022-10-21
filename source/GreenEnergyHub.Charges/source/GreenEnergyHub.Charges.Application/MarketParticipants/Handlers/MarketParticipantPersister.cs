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
using GreenEnergyHub.Charges.Domain.Dtos.Events;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.GridAreaLinks;
using GreenEnergyHub.Charges.Domain.MarketParticipants;

namespace GreenEnergyHub.Charges.Application.MarketParticipants.Handlers
{
    public class MarketParticipantPersister : IMarketParticipantPersister
    {
        private readonly IMarketParticipantRepository _marketParticipantRepository;
        private readonly IGridAreaLinkRepository _gridAreaLinkRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MarketParticipantPersister(
            IMarketParticipantRepository marketParticipantRepository,
            IGridAreaLinkRepository gridAreaLinkRepository,
            IUnitOfWork unitOfWork)
        {
            _marketParticipantRepository = marketParticipantRepository;
            _gridAreaLinkRepository = gridAreaLinkRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task PersistAsync(MarketParticipantUpdatedCommand marketParticipantUpdatedCommand)
        {
            ArgumentNullException.ThrowIfNull(marketParticipantUpdatedCommand);

            foreach (var businessProcessRole in marketParticipantUpdatedCommand.BusinessProcessRoles)
                await HandleBusinessProcessRoleAsync(marketParticipantUpdatedCommand, businessProcessRole).ConfigureAwait(false);

            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
        }

        private async Task HandleBusinessProcessRoleAsync(
            MarketParticipantUpdatedCommand marketParticipantUpdatedCommand,
            MarketParticipantRole businessProcessRole)
        {
            var marketParticipant = await _marketParticipantRepository.SingleOrNullAsync(
                businessProcessRole, marketParticipantUpdatedCommand.MarketParticipantId).ConfigureAwait(false);

            if (marketParticipant is null)
                marketParticipant = await _marketParticipantRepository.GetByActorIdAsync(marketParticipantUpdatedCommand.ActorId).ConfigureAwait(false);

            if (marketParticipant is null)
            {
                marketParticipant = await CreateMarketParticipantAsync(
                    marketParticipantUpdatedCommand, businessProcessRole).ConfigureAwait(false);
            }
            else
            {
                UpdateMarketParticipant(marketParticipantUpdatedCommand, marketParticipant);
            }

            await ConnectToGridAreaAsync(marketParticipantUpdatedCommand, marketParticipant!).ConfigureAwait(false);
        }

        private async Task<MarketParticipant?> CreateMarketParticipantAsync(
            MarketParticipantUpdatedCommand marketParticipantUpdatedCommand,
            MarketParticipantRole businessProcessRole)
        {
            var marketParticipantAlreadyExistWithOtherRole = await _marketParticipantRepository
                .SingleOrNullAsync(marketParticipantUpdatedCommand.MarketParticipantId).ConfigureAwait(false);

            if (marketParticipantAlreadyExistWithOtherRole != null)
            {
                throw new InvalidOperationException(
                    $"Only 1 market participant with MarketParticipantId '{marketParticipantAlreadyExistWithOtherRole.MarketParticipantId}' is allowed, " +
                    $"the current persisted market participant has role {marketParticipantAlreadyExistWithOtherRole.BusinessProcessRole} " +
                    $"and the new market participant to be created has role {businessProcessRole}");
            }

            var marketParticipant = await AddMarketParticipantAsync(
                marketParticipantUpdatedCommand, businessProcessRole).ConfigureAwait(false);

            return marketParticipant;
        }

        private async Task<MarketParticipant> AddMarketParticipantAsync(
            MarketParticipantUpdatedCommand marketParticipantUpdatedCommand,
            MarketParticipantRole businessProcessRole)
        {
            var marketParticipant = MarketParticipant.Create(
                marketParticipantUpdatedCommand.ActorId,
                marketParticipantUpdatedCommand.MarketParticipantId,
                string.Empty,
                marketParticipantUpdatedCommand.Status,
                businessProcessRole);

            await _marketParticipantRepository.AddAsync(marketParticipant).ConfigureAwait(false);

            return marketParticipant;
        }

        private void UpdateMarketParticipant(
            MarketParticipantUpdatedCommand marketParticipantUpdatedCommand,
            MarketParticipant existingMarketParticipant)
        {
            existingMarketParticipant.Update(
                marketParticipantUpdatedCommand.ActorId,
                marketParticipantUpdatedCommand.B2CActorId,
                marketParticipantUpdatedCommand.Status);
        }

        private async Task ConnectToGridAreaAsync(
            MarketParticipantUpdatedCommand marketParticipantUpdatedCommand,
            MarketParticipant marketParticipant)
        {
            if (!marketParticipant.BusinessProcessRole.Equals(MarketParticipantRole.GridAccessProvider)) return;

            foreach (var gridAreaId in marketParticipantUpdatedCommand.GridAreas)
            {
                var existingGridAreaLink = await _gridAreaLinkRepository.GetGridAreaOrNullAsync(gridAreaId).ConfigureAwait(false);
                if (existingGridAreaLink is null) return;
                if (existingGridAreaLink.OwnerId == marketParticipant.Id) return;

                existingGridAreaLink.OwnerId = marketParticipant.Id;
            }
        }
    }
}
