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
using Energinet.DataHub.Core.App.Common.Abstractions.Actor;
using GreenEnergyHub.Charges.Domain.MarketParticipants;

namespace GreenEnergyHub.Charges.Infrastructure
{
    public class ActorProvider : IActorProvider
    {
        // At the moment the only type used is GLN, but other types must be handled in the future.
        private const string CurrentSupportedIdentificationType = "GLN";
        private readonly IMarketParticipantRepository _marketParticipantRepository;

        public ActorProvider(IMarketParticipantRepository marketParticipantRepository)
        {
            _marketParticipantRepository = marketParticipantRepository;
        }

        public async Task<Actor> GetActorAsync(Guid actorId)
        {
            var mp = await _marketParticipantRepository.SingleOrNullAsync(actorId).ConfigureAwait(false);

            if (mp == null)
                throw new Exception($"no actor found with actorId {actorId}");

            return new Actor(
                mp.Id,
                CurrentSupportedIdentificationType,
                mp.MarketParticipantId,
                mp.BusinessProcessRole.ToString());
        }
    }
}
