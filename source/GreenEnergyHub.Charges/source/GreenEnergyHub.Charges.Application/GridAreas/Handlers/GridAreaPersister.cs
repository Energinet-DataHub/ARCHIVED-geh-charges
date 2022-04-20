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
using GreenEnergyHub.Charges.Domain.Dtos.MarketParticipantsChangedEvents;
using GreenEnergyHub.Charges.Domain.GridAreas;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Charges.Application.GridAreas.Handlers
{
    public class GridAreaPersister : IGridAreaPersister
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGridAreaRepository _gridAreaRepository;
        private readonly ILogger _logger;

        public GridAreaPersister(
            IGridAreaRepository gridAreaRepository,
            ILoggerFactory loggerFactory,
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _gridAreaRepository = gridAreaRepository;
            _logger = loggerFactory.CreateLogger(nameof(GridAreaPersister));
        }

        public async Task PersistAsync(GridAreaChangedEvent gridAreaChangedEvent)
        {
            if (gridAreaChangedEvent is null)
                throw new ArgumentNullException(nameof(gridAreaChangedEvent));
            var existingGridArea = await _gridAreaRepository.GetOrNullAsync(
                gridAreaChangedEvent.Id).ConfigureAwait(false);
            if (existingGridArea is null)
            {
                var gridArea = new GridArea(
                    gridAreaChangedEvent.Id,
                    gridAreaChangedEvent.GridAreaId);
                await _gridAreaRepository.AddAsync(gridArea).ConfigureAwait(false);
                _logger.LogInformation(
                    $"GridArea ID {gridArea.Id} has been persisted");
            }
            else
            {
                existingGridArea.GridAccessProviderId = gridAreaChangedEvent.GridAreaId;
                _logger.LogInformation(
                    $"GridArea ID '{existingGridArea.Id}' has changed AreaAccessProviderId to " +
                $"'{existingGridArea.GridAccessProviderId}'");
            }

            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
