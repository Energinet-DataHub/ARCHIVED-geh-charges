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
using GreenEnergyHub.Charges.Domain.Dtos.Events;
using GreenEnergyHub.Charges.Domain.GridAreaLinks;

namespace GreenEnergyHub.Charges.Application.MarketParticipants.Handlers
{
    public class GridAreaLinkPersister : IGridAreaLinkPersister
    {
        private readonly IGridAreaLinkRepository _gridAreaLinkRepository;

        public GridAreaLinkPersister(IGridAreaLinkRepository gridAreaLinkRepository)
        {
            _gridAreaLinkRepository = gridAreaLinkRepository;
        }

        public async Task PersistAsync(MarketParticipantGridAreaUpdatedCommand marketParticipantGridAreaUpdatedCommand)
        {
            ArgumentNullException.ThrowIfNull(marketParticipantGridAreaUpdatedCommand);
            await PersistGridAreaLinkAsync(marketParticipantGridAreaUpdatedCommand).ConfigureAwait(false);
        }

        private async Task PersistGridAreaLinkAsync(MarketParticipantGridAreaUpdatedCommand marketParticipantGridAreaUpdatedCommand)
        {
            var existingGridAreaLink = await _gridAreaLinkRepository
                .GetOrNullAsync(marketParticipantGridAreaUpdatedCommand.GridAreaLinkId).ConfigureAwait(false);

            if (existingGridAreaLink is null)
            {
                var gridAreaLink = new GridAreaLink(marketParticipantGridAreaUpdatedCommand.GridAreaLinkId, marketParticipantGridAreaUpdatedCommand.GridAreaId, null);

                await _gridAreaLinkRepository.AddAsync(gridAreaLink).ConfigureAwait(false);
            }
            else
            {
                if (existingGridAreaLink.GridAreaId == marketParticipantGridAreaUpdatedCommand.GridAreaId)
                {
                    return;
                }

                existingGridAreaLink.GridAreaId = marketParticipantGridAreaUpdatedCommand.GridAreaId;
            }
        }
    }
}
