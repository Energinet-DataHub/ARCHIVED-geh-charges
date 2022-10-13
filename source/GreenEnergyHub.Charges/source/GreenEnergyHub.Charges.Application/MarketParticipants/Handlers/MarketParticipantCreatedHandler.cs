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
using GreenEnergyHub.Charges.Domain.MarketParticipants;

namespace GreenEnergyHub.Charges.Application.MarketParticipants.Handlers
{
    public class MarketParticipantCreatedHandler : IMarketParticipantCreatedHandler
    {
        private readonly IMarketParticipantRepository _marketParticipantRepository;

        public MarketParticipantCreatedHandler(
            IMarketParticipantRepository marketParticipantRepository)
        {
            _marketParticipantRepository = marketParticipantRepository;
        }

        public async Task HandleAsync(MarketParticipantCreatedCommand marketParticipantCreatedCommand)
        {
            foreach (var role in marketParticipantCreatedCommand.BusinessProcessRoles)
            {
                var marketParticipant = MarketParticipant.Create(
                    marketParticipantCreatedCommand.ActorId,
                    marketParticipantCreatedCommand.B2CActorId,
                    marketParticipantCreatedCommand.MarketParticipantId,
                    marketParticipantCreatedCommand.Status,
                    role);

                await _marketParticipantRepository.AddAsync(marketParticipant).ConfigureAwait(false);
            }
        }
    }
}
