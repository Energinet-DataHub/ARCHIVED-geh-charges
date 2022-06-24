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
using GreenEnergyHub.Charges.Domain.Dtos.GridAreas;
using GreenEnergyHub.Charges.Domain.GridAreaLinks;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.Application.MarketParticipants.Handlers
{
    public class GridAreaLinkPersister : IGridAreaLinkPersister
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGridAreaLinkRepository _gridAreaLinkRepository;
        private readonly ILogger _logger;

        public GridAreaLinkPersister(
            IGridAreaLinkRepository gridAreaLinkRepository,
            ILoggerFactory loggerFactory,
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _gridAreaLinkRepository = gridAreaLinkRepository;
            _logger = loggerFactory.CreateLogger(nameof(GridAreaLinkPersister));
        }

        public async Task PersistAsync(GridAreaUpdatedEvent gridAreaUpdatedEvent)
        {
            ArgumentNullException.ThrowIfNull(gridAreaUpdatedEvent);
            await PersistGridAreaLinkAsync(gridAreaUpdatedEvent).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
        }

        private async Task PersistGridAreaLinkAsync(GridAreaUpdatedEvent gridAreaUpdatedEvent)
        {
            var existingGridAreaLink = await _gridAreaLinkRepository
                .GetOrNullAsync(gridAreaUpdatedEvent.GridAreaLinkId).ConfigureAwait(false);

            if (existingGridAreaLink is null)
            {
                var gridAreaLink = new GridAreaLink(gridAreaUpdatedEvent.GridAreaLinkId, gridAreaUpdatedEvent.GridAreaId, null);

                await _gridAreaLinkRepository.AddAsync(gridAreaLink).ConfigureAwait(false);
                _logger.LogInformation(
                    "GridAreaLink ID {GridAreaLink} for GridArea ID {GridAreaId} has been persisted",
                    gridAreaLink.Id,
                    gridAreaLink.GridAreaId);
            }
            else
            {
                if (existingGridAreaLink.GridAreaId == gridAreaUpdatedEvent.GridAreaId)
                {
                    return;
                }

                existingGridAreaLink.GridAreaId = gridAreaUpdatedEvent.GridAreaId;
                _logger.LogInformation(
                    "GridAreaLink ID {GridAreaLink} with {OwnerId} has changed GridArea ID to {GridAreaId}",
                    existingGridAreaLink.Id,
                    existingGridAreaLink.OwnerId,
                    existingGridAreaLink.GridAreaId);
            }
        }
    }
}
