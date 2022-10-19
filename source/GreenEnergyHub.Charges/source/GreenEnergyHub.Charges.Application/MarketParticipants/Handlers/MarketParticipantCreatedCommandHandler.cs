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
using System.Collections.Generic;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Domain.Dtos.Events;
using GreenEnergyHub.Charges.Domain.Dtos.SharedDtos;
using GreenEnergyHub.Charges.Domain.GridAreaLinks;
using GreenEnergyHub.Charges.Domain.MarketParticipants;

namespace GreenEnergyHub.Charges.Application.MarketParticipants.Handlers
{
    public class MarketParticipantCreatedCommandHandler : IMarketParticipantCreatedCommandHandler
    {
        private readonly IMarketParticipantRepository _marketParticipantRepository;
        private readonly IGridAreaLinkRepository _gridAreaLinkRepository;

        public MarketParticipantCreatedCommandHandler(
            IMarketParticipantRepository marketParticipantRepository,
            IGridAreaLinkRepository gridAreaLinkRepository)
        {
            _marketParticipantRepository = marketParticipantRepository;
            _gridAreaLinkRepository = gridAreaLinkRepository;
        }

        public async Task HandleAsync(MarketParticipantCreatedCommand command)
        {
            foreach (var role in command.BusinessProcessRoles)
            {
                var marketParticipant = MarketParticipant.Create(
                    command.ActorId,
                    command.MarketParticipantId,
                    command.Status,
                    role);

                await _marketParticipantRepository.AddAsync(marketParticipant).ConfigureAwait(false);

                if (marketParticipant.BusinessProcessRole is MarketParticipantRole.GridAccessProvider)
                {
                    await AddMarketParticipantAsOwnerOfGridAreasAsync(
                            command.GridAreas,
                            marketParticipant.Id)
                        .ConfigureAwait(false);
                }
            }
        }

        private async Task AddMarketParticipantAsOwnerOfGridAreasAsync(IEnumerable<Guid> gridAreas, Guid marketParticipantId)
        {
            foreach (var gridAreaId in gridAreas)
            {
                var existingGridAreaLink = await _gridAreaLinkRepository.GetGridAreaOrNullAsync(gridAreaId).ConfigureAwait(false);
                if (existingGridAreaLink is null) continue;
                if (existingGridAreaLink.OwnerId == marketParticipantId) return;

                existingGridAreaLink.OwnerId = marketParticipantId;
            }
        }
    }
}
