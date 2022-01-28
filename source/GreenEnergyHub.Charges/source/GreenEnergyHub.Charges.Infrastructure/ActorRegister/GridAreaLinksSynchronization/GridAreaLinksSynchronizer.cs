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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GreenEnergyHub.Charges.Infrastructure.ActorRegister.Persistence;
using GreenEnergyHub.Charges.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GreenEnergyHub.Charges.Infrastructure.ActorRegister.GridAreaLinksSynchronization
{
    public class GridAreaLinksSynchronizer : IGridAreaLinksSynchronizer
    {
        private readonly IActorRegister _actorRegister;
        private readonly IChargesDatabaseContext _chargesDatabaseContext;

        public GridAreaLinksSynchronizer(IActorRegister actorRegister, IChargesDatabaseContext chargesDatabaseContext)
        {
            _actorRegister = actorRegister;
            _chargesDatabaseContext = chargesDatabaseContext;
        }

        public async Task SynchronizeAsync()
        {
            var linksInRegister = await _actorRegister.GridAreaLinks.ToListAsync().ConfigureAwait(false);
            var links = await _chargesDatabaseContext.GridAreaLinks.ToListAsync().ConfigureAwait(false);

            foreach (var linkInRegister in linksInRegister)
                await AddOrUpdateGridAreaLinkAsync(links, linkInRegister).ConfigureAwait(false);

            await _chargesDatabaseContext.SaveChangesAsync().ConfigureAwait(false);
        }

        private async Task AddOrUpdateGridAreaLinkAsync(List<GridAreaLink> links, Persistence.GridAreaLinks.GridAreaLink linkInRegister)
        {
            var link = links.SingleOrDefault(a => a.Id == linkInRegister.Id);

            if (link == null)
                await AddGridAreaLinkAsync(linkInRegister).ConfigureAwait(false);
            else
                UpdateGridAreaLink(linkInRegister, link);
        }

        private static void UpdateGridAreaLink(Persistence.GridAreaLinks.GridAreaLink linkInRegister, GridAreaLink link)
        {
            link.GridAreaId = linkInRegister.GridAreaId;
        }

        private async Task AddGridAreaLinkAsync(Persistence.GridAreaLinks.GridAreaLink linkInRegister)
        {
            var link = new GridAreaLink(linkInRegister.Id, linkInRegister.GridAreaId);
            await _chargesDatabaseContext.GridAreaLinks.AddAsync(link).ConfigureAwait(false);
        }
    }
}
