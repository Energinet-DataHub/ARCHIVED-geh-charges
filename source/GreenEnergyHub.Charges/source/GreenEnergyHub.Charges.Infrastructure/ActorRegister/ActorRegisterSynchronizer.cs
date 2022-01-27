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

using System.Threading.Tasks;
using GreenEnergyHub.Charges.Infrastructure.ActorRegister.GridAreaLinksSynchronization;
using GreenEnergyHub.Charges.Infrastructure.ActorRegister.GridAreasSynchronization;
using GreenEnergyHub.Charges.Infrastructure.ActorRegister.MarketParticipantsSynchronization;

namespace GreenEnergyHub.Charges.Infrastructure.ActorRegister
{
    public class ActorRegisterSynchronizer : IActorRegisterSynchronizer
    {
        private readonly IMarketParticipantsSynchronizer _marketParticipantsSynchronizer;
        private readonly IGridAreasSynchronizer _gridAreasSynchronizer;
        private readonly IGridAreaLinksSynchronizer _gridAreaLinksSynchronizer;

        public ActorRegisterSynchronizer(IMarketParticipantsSynchronizer marketParticipantsSynchronizer, IGridAreasSynchronizer gridAreasSynchronizer, IGridAreaLinksSynchronizer gridAreaLinksSynchronizer)
        {
            _marketParticipantsSynchronizer = marketParticipantsSynchronizer;
            _gridAreasSynchronizer = gridAreasSynchronizer;
            _gridAreaLinksSynchronizer = gridAreaLinksSynchronizer;
        }

        public async Task SynchronizeAsync()
        {
            await _marketParticipantsSynchronizer.SynchronizeAsync().ConfigureAwait(false);
            await _gridAreasSynchronizer.SynchronizeAsync().ConfigureAwait(false);
            await _gridAreaLinksSynchronizer.SynchronizeAsync().ConfigureAwait(false);
        }
    }
}
