﻿// Copyright 2020 Energinet DataHub A/S
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
using GreenEnergyHub.Charges.Domain.MarketParticipants;

namespace GreenEnergyHub.Charges.Application.MarketParticipants.Handlers
{
    public class GridAreaRemovedFromMarketParticipantCommandHandler : IGridAreaRemovedFromMarketParticipantCommandHandler
    {
        private readonly IGridAreaLinkRepository _gridAreaLinkRepository;

        public GridAreaRemovedFromMarketParticipantCommandHandler(
            IGridAreaLinkRepository gridAreaLinkRepository)
        {
            _gridAreaLinkRepository = gridAreaLinkRepository;
        }

        public async Task HandleAsync(GridAreaRemovedFromMarketParticipantCommand command)
        {
            var gridArea = await _gridAreaLinkRepository.GetGridAreaOrNullAsync(command.GridAreaId).ConfigureAwait(false);
            if (gridArea == null)
            {
                throw new InvalidOperationException(
                    $"Could not removed grid area from market participant. Grid area not found by grid area id: {command.GridAreaId}");
            }

            gridArea.RemoveOwner();
        }
    }
}
